using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musical.Api.Data;
using Musical.Core.Models;

namespace Musical.Api.Controllers;

[ApiController]
[Route("api/site-content")]
public class SiteContentController(MusicalDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await db.SiteContents
            .Select(sc => new { sc.Slug, sc.Title, sc.Content, sc.UpdatedAt })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Get(string slug)
    {
        var item = await db.SiteContents.FirstOrDefaultAsync(sc => sc.Slug == slug);
        if (item is null) return NotFound();
        return Ok(new { item.Slug, item.Title, item.Content, item.UpdatedAt });
    }

    [HttpPut("{slug}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string slug, [FromBody] SiteContentUpdateDto dto)
    {
        var item = await db.SiteContents.FirstOrDefaultAsync(sc => sc.Slug == slug);
        if (item is null) return NotFound();

        item.Title = dto.Title;
        item.Content = dto.Content;
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(new { item.Slug, item.Title, item.Content, item.UpdatedAt });
    }
}

public record SiteContentUpdateDto(string Title, string Content);
