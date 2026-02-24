using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musical.Api.Data;
using Musical.Api.Dtos;
using Musical.Api.Models;
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
            .Join(db.Users,
                f => f.UserId,
                u => u.Id,
                (f, u) => new FolderDto(
                    f.Id, f.Name, f.Description, f.Color, f.IsMasked,
                    f.UserId, f.UserDisplayName,
                    u.Bio, u.HeadshotFileName,
                    f.Scores.Count, f.CreatedAt))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FolderDto>> GetById(int id)
    {
        var result = await db.Folders
            .Where(f => f.Id == id)
            .Join(db.Users,
                f => f.UserId,
                u => u.Id,
                (f, u) => new { f, u })
            .FirstOrDefaultAsync();

        if (result is null) return NotFound();
        if (!IsAdmin && result.f.UserId != UserId) return Forbid();

        var f = result.f;
        var u = result.u;
        return new FolderDto(f.Id, f.Name, f.Description, f.Color, f.IsMasked,
            f.UserId, f.UserDisplayName, u.Bio, u.HeadshotFileName,
            f.Scores.Count, f.CreatedAt);
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

        var user = await db.Users.FindAsync(UserId);
        return CreatedAtAction(nameof(GetById), new { id = folder.Id },
            new FolderDto(folder.Id, folder.Name, folder.Description, folder.Color,
                folder.IsMasked, folder.UserId, folder.UserDisplayName,
                user?.Bio, user?.HeadshotFileName, 0, folder.CreatedAt));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FolderDto>> Update(int id, UpdateFolderRequest request)
    {
        var result = await db.Folders
            .Where(f => f.Id == id)
            .Join(db.Users, f => f.UserId, u => u.Id, (f, u) => new { f, u })
            .FirstOrDefaultAsync();

        if (result is null) return NotFound();
        if (!IsAdmin && result.f.UserId != UserId) return Forbid();

        result.f.Name        = request.Name.Trim();
        result.f.Description = request.Description?.Trim();
        result.f.Color       = string.IsNullOrWhiteSpace(request.Color) ? result.f.Color : request.Color;
        result.f.IsMasked    = request.IsMasked;

        await db.SaveChangesAsync();

        return new FolderDto(result.f.Id, result.f.Name, result.f.Description, result.f.Color,
            result.f.IsMasked, result.f.UserId, result.f.UserDisplayName,
            result.u.Bio, result.u.HeadshotFileName,
            result.f.Scores.Count, result.f.CreatedAt);
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
