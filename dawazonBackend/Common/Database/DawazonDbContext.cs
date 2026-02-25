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
        modelBuilder.Entity<User>()
            .Property(u => u.ProductsFavs)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
            );
        
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
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = "FIG000000001", Name = "Figuras", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Id = "COM000000001", Name = "Comics", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Id = "ROP000000001", Name = "Ropa", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        modelBuilder.Entity<Product>().HasData(
            new { Id = "PRD000000001", Name = "Funko Pop Iron Man", Price = 15.99, Stock = 50, Description = "Figura Funko Pop de Iron Man de Marvel.", CreatorId = 3L, CategoryId = "FIG000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/ironman_funko.jpg", "/uploads/products/ironman_funko_2.jpg"} },
            new { Id = "PRD000000002", Name = "Spider-Man Comic #1", Price = 9.99, Stock = 20, Description = "Primer número del comic de Spider-Man.", CreatorId = 3L, CategoryId = "COM000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/spiderman_comic.jpg", "/uploads/products/spiderman_comic_2.jpg" } },
            new { Id = "PRD000000003", Name = "Captain America T-Shirt", Price = 19.99, Stock = 30, Description = "Camiseta de algodón con el escudo del Capitán América.", CreatorId = 1L, CategoryId = "ROP000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/cap_tshirt.jpg", "/uploads/products/cap_tshirt_2.jpg"} },
            new { Id = "PRD000000004", Name = "Funko Pop Batman", Price = 14.99, Stock = 40, Description = "Figura Funko Pop de Batman de DC.", CreatorId = 1L, CategoryId = "FIG000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/batman_funko.jpg", "/uploads/products/batman_funko_2.jpg" } },
            new { Id = "PRD000000005", Name = "Funko Pop Joker", Price = 16.50, Stock = 15, Description = "Figura Funko Pop del Joker de DC Comics.", CreatorId = 1L, CategoryId = "FIG000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/joker_funko.jpg", "/uploads/products/joker_funko_2.jpg" } },
            new { Id = "PRD000000006", Name = "Funko Pop Hulk", Price = 18.00, Stock = 25, Description = "Figura Funko Pop de Hulk de Marvel.", CreatorId = 1L, CategoryId = "FIG000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/hulk_funko.jpg", "/uploads/products/hulk_funko_2.jpg" } },
            new { Id = "PRD000000007", Name = "Batman Comic #50", Price = 7.50, Stock = 60, Description = "Edición especial del comic de Batman.", CreatorId = 1L, CategoryId = "COM000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/batman_50.jpg", "/uploads/products/batman_50_2.jpg" } },
            new { Id = "PRD000000008", Name = "X-Men Comic #10", Price = 8.25, Stock = 12, Description = "Comic de los X-Men, número 10.", CreatorId = 1L, CategoryId = "COM000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/xmen_10.jpg", "/uploads/products/xmen_10_2.jpg" } },
            new { Id = "PRD000000009", Name = "Watchmen", Price = 25.00, Stock = 10, Description = "La aclamada novela gráfica Watchmen.", CreatorId = 1L, CategoryId = "COM000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/watchmen.jpg", "/uploads/products/watchmen_2.jpg" } },
            new { Id = "PRD000000010", Name = "Black Widow Hoodie", Price = 35.99, Stock = 18, Description = "Sudadera de la Viuda Negra.", CreatorId = 1L, CategoryId = "ROP000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/widow_hoodie.jpg", "/uploads/products/widow_hoodie_2.jpg" } },
            new { Id = "PRD000000011", Name = "Thor Logo Cap", Price = 12.00, Stock = 45, Description = "Gorra con el logo de Thor.", CreatorId = 1L, CategoryId = "ROP000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/thor_cap.jpg", "/uploads/products/thor_cap_2.jpg" } },
            new { Id = "PRD000000012", Name = "Hulk Gloves", Price = 22.50, Stock = 20, Description = "Guantes gigantes de Hulk.", CreatorId = 1L, CategoryId = "ROP000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/hulk_gloves.jpg", "/uploads/products/hulk_gloves_2.jpg"} },
            new { Id = "PRD000000013", Name = "Iron Man Mug", Price = 10.99, Stock = 100, Description = "Taza con diseño de casco de Iron Man.", CreatorId = 1L, CategoryId = "ROP000000001", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Version = 1L, Images = new List<string> { "/uploads/products/ironman_mug.jpg", "/uploads/products/ironman_mug_2.jpg" } }
        );

        modelBuilder.Entity<Cart.Models.Cart>().HasData(
            new 
            { 
                Id = "CART00000001", 
                UserId = 2L, 
                Purchased = true, 
                TotalItems = 2, 
                Total = 25.98, 
                CreatedAt = DateTime.UtcNow.AddDays(-1), 
                UploadAt = DateTime.UtcNow.AddDays(-1),
                CheckoutInProgress = false
            },
            new 
            { 
                Id = "CART00000002", 
                UserId = 2L, 
                Purchased = false, 
                TotalItems = 1, 
                Total = 19.99, 
                CreatedAt = DateTime.UtcNow, 
                UploadAt = DateTime.UtcNow,
                CheckoutInProgress = false
            }
        );

        modelBuilder.Entity<Cart.Models.Cart>().OwnsOne(c => c.Client).HasData(
            new { CartId = "CART00000001", Name = "Test User", Email = "user@user.com", Phone = "123456789" },
            new { CartId = "CART00000002", Name = "Test User", Email = "user@user.com", Phone = "123456789" }
        );

        modelBuilder.Entity<Cart.Models.Cart>().OwnsOne(c => c.Client).OwnsOne(c => c.Address).HasData(
            new { ClientCartId = "CART00000001", Number = (short)123, Street = "Calle Falsa", City = "Soria", Province = "Soria", Country = "España", PostalCode = 42001 },
            new { ClientCartId = "CART00000002", Number = (short)123, Street = "Calle Falsa", City = "Soria", Province = "Soria", Country = "España", PostalCode = 42001 }
        );

        modelBuilder.Entity<Cart.Models.Cart>().OwnsMany(c => c.CartLines).HasData(
            new { CartId = "CART00000001", ProductId = "PRD000000001", Quantity = 1, ProductPrice = 15.99, Status = Status.Preparado },
            new { CartId = "CART00000001", ProductId = "PRD000000002", Quantity = 1, ProductPrice = 9.99, Status = Status.Enviado },
            new { CartId = "CART00000002", ProductId = "PRD000000003", Quantity = 1, ProductPrice = 19.99, Status = Status.EnCarrito }
        );
    }
    
}