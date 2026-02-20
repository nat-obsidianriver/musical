using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musical.Api.Data;
using Musical.Api.Dtos;
using Musical.Core.Models;

namespace Musical.Api.Controllers;

[ApiController]
[Route("api/scores/{scoreId}/annotations")]
public class AnnotationsController(MusicalDbContext db) : ControllerBase
{
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
                a.PositionX, a.PositionY, a.CreatedAt))
            .ToListAsync();

        return Ok(annotations);
    }

    [HttpPost]
    public async Task<ActionResult<AnnotationDto>> Create(
        int scoreId,
        CreateAnnotationRequest request)
    {
        var scoreExists = await db.Scores.AnyAsync(s => s.Id == scoreId);
        if (!scoreExists)
            return NotFound();

        if (string.IsNullOrWhiteSpace(request.AuthorName))
            return BadRequest("AuthorName is required.");

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Content is required.");

        var annotation = new Annotation
        {
            ScoreId = scoreId,
            AuthorName = request.AuthorName.Trim(),
            Content = request.Content.Trim(),
            PositionX = Math.Clamp(request.PositionX, 0, 100),
            PositionY = Math.Clamp(request.PositionY, 0, 100),
        };

        db.Annotations.Add(annotation);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByScore), new { scoreId },
            new AnnotationDto(annotation.Id, annotation.ScoreId, annotation.AuthorName,
                annotation.Content, annotation.PositionX, annotation.PositionY,
                annotation.CreatedAt));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int scoreId, int id)
    {
        var annotation = await db.Annotations
            .FirstOrDefaultAsync(a => a.Id == id && a.ScoreId == scoreId);

        if (annotation is null)
            return NotFound();

        db.Annotations.Remove(annotation);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
