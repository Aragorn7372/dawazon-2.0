using dawazonBackend.Products.Models;
using dawazonBackend.Cart.Models;
using Microsoft.EntityFrameworkCore;

namespace dawazonBackend.Common.Database;

/// <summary>
/// Contexto de base de datos para la aplicación ProductApi.
/// Define las tablas, relaciones y datos iniciales (seeding).
/// </summary>
public class DawazonDbContext : DbContext
{
    /// <summary>
    /// Configura el modelo de datos, indices y datos iniciales.
    /// </summary>
    /// <param name="modelBuilder">Constructor de modelos de EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        SeedData(modelBuilder); 
        modelBuilder.Entity<Product>()
            .OwnsMany(p => p.Images, builder =>
            {
                builder.ToTable("ProductImages");
                builder.WithOwner()
                    .HasForeignKey("ProductId"); 
                builder.Property<string>("Value") 
                    .HasColumnName("Image")
                    .IsRequired();
                builder.HasKey("ProductId", "Value"); // PK compuesta
            });
        modelBuilder.Entity<Product>()
            .OwnsMany(p => p.Comments, builder =>
            {
                builder.ToTable("ProductComments");       // nombre de la tabla
                builder.WithOwner().HasForeignKey("ProductId"); // FK hacia Product
                builder.Property<int>("Id");               // PK necesaria
                builder.HasKey("Id");

                builder.Property(c => c.UserId).IsRequired();
                builder.Property(c => c.Content).HasMaxLength(200).IsRequired();
                builder.Property(c => c.CreatedAt).IsRequired();
                builder.Property(c => c.verified).IsRequired();
                builder.Property(c => c.recommended).IsRequired();
            });
        modelBuilder.Entity<Product>()
            .Navigation(p => p.Images)
            .AutoInclude(); 
        modelBuilder.Entity<Product>()
            .Navigation(p => p.Comments)
            .AutoInclude();
        modelBuilder.Entity<Product>()
            .Property(p => p.Version)
            .IsConcurrencyToken();

        modelBuilder.Entity<Cart>()
            .OwnsMany(c =>  c.CartLines)
    }
    
    
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categorias { get; set; } = null!;
    
  /// <summary>
  /// Inicializa una nueva instancia del contexto de base de datos.
  /// </summary>
  /// <param name="options">Opciones de configuración del contexto.</param>
    public DawazonDbContext(DbContextOptions<DawazonDbContext> options)
        : base(options)
    { }
   
        
    /// <summary>
    /// Método privado para sembrar datos de prueba en la base de datos.
    /// </summary>
    /// <param name="modelBuilder">Constructor de modelos.</param>
    private void SeedData(ModelBuilder modelBuilder)
    {

    }
    
}