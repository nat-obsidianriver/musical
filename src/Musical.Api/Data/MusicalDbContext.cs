using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Musical.Api.Models;
using Musical.Core.Models;

namespace Musical.Api.Data;

public class MusicalDbContext(DbContextOptions<MusicalDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<Annotation> Annotations => Set<Annotation>();
    public DbSet<Folder> Folders => Set<Folder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // required for Identity tables

        modelBuilder.Entity<Score>(e =>
        {
            e.Property(s => s.Title).HasMaxLength(200).IsRequired();
            e.Property(s => s.Composer).HasMaxLength(200);
            e.Property(s => s.ImageFileName).HasMaxLength(500).IsRequired();
            e.HasOne(s => s.Folder)
             .WithMany(f => f.Scores)
             .HasForeignKey(s => s.FolderId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Annotation>(e =>
        {
            e.Property(a => a.AuthorName).HasMaxLength(100).IsRequired();
            e.Property(a => a.Content).HasMaxLength(2000).IsRequired();
            e.Property(a => a.AttachmentFileName).HasMaxLength(500);
            e.Property(a => a.UserId).HasMaxLength(450);
            e.HasOne(a => a.Score)
             .WithMany(s => s.Annotations)
             .HasForeignKey(a => a.ScoreId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Folder)
             .WithMany()
             .HasForeignKey(a => a.FolderId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Folder>(e =>
        {
            e.Property(f => f.Name).HasMaxLength(200).IsRequired();
            e.Property(f => f.Color).HasMaxLength(20);
            e.Property(f => f.UserId).HasMaxLength(450).IsRequired();
            e.Property(f => f.UserDisplayName).HasMaxLength(200);
        });
    }
}
