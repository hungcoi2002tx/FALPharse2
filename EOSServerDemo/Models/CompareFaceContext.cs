using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EOSServerDemo.Models;

public partial class CompareFaceContext : DbContext
{
    public CompareFaceContext()
    {
    }

    public CompareFaceContext(DbContextOptions<CompareFaceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Source> Sources { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("server=TOMY;database=CompareFace;uid=sa;pwd=1234;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__Result__976902289813B3AD");

            entity.ToTable("Result");

            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.SourceId).HasColumnName("SourceID");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Time).HasColumnType("datetime");

            entity.HasOne(d => d.Source).WithMany(p => p.Results)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Result__SourceID__398D8EEE");
        });

        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasKey(e => e.SourceId).HasName("PK__Source__16E019F9E2EECE6E");

            entity.ToTable("Source");

            entity.Property(e => e.SourceId).HasColumnName("SourceID");
            entity.Property(e => e.ImagePath).HasMaxLength(255);
            entity.Property(e => e.StudentCode).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
