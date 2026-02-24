using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
        if (!scoreExists) return NotFound();

        var q = from a in db.Annotations
                where a.ScoreId == scoreId
                join f in db.Folders on a.FolderId equals f.Id into fj
                from f in fj.DefaultIfEmpty()
                orderby a.CreatedAt
                select new AnnotationDto(
                    a.Id, a.ScoreId, a.AuthorName, a.Content,
                    a.PositionX, a.PositionY, a.PositionXEnd, a.PositionYEnd,
                    a.AttachmentFileName, a.CreatedAt,
                    a.UserId, a.FolderId,
                    f != null ? f.Name : null,
                    f != null ? f.Color : null);

        return Ok(await q.ToListAsync());
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AnnotationDto>> Create(
        int scoreId,
        [FromForm] CreateAnnotationForm form)
    {
        var scoreExists = await db.Scores.AnyAsync(s => s.Id == scoreId);
        if (!scoreExists) return NotFound();

        if (string.IsNullOrWhiteSpace(form.Content))
            return BadRequest("Content is required.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var displayName = User.FindFirstValue(ClaimTypes.Name) ?? "User";

        // Resolve folder: use provided FolderId, else the user's oldest (default) folder
        int? folderId = form.FolderId;
        string? folderName = null;
        string? folderColor = null;

        if (folderId.HasValue)
        {
            var folder = await db.Folders.FindAsync(folderId.Value);
            if (folder is null) return BadRequest("Folder not found.");
            if (folder.UserId != userId && !User.IsInRole("Admin")) return Forbid();
            folderName = folder.Name;
            folderColor = folder.Color;
        }
        else
        {
            var defaultFolder = await db.Folders
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.CreatedAt)
                .FirstOrDefaultAsync();
            if (defaultFolder is not null)
            {
                folderId = defaultFolder.Id;
                folderName = defaultFolder.Name;
                folderColor = defaultFolder.Color;
            }
        }

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
            UserId = userId,
            FolderId = folderId,
            AuthorName = displayName,
            Content = form.Content.Trim(),
            PositionX = Math.Clamp(form.PositionX, 0, 100),
            PositionY = Math.Clamp(form.PositionY, 0, 100),
            PositionXEnd = form.PositionXEnd.HasValue ? Math.Clamp(form.PositionXEnd.Value, 0, 100) : null,
            PositionYEnd = form.PositionYEnd.HasValue ? Math.Clamp(form.PositionYEnd.Value, 0, 100) : null,
            AttachmentFileName = attachmentFileName,
        };

        db.Annotations.Add(annotation);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByScore), new { scoreId },
            new AnnotationDto(annotation.Id, annotation.ScoreId, annotation.AuthorName,
                annotation.Content, annotation.PositionX, annotation.PositionY,
                annotation.PositionXEnd, annotation.PositionYEnd,
                annotation.AttachmentFileName, annotation.CreatedAt,
                annotation.UserId, annotation.FolderId, folderName, folderColor));
    }

    [HttpGet("{id}/attachment")]
    public async Task<IActionResult> GetAttachment(int scoreId, int id)
    {
        var annotation = await db.Annotations
            .FirstOrDefaultAsync(a => a.Id == id && a.ScoreId == scoreId);

        if (annotation is null || annotation.AttachmentFileName is null) return NotFound();

        var filePath = Path.Combine(env.ContentRootPath, "uploads", annotation.AttachmentFileName);
        if (!System.IO.File.Exists(filePath)) return NotFound();

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
    [Authorize]
    public async Task<IActionResult> Delete(int scoreId, int id)
    {
        var annotation = await db.Annotations
            .FirstOrDefaultAsync(a => a.Id == id && a.ScoreId == scoreId);

        if (annotation is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (annotation.UserId != userId && !isAdmin) return Forbid();

        if (annotation.AttachmentFileName is not null)
        {
            var filePath = Path.Combine(env.ContentRootPath, "uploads", annotation.AttachmentFileName);
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
        }

        db.Annotations.Remove(annotation);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
