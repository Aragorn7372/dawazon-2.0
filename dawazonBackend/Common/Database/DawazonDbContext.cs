using dawazonBackend.Products.Models;
using dawazonBackend.Cart.Models;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dawazonBackend.Common.Database;

/// <summary>
/// Contexto de base de datos para la aplicación ProductApi.
/// Define las tablas, relaciones y datos iniciales (seeding).
/// </summary>
public class DawazonDbContext(DbContextOptions<DawazonDbContext> options)
    : IdentityDbContext<User, IdentityRole<long>, long>(options)
{
    /// <summary>
    /// Configura el modelo de datos, indices y datos iniciales.
    /// </summary>
    /// <param name="modelBuilder">Constructor de modelos de EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        SeedData(modelBuilder); 
        modelBuilder.Entity<User>()
            .OwnsOne(u => u.Client, clientBuilder =>
            {
                clientBuilder.OwnsOne(c => c.Address);
            });
        
        modelBuilder.Entity<Product>()
            .Property(p => p.Images)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
            );
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
            .Navigation(p => p.Comments)
            .AutoInclude();
        modelBuilder.Entity<Product>()
            .Property(p => p.Version)
            .IsConcurrencyToken();

        modelBuilder.Entity<Cart.Models.Cart>(entity =>
        {
            // Configuramos el cliente dentro del carrito
            entity.OwnsOne(c => c.Client, clientBuilder =>
            {
                clientBuilder.OwnsOne(c => c.Address);
            });

            // Configuramos las líneas del carrito
            entity.OwnsMany(c => c.CartLines, builder =>
            {
                builder.ToTable("CartLines");
                builder.WithOwner().HasForeignKey("CartId");
            
                builder.HasOne(cl => cl.Product)
                    .WithMany()
                    .HasForeignKey(cl => cl.ProductId);
            
                builder.HasKey("CartId", "ProductId");
            
                // LA CORRECCIÓN DEL ENUM: Así EF Core lo guarda como string automáticamente
                builder.Property(cl => cl.Status)
                    .HasConversion<string>()
                    .IsRequired();
            });
        });
        
        

    }
    
    
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categorias { get; set; } = null!;

    public DbSet<Cart.Models.Cart> Carts { get; set; } = null!;
    
        
    /// <summary>
    /// Método privado para sembrar datos de prueba en la base de datos.
    /// </summary>
    /// <param name="modelBuilder">Constructor de modelos.</param>
    private static void SeedData(ModelBuilder modelBuilder)
    {

    }
    
}