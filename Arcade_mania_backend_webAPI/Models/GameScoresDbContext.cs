using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Arcade_mania_backend_webAPI.Models;

public partial class GameScoresDbContext : DbContext
{
    public GameScoresDbContext()
    {
    }

    public GameScoresDbContext(DbContextOptions<GameScoresDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Name, "uq_users_name").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.FighterHighScore)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("fighter_high_score");
            entity.Property(e => e.MemoryHighScore)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("memory_high_score");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.SnakeHighScore)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("snake_high_score");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
