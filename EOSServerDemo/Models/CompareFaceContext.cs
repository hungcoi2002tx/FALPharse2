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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server =(local);database=CompareFace;uid=sa;pwd=123456;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__Result__97690228E263A9A8");

            entity.ToTable("Result");

            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.ExamCode).HasMaxLength(50);
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.Note).HasMaxLength(255);
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
            entity.HasKey(e => e.SourceId).HasName("PK__Source__16E019F9BC4BE5E5");

            entity.ToTable("Source");

            entity.Property(e => e.SourceId).HasColumnName("SourceID");
            entity.Property(e => e.ImagePath).HasMaxLength(255);
            entity.Property(e => e.StudentCode).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
