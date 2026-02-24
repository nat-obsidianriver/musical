using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musical.Api.Data;
using Musical.Api.Dtos;
using Musical.Core.Models;

namespace Musical.Api.Controllers;

[ApiController]
[Route("api/folders")]
[Authorize]
public class FoldersController(MusicalDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsAdmin  => User.IsInRole("Admin");

    [HttpGet]
    public async Task<IEnumerable<FolderDto>> GetAll()
    {
        var query = db.Folders.AsQueryable();
        if (!IsAdmin)
            query = query.Where(f => f.UserId == UserId);

        return await query
            .OrderBy(f => f.Name)
            .Select(f => new FolderDto(
                f.Id, f.Name, f.Description, f.Color, f.IsMasked,
                f.UserId, f.UserDisplayName,
                f.Scores.Count, f.CreatedAt))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FolderDto>> GetById(int id)
    {
        var folder = await db.Folders.Include(f => f.Scores)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (folder is null) return NotFound();
        if (!IsAdmin && folder.UserId != UserId) return Forbid();

        return new FolderDto(folder.Id, folder.Name, folder.Description, folder.Color,
            folder.IsMasked, folder.UserId, folder.UserDisplayName,
            folder.Scores.Count, folder.CreatedAt);
    }

    [HttpPost]
    public async Task<ActionResult<FolderDto>> Create(CreateFolderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Folder name is required.");

        var displayName = User.FindFirstValue(ClaimTypes.Name) ?? "User";
        var folder = new Folder
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#87CEEB" : request.Color,
            UserId = UserId,
            UserDisplayName = displayName
        };

        db.Folders.Add(folder);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = folder.Id },
            new FolderDto(folder.Id, folder.Name, folder.Description, folder.Color,
                folder.IsMasked, folder.UserId, folder.UserDisplayName, 0, folder.CreatedAt));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FolderDto>> Update(int id, UpdateFolderRequest request)
    {
        var folder = await db.Folders.Include(f => f.Scores)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (folder is null) return NotFound();
        if (!IsAdmin && folder.UserId != UserId) return Forbid();

        folder.Name        = request.Name.Trim();
        folder.Description = request.Description?.Trim();
        folder.Color       = string.IsNullOrWhiteSpace(request.Color) ? folder.Color : request.Color;
        folder.IsMasked    = request.IsMasked;

        await db.SaveChangesAsync();

        return new FolderDto(folder.Id, folder.Name, folder.Description, folder.Color,
            folder.IsMasked, folder.UserId, folder.UserDisplayName,
            folder.Scores.Count, folder.CreatedAt);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var folder = await db.Folders.FindAsync(id);
        if (folder is null) return NotFound();
        if (!IsAdmin && folder.UserId != UserId) return Forbid();

        db.Folders.Remove(folder);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
