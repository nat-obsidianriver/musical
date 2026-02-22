using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musical.Api.Data;
using Musical.Api.Dtos;
using Musical.Core.Models;

namespace Musical.Api.Controllers;

[ApiController]
[Route("api/scores/{scoreId}/annotations")]
public class AnnotationsController(MusicalDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AnnotationDto>>> GetByScore(int scoreId)
    {
        var scoreExists = await db.Scores.AnyAsync(s => s.Id == scoreId);
        if (!scoreExists)
            return NotFound();

        var annotations = await db.Annotations
            .Where(a => a.ScoreId == scoreId)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AnnotationDto(
                a.Id, a.ScoreId, a.AuthorName, a.Content,
                a.PositionX, a.PositionY,
                a.PositionXEnd, a.PositionYEnd,
                a.AttachmentFileName,
                a.CreatedAt))
            .ToListAsync();

        return Ok(annotations);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AnnotationDto>> Create(
        int scoreId,
        [FromForm] CreateAnnotationForm form)
    {
        var scoreExists = await db.Scores.AnyAsync(s => s.Id == scoreId);
        if (!scoreExists)
            return NotFound();

        if (string.IsNullOrWhiteSpace(form.AuthorName))
            return BadRequest("AuthorName is required.");

        if (string.IsNullOrWhiteSpace(form.Content))
            return BadRequest("Content is required.");

        string? attachmentFileName = null;
        if (form.Attachment is { Length: > 0 })
        {
            var ext = Path.GetExtension(form.Attachment.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                return BadRequest("Attachment must be an image (jpg, png, gif, webp).");

            var uploadsDir = Path.Combine(env.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir);
            attachmentFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, attachmentFileName);
            await using var stream = System.IO.File.Create(filePath);
            await form.Attachment.CopyToAsync(stream);
        }

        var annotation = new Annotation
        {
            ScoreId = scoreId,
            AuthorName = form.AuthorName.Trim(),
            Content = form.Content.Trim(),
            PositionX = Math.Clamp(form.PositionX, 0, 100),
            PositionY = Math.Clamp(form.PositionY, 0, 100),
            PositionXEnd = form.PositionXEnd.HasValue ? Math.Clamp(form.PositionXEnd.Value, 0, 100) : null,
            PositionYEnd = form.PositionYEnd.HasValue ? Math.Clamp(form.PositionYEnd.Value, 0, 100) : null,
            AttachmentFileName = attachmentFileName,
        };

        db.Annotations.Add(annotation);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByScore), new { scoreId }, ToDto(annotation));
    }

    [HttpGet("{id}/attachment")]
    public async Task<IActionResult> GetAttachment(int scoreId, int id)
    {
        var annotation = await db.Annotations
            .FirstOrDefaultAsync(a => a.Id == id && a.ScoreId == scoreId);

        if (annotation is null || annotation.AttachmentFileName is null)
            return NotFound();

        var filePath = Path.Combine(env.ContentRootPath, "uploads", annotation.AttachmentFileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var ext = Path.GetExtension(annotation.AttachmentFileName).TrimStart('.').ToLowerInvariant();
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int scoreId, int id)
    {
        var annotation = await db.Annotations
            .FirstOrDefaultAsync(a => a.Id == id && a.ScoreId == scoreId);

        if (annotation is null)
            return NotFound();

        if (annotation.AttachmentFileName is not null)
        {
            var filePath = Path.Combine(env.ContentRootPath, "uploads", annotation.AttachmentFileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        db.Annotations.Remove(annotation);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static AnnotationDto ToDto(Annotation a) => new(
        a.Id, a.ScoreId, a.AuthorName, a.Content,
        a.PositionX, a.PositionY,
        a.PositionXEnd, a.PositionYEnd,
        a.AttachmentFileName,
        a.CreatedAt);
}
