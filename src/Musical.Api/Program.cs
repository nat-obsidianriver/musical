using Microsoft.EntityFrameworkCore;
using Musical.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MusicalDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=musical.db"));

builder.Services.AddControllers();

// Allow the Razor Pages frontend to call this API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                builder.Configuration["AllowedOrigins"]?.Split(',')
                    ?? ["https://localhost:7040", "http://localhost:5127"])
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MusicalDbContext>();
    db.Database.Migrate();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
