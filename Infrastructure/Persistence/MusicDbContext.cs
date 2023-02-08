using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public partial class MusicDbContext : DbContext, IMusicDbContext
{
    public MusicDbContext()
    {
    }

    public MusicDbContext(DbContextOptions<MusicDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MusicAou> MusicAous { get; set; } = null!;

    public virtual DbSet<MusicExtra> MusicExtras { get; set; } = null!;

    public virtual DbSet<MusicUnlock> MusicUnlocks { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var defaultDb = Path.Combine(PathHelper.DatabasePath, "music471omni.db3");
        optionsBuilder.UseSqlite($"Data Source={defaultDb}");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MusicAou>(entity =>
        {
            entity.HasKey(e => e.MusicId);

            entity.ToTable("music_aou");

            entity.Property(e => e.MusicId)
                .ValueGeneratedNever()
                .HasColumnName("music_id");
            entity.Property(e => e.UseFlag).HasColumnName("use_flag");
        });

        modelBuilder.Entity<MusicExtra>(entity =>
        {
            entity.HasKey(e => e.MusicId);

            entity.ToTable("music_extra");

            entity.Property(e => e.MusicId)
                .ValueGeneratedNever()
                .HasColumnName("music_id");
            entity.Property(e => e.UseFlag).HasColumnName("use_flag");
        });

        modelBuilder.Entity<MusicUnlock>(entity =>
        {
            entity.HasKey(e => e.MusicId);

            entity.ToTable("music_unlock");

            entity.Property(e => e.MusicId)
                .ValueGeneratedNever()
                .HasColumnName("music_id");
            entity.Property(e => e.Artist).HasColumnName("artist");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.NewFlag).HasColumnName("new_flag");
            entity.Property(e => e.UseFlag).HasColumnName("use_flag");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
