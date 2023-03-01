using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Parkhausverwaltung.Shared.Models;

namespace Parkhausverwaltung.Server.Infrastructure;

public partial class ParkhausverwaltungContext : DbContext
{
    public ParkhausverwaltungContext()
    {
    }

    public ParkhausverwaltungContext(DbContextOptions<ParkhausverwaltungContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Floor> Floors { get; set; }

    public virtual DbSet<Mieter> Mieters { get; set; }

    public virtual DbSet<Parkhau> Parkhaus { get; set; }

    public virtual DbSet<Tarif> Tarifs { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DB");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasKey(e => e.FloorId).HasName("PK__FLoor__61422FE220F42544");

            entity.ToTable("FLoor");

            entity.Property(e => e.FloorId)
                .ValueGeneratedNever()
                .HasColumnName("FLoorId");

            entity.HasOne(d => d.Parkhaus).WithMany(p => p.Floors)
                .HasForeignKey(d => d.ParkhausId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FLoor__ParkhausI__2D27B809");
        });

        modelBuilder.Entity<Mieter>(entity =>
        {
            entity.HasKey(e => e.MieterId).HasName("PK__Mieter__883C6102AEAD7DC5");

            entity.ToTable("Mieter");

            entity.HasIndex(e => e.MieterCode, "UQ__Mieter__C1793B1F6365E562").IsUnique();

            entity.Property(e => e.MieterId).ValueGeneratedNever();
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Parkhaus).WithMany(p => p.Mieters)
                .HasForeignKey(d => d.ParkhausId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Mieter__Parkhaus__276EDEB3");
        });

        modelBuilder.Entity<Parkhau>(entity =>
        {
            entity.HasKey(e => e.ParkhausId).HasName("PK__Parkhaus__E7FDE3F48EE4CF3E");

            entity.Property(e => e.ParkhausId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Tarif>(entity =>
        {
            entity.HasKey(e => e.TarifId).HasName("PK__Tarif__57971C71D7AE7AC7");

            entity.ToTable("Tarif");

            entity.Property(e => e.TarifId).ValueGeneratedNever();
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Preis).HasColumnType("decimal(10, 1)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Parkhaus).WithMany(p => p.Tarifs)
                .HasForeignKey(d => d.ParkhausId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tarif__ParkhausI__2A4B4B5E");
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.VisitId).HasName("PK__Visit__4D3AA1DEC779F329");

            entity.ToTable("Visit");

            entity.Property(e => e.VisitId).ValueGeneratedNever();
            entity.Property(e => e.Arrival).HasColumnType("datetime");
            entity.Property(e => e.Cost).HasColumnType("decimal(10, 1)");
            entity.Property(e => e.Departure).HasColumnType("datetime");

            entity.HasOne(d => d.Mieter).WithMany(p => p.Visits)
                .HasForeignKey(d => d.MieterId)
                .HasConstraintName("FK__Visit__MieterId__30F848ED");

            entity.HasOne(d => d.Parkhaus).WithMany(p => p.Visits)
                .HasForeignKey(d => d.ParkhausId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Visit__ParkhausI__300424B4");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
