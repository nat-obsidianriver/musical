using Microsoft.EntityFrameworkCore;
using Musical.Core.Models;

namespace Musical.Api.Data;

public class MusicalDbContext(DbContextOptions<MusicalDbContext> options) : DbContext(options)
{
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<Annotation> Annotations => Set<Annotation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Score>(e =>
        {
            e.Property(s => s.Title).HasMaxLength(200).IsRequired();
            e.Property(s => s.Composer).HasMaxLength(200);
            e.Property(s => s.ImageFileName).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<Annotation>(e =>
        {
            e.Property(a => a.AuthorName).HasMaxLength(100).IsRequired();
            e.Property(a => a.Content).HasMaxLength(2000).IsRequired();
            e.HasOne(a => a.Score)
             .WithMany(s => s.Annotations)
             .HasForeignKey(a => a.ScoreId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
