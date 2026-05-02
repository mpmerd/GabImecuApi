using GabImecuApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GabImecuApi.Data;

public class BdGabineteContext : DbContext
{
    public BdGabineteContext(DbContextOptions<BdGabineteContext> options) : base(options) { }

    public DbSet<TCategoria> TCategoria => Set<TCategoria>();
    public DbSet<TDistrito> TDistritos => Set<TDistrito>();
    public DbSet<TFamilia> TFamilia => Set<TFamilia>();
    public DbSet<THistorial> THistorials => Set<THistorial>();
    public DbSet<TIglesia> TIglesia => Set<TIglesia>();
    public DbSet<TLogin> TLogins => Set<TLogin>();
    public DbSet<TObispo> TObispos => Set<TObispo>();
    public DbSet<TOrdenacion> TOrdenacions => Set<TOrdenacion>();
    public DbSet<TOutside> TOutsides => Set<TOutside>();
    public DbSet<TPastor> TPastors => Set<TPastor>();
    public DbSet<TSupeditado> TSupeditados => Set<TSupeditado>();
    public DbSet<TSuperintendente> TSuperintendentes => Set<TSuperintendente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TCategoria>(entity =>
        {
            entity.HasNoKey().ToTable("T_categoria");
            entity.Property(e => e.IdCategoria).ValueGeneratedOnAdd();
            entity.Property(e => e.Categoria).HasMaxLength(50);
        });

        modelBuilder.Entity<TDistrito>(entity =>
        {
            entity.HasKey(e => e.IdDistrito);
            entity.ToTable("T_distrito");
            entity.Property(e => e.Distrito).HasMaxLength(50);
        });

        modelBuilder.Entity<TFamilia>(entity =>
        {
            entity.HasKey(e => e.IdFamilia).HasName("PK__T_famili__751F80CFE5E16A81");
            entity.ToTable("T_familia");
        });

        modelBuilder.Entity<THistorial>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PK__T_histor__9CC7DBB4B1C275DE");
            entity.ToTable("T_historial");
        });

        modelBuilder.Entity<TIglesia>(entity =>
        {
            entity.HasKey(e => e.IdIglesia);
            entity.ToTable("T_iglesia");
            entity.Property(e => e.Distrito).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TLogin>(entity =>
        {
            entity.HasNoKey().ToTable("T_login");
            entity.Property(e => e.IdUser).ValueGeneratedOnAdd();
            entity.Property(e => e.Usuario).HasMaxLength(10);
            entity.Property(e => e.Contraseña).HasMaxLength(20);
        });

        modelBuilder.Entity<TObispo>(entity =>
        {
            entity.HasKey(e => e.IdObispo);
            entity.ToTable("T_obispo");
        });

        modelBuilder.Entity<TOrdenacion>(entity =>
        {
            entity.HasKey(e => e.IdOrdenacion).HasName("PK__T_ordena__1E09BF4AF6C49CF7");
            entity.ToTable("T_ordenacion");
        });

        modelBuilder.Entity<TOutside>(entity =>
        {
            entity.HasNoKey().ToTable("T_outside");
            entity.Property(e => e.Categoria).HasMaxLength(10);
        });

        modelBuilder.Entity<TPastor>(entity =>
        {
            entity.HasKey(e => e.IdPastor);
            entity.ToTable("T_pastor");
            entity.Property(e => e.Categoria).HasMaxLength(50);
            entity.Property(e => e.Celular).HasMaxLength(50);
        });

        modelBuilder.Entity<TSupeditado>(entity =>
        {
            entity.HasNoKey().ToTable("T_supeditados");
            entity.Property(e => e.IdSupeditado).ValueGeneratedOnAdd();
            entity.Property(e => e.Categoria).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.Organismo).HasMaxLength(50);
        });

        modelBuilder.Entity<TSuperintendente>(entity =>
        {
            entity.HasKey(e => e.IdSuperintendente);
            entity.ToTable("T_superintendente");
        });
    }
}
