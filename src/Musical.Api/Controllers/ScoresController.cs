using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musical.Api.Data;
using Musical.Api.Dtos;
using Musical.Core.Models;

namespace Musical.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoresController(MusicalDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    [HttpGet]
    public async Task<IEnumerable<ScoreDto>> GetAll() =>
        await db.Scores
            .OrderByDescending(s => s.UploadedAt)
            .Select(s => new ScoreDto(
                s.Id, s.Title, s.Composer, s.Description,
                s.ImageFileName, s.UploadedAt,
                s.Annotations.Count))
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<ScoreDto>> GetById(int id)
    {
        var score = await db.Scores
            .Include(s => s.Annotations)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (score is null)
            return NotFound();

        return new ScoreDto(
            score.Id, score.Title, score.Composer, score.Description,
            score.ImageFileName, score.UploadedAt,
            score.Annotations.Count);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ScoreDto>> Create(
        [FromForm] CreateScoreRequest request,
        IFormFile image)
    {
        if (image is null || image.Length == 0)
            return BadRequest("An image file is required.");

        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest($"Allowed image types: {string.Join(", ", AllowedExtensions)}");

        var uploadsDir = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var stream = System.IO.File.Create(filePath))
            await image.CopyToAsync(stream);

        var score = new Score
        {
            Title = request.Title,
            Composer = request.Composer,
            Description = request.Description,
            ImageFileName = fileName,
        };

        db.Scores.Add(score);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = score.Id },
            new ScoreDto(score.Id, score.Title, score.Composer, score.Description,
                score.ImageFileName, score.UploadedAt, 0));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var score = await db.Scores.FindAsync(id);
        if (score is null)
            return NotFound();

        var filePath = Path.Combine(env.ContentRootPath, "uploads", score.ImageFileName);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        db.Scores.Remove(score);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Serve uploaded images
    [HttpGet("{id}/image")]
    public async Task<IActionResult> GetImage(int id)
    {
        var score = await db.Scores.FindAsync(id);
        if (score is null)
            return NotFound();

        var filePath = Path.Combine(env.ContentRootPath, "uploads", score.ImageFileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var ext = Path.GetExtension(score.ImageFileName).TrimStart('.').ToLowerInvariant();
        var contentType = ext switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, contentType);
    }
}
