namespace dawazonBackend.Cart.Mapper;

using System;
using System.Linq;
using dawazonBackend.Cart.Models;
using dawazonBackend.Cart.Dto;

public static class CartMapper
{
    /// <summary>
    /// Convierte un modelo de base de datos Cart a un CartResponseDto.
    /// </summary>
    public static CartResponseDto ToDto(this Models.Cart model)
    {
        return new CartResponseDto(
            Id: model.Id,
            UserId: model.UserId,
            Purchased: model.Purchased,
            Client: model.Client.ToDto(), 
            CartLines: model.CartLines?.Select(cl => cl.ToDto(model)).ToList() ?? [],
            TotalItems: model.TotalItems,
            Total: model.Total
        );
    }

    /// <summary>
    /// Convierte un CartResponseDto a un modelo Cart.
    /// Nota: Campos internos de gestión (como CreatedAt, UploadAt, CheckoutInProgress) 
    /// se inicializan con valores por defecto.
    /// </summary>
    public static Models.Cart ToModel(this CartResponseDto dto)
    {
        return new Models.Cart
        {
            Id = dto.Id,
            UserId = dto.UserId,
            Purchased = dto.Purchased,
            Client = dto.Client.ToModel(), 
            CartLines = dto.CartLines?.Select(sl => sl.ToModel()).ToList() ?? [],
            TotalItems = dto.TotalItems,
            Total = dto.Total,
            CreatedAt = DateTime.UtcNow,
            UploadAt = DateTime.UtcNow,
            CheckoutInProgress = false,
            CheckoutStartedAt = null
        };
    }

    /// <summary>
    /// Convierte un modelo CartLine a SaleLineDto.
    /// Se le pasa el Cart padre para poder extraer datos que CartLine no tiene (como Client o UserId).
    /// </summary>
    public static SaleLineDto ToDto(this CartLine model, Models.Cart? parentCart = null)
    {
        return new SaleLineDto
        {
            SaleId = model.CartId, // Mapeamos el CartId al SaleId
            ProductId = model.ProductId,
            ProductName = model.Product?.Name ?? string.Empty,
            Quantity = model.Quantity,
            ProductPrice = model.ProductPrice,
            TotalPrice = model.TotalPrice, // Propiedad calculada en el modelo
            Status = model.Status,
            
            // Los siguientes campos existen en SaleLineDto pero no en CartLine.
            // Los rellenamos usando el carrito padre si está disponible.
            Client = parentCart?.Client ?? new Client(),
            UserId = parentCart?.UserId ?? 0,
            
            // Estos campos no tienen una correspondencia directa, se inicializan por defecto.
            // (Deberás ajustarlos en tu lógica de negocio si es necesario).
            ManagerId = 0, 
            ManagerName = string.Empty,
            CreateAt = DateTime.UtcNow,
            UpdateAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Convierte un SaleLineDto a un modelo CartLine.
    /// </summary>
    public static CartLine ToModel(this SaleLineDto dto)
    {
        return new CartLine
        {
            CartId = dto.SaleId, // Mapeamos el SaleId de vuelta al CartId
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            ProductPrice = dto.ProductPrice,
            Status = dto.Status
        };
    }
    
    /// <summary>
    /// Convierte un modelo Client a un ClientDto, aplanando la propiedad Address.
    /// </summary>
    public static ClientDto ToDto(this Client model)
    {
        return new ClientDto
        {
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            
            // Aplanamos las propiedades del objeto anidado Address
            Number = model.Address?.Number ?? 0,
            Street = model.Address?.Street ?? string.Empty,
            City = model.Address?.City ?? string.Empty,
            Province = model.Address?.Province ?? string.Empty,
            Country = model.Address?.Country ?? string.Empty,
            PostalCode = model.Address?.PostalCode ?? 0
        };
    }

    /// <summary>
    /// Convierte un ClientDto a un modelo Client, reconstruyendo el objeto Address.
    /// </summary>
    public static Client ToModel(this ClientDto dto)
    {
        return new Client
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            
            Address = new Address
            {
                Number = dto.Number,
                Street = dto.Street,
                City = dto.City,
                Province = dto.Province,
                Country = dto.Country,
                PostalCode = dto.PostalCode
            }
        };
    }
}