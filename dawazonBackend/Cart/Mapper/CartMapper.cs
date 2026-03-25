namespace dawazonBackend.Cart.Mapper;

using System;
using System.Linq;
using Models;
using Dto;

/// <summary>
/// Clase de utilidad estática para mapear entre modelos de dominio de carrito, 
/// líneas de carrito y sus respectivos Data Transfer Objects (DTOs).
/// </summary>
/// <remarks>
/// Esta clase proporciona métodos de extensión para convertir entre las entidades
/// del dominio (modelos de base de datos) y los DTOs utilizados en la capa de presentación/API.
///
/// <para><b>Patrón de diseño:</b></para>
/// Utiliza el patrón Mapper con métodos de extensión para mantener la coherencia
/// con el estilo de código del proyecto. Los métodos son estáticos y no mantienen estado.
///
/// <para><b>Convenciones de nomenclatura:</b></para>
/// <list type="bullet">
///     <item>ToDto(): Convierte modelo → DTO</item>
///     <item>ToModel(): Convierte DTO → modelo</item>
/// </list>
/// 
/// <para><b>Modelos soportados:</b></para>
/// <list type="bullet">
///     <item><see cref="Cart"/> ↔ <see cref="CartResponseDto"/></item>
///     <item><see cref="CartLine"/> ↔ <see cref="SaleLineDto"/></item>
///     <item><see cref="Client"/> ↔ <see cref="ClientDto"/></item>
/// </list>
/// </remarks>
public static class CartMapper
{
    /// <summary>
    /// Convierte un modelo de dominio <see cref="Cart"/> a un <see cref="CartResponseDto"/>
    /// para su uso en la capa de presentación o API.
    /// </summary>
    /// <param name="model">El modelo del carrito a convertir.</param>
    /// <returns>Un <see cref="CartResponseDto"/> con los datos del carrito.</returns>
    /// <remarks>
    /// Este método mapea todas las propiedades del modelo a su correspondiente DTO:
    /// <list type="bullet">
    ///     <item>Id, UserId, Purchased, TotalItems, Total se copian directamente</item>
    ///     <item>Client se convierte usando <see cref="ToDto(Client)"/></item>
    ///     <item>CartLines se convierten usando <see cref="ToDto(CartLine, Cart)"/></item>
    /// </list>
    /// 
    /// <para><b>Nota:</b></para>
    /// El parámetro de extensión es el modelo, y se pasa como "this" para permitir
    /// la sintaxis de método de extensión: <c>cart.ToDto()</c>
    /// </remarks>
    public static CartResponseDto ToDto(this Cart model)
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
    /// Convierte un <see cref="CartResponseDto"/> a un modelo de dominio <see cref="Cart"/>.
    /// </summary>
    /// <param name="dto">El DTO del carrito a convertir.</param>
    /// <returns>Un <see cref="Models.Cart"/> con los datos del DTO.</returns>
    /// <remarks>
    /// Este método se utiliza principalmente para reconstruir objetos de dominio desde DTOs
    /// recibidos en solicitudes HTTP.
    /// 
    /// <para><b>Campos inicializados con valores por defecto:</b></para>
    /// <list type="bullet">
    ///     <item>CreatedAt: Fecha UTC actual</item>
    ///     <item>UploadAt: Fecha UTC actual</item>
    ///     <item>CheckoutInProgress: false</item>
    ///     <item>CheckoutStartedAt: null</item>
    /// </list>
    /// 
    /// <para><b>Nota de seguridad:</b></para>
    /// Los campos de gestión interna se inicializan con valores por defecto para evitar
    /// manipulación desde el exterior.
    /// </remarks>
    public static Cart ToModel(this CartResponseDto dto)
    {
        return new Cart
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
    /// Convierte un modelo de línea de carrito <see cref="CartLine"/> a un <see cref="SaleLineDto"/>.
    /// </summary>
    /// <param name="model">El modelo de línea de carrito a convertir.</param>
    /// <param name="parentCart">Opcional. Carrito padre para obtener datos del cliente y usuario.</param>
    /// <returns>Un <see cref="SaleLineDto"/> con los datos de la línea.</returns>
    /// <remarks>
    /// Este método mapea las propiedades de la línea de carrito al DTO:
    /// <list type="bullet">
    ///     <item>SaleId: Se mapea desde CartId</item>
    ///     <item>ProductName: Se obtiene del producto relacionado, o string.Empty si no existe</item>
    ///     <item>Client y UserId: Se obtienen del carrito padre si se proporciona</item>
    ///     <item>ManagerId y ManagerName: Se inicializan vacíos (se asignan al procesar el pedido)</item>
    ///     <item>CreateAt y UpdateAt: Se inicializan con la fecha UTC actual</item>
    /// </list>
    /// 
    /// <para><b>Uso típico:</b></para>
    /// Se llama durante la conversión de un carrito completo, pasando el padre para
    /// enriquecer las líneas con información del cliente.
    /// </remarks>
    public static SaleLineDto ToDto(this CartLine model, Cart? parentCart = null)
    {
        return new SaleLineDto
        {
            SaleId = model.CartId, 
            ProductId = model.ProductId,
            ProductName = model.Product?.Name ?? string.Empty,
            Quantity = model.Quantity,
            ProductPrice = model.ProductPrice,
            TotalPrice = model.TotalPrice, 
            Status = model.Status,
            
            Client = parentCart?.Client ?? new Client(),
            UserId = parentCart?.UserId ?? 0,
            
            ManagerId = 0, 
            ManagerName = string.Empty,
            CreateAt = DateTime.UtcNow,
            UpdateAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Convierte un <see cref="SaleLineDto"/> a un modelo de línea de carrito <see cref="CartLine"/>.
    /// </summary>
    /// <param name="dto">El DTO de línea de venta a convertir.</param>
    /// <returns>Un <see cref="CartLine"/> con los datos del DTO.</returns>
    /// <remarks>
    /// Este método realiza una conversión simple, mapeando solo las propiedades
    /// esenciales de la línea de carrito:
    /// <list type="bullet">
    ///     <item>CartId: Se mapea desde SaleId</item>
    ///     <item>ProductId, Quantity, ProductPrice, Status se copian directamente</item>
    /// </list>
    /// 
    /// <para><b>Nota:</b></para>
    /// No se mapean propiedades como Product (relación de EF Core) ya que
    /// estas se resuelven independientemente en el repositorio.
    /// </remarks>
    public static CartLine ToModel(this SaleLineDto dto)
    {
        return new CartLine
        {
            CartId = dto.SaleId, 
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            ProductPrice = dto.ProductPrice,
            Status = dto.Status
        };
    }
    
    /// <summary>
    /// Convierte un modelo de dominio <see cref="Client"/> a un <see cref="ClientDto"/>,
    /// aplanando la propiedad Address anidada en campos individuales.
    /// </summary>
    /// <param name="model">El modelo de cliente a convertir.</param>
    /// <returns>Un <see cref="ClientDto"/> con los datos del cliente.</returns>
    /// <remarks>
    /// Este método "aplana" la estructura anidada del Address del modelo de dominio
    /// en campos individuales del DTO:
    /// <list type="bullet">
    ///     <item>Name, Email, Phone se copian directamente</item>
    ///     <item>Number, Street, City, Province, Country, PostalCode se extraen de Address</item>
    /// </list>
    /// 
    /// <para><b>Seguridad:</b></para>
    /// Si Address es null, los campos de dirección se inicializan con valores por defecto
    /// (string.Empty para strings, 0 para números).
    /// </remarks>
    public static ClientDto ToDto(this Client model)
    {
        return new ClientDto
        {
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            
            Number = model.Address?.Number ?? 0,
            Street = model.Address?.Street ?? string.Empty,
            City = model.Address?.City ?? string.Empty,
            Province = model.Address?.Province ?? string.Empty,
            Country = model.Address?.Country ?? string.Empty,
            PostalCode = model.Address?.PostalCode ?? 0
        };
    }

    /// <summary>
    /// Convierte un <see cref="ClientDto"/> a un modelo de dominio <see cref="Client"/>,
    /// reconstruyendo el objeto Address a partir de los campos individuales del DTO.
    /// </summary>
    /// <param name="dto">El DTO de cliente a convertir.</param>
    /// <returns>Un <see cref="Client"/> con los datos del DTO.</returns>
    /// <remarks>
    /// Este método "reconstruye" la estructura anidada del modelo de dominio
    /// a partir de los campos planos del DTO:
    /// <list type="bullet">
    ///     <item>Name, Email, Phone se copian directamente</item>
    ///     <item>Se crea un nuevo Address con los datos de dirección</item>
    /// </list>
    /// 
    /// <para><b>Uso típico:</b></para>
    /// Se utiliza al crear o actualizar un cliente desde datos recibidos en la API.
    /// </remarks>
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