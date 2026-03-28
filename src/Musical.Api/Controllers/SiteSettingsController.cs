using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musical.Api.Data;
using Musical.Core.Models;

namespace Musical.Api.Controllers;

[ApiController]
[Route("api/site-settings")]
public class SiteSettingsController(MusicalDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private static readonly string[] AllowedVideoExtensions = [".mp4", ".webm", ".ogg"];

    [HttpGet("{key}")]
    public async Task<ActionResult<SiteSettingDto>> Get(string key)
    {
        var setting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
            return NotFound();

        return new SiteSettingDto { Key = setting.Key, Value = setting.Value };
    }

    [HttpPut("{key}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SiteSettingDto>> Upsert(string key, [FromBody] SiteSettingUpdateDto dto)
    {
        var setting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            setting = new SiteSetting { Key = key, Value = dto.Value, UpdatedAt = DateTime.UtcNow };
            db.SiteSettings.Add(setting);
        }
        else
        {
            setting.Value = dto.Value;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return new SiteSettingDto { Key = setting.Key, Value = setting.Value };
    }

    [HttpPost("tutorial-video/upload")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(200_000_000)]
    public async Task<ActionResult<SiteSettingDto>> UploadVideo(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedVideoExtensions.Contains(ext))
            return BadRequest($"Only {string.Join(", ", AllowedVideoExtensions)} files are allowed.");

        var uploadsDir = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        // Remove old uploaded video if exists
        var existing = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "tutorial-video-file");
        if (existing is not null)
        {
            var oldPath = Path.Combine(uploadsDir, existing.Value);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        var fileName = $"tutorial-video{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Save file reference
        if (existing is null)
        {
            db.SiteSettings.Add(new SiteSetting { Key = "tutorial-video-file", Value = fileName, UpdatedAt = DateTime.UtcNow });
        }
        else
        {
            existing.Value = fileName;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        // Clear any YouTube URL since we're using an uploaded file
        var urlSetting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "tutorial-video-url");
        if (urlSetting is not null)
            db.SiteSettings.Remove(urlSetting);

        // Set source to "file"
        var sourceSetting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "tutorial-video-source");
        if (sourceSetting is null)
            db.SiteSettings.Add(new SiteSetting { Key = "tutorial-video-source", Value = "file", UpdatedAt = DateTime.UtcNow });
        else
        {
            sourceSetting.Value = "file";
            sourceSetting.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return new SiteSettingDto { Key = "tutorial-video-file", Value = fileName };
    }

    [HttpGet("tutorial-video/file")]
    public async Task<IActionResult> GetVideoFile()
    {
        var setting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "tutorial-video-file");
        if (setting is null)
            return NotFound();

        var filePath = Path.Combine(env.ContentRootPath, "uploads", setting.Value);
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var ext = Path.GetExtension(setting.Value).ToLowerInvariant();
        var contentType = ext switch
        {
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".ogg" => "video/ogg",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, contentType);
    }
}

public record SiteSettingDto
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public record SiteSettingUpdateDto
{
    public string Value { get; init; } = string.Empty;
}
