namespace dawazonBackend.Cart.Dto;

/// <summary>
/// Data Transfer Object (DTO) que representa la respuesta completa de un carrito de compras.
/// </summary>
/// <remarks>
/// Este DTO es un record que contiene toda la información de un carrito de compras,
/// incluyendo el identificador, usuario, estado de compra, información del cliente,
/// líneas de productos, y totales.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Inmutable: Como record, una vez creado no puede ser modificado.</item>
///     <item>Igualdad por valor: Dos instancias con los mismos valores son iguales.</item>
///     <item>Conciso: Utilza la sintaxis primaria constructor para definir propiedades.</item>
/// </list>
/// 
/// <para><b>Uso típico:</b></para>
/// Se utiliza como respuesta en los endpoints de API del módulo de carrito:
/// <list type="bullet">
///     <item>GET /cart/{id} - Obtener un carrito específico</item>
///     <item>GET /cart/user/{userId} - Obtener carrito de un usuario</item>
///     <item>GET /cart/all - Listar carritos con filtros</item>
/// </list>
/// </remarks>
public record CartResponseDto(
    /// <summary>
    /// Identificador único del carrito de compras.
    /// </summary>
    /// <value>String único que identifica el carrito (ejemplo: "CART-2024-001").</value>
    /// <remarks>
    /// Este identificador se genera automáticamente al crear el carrito
    /// y se utiliza para todas las operaciones subsecuentes.
    /// </remarks>
    string Id,
    
    /// <summary>
    /// Identificador del usuario propietario del carrito.
    /// </summary>
    /// <value>Identificador numérico del usuario en el sistema.</value>
    /// <remarks>
    /// Relaciona el carrito con el usuario que lo creó y es propietario.
    /// </remarks>
    long UserId,
    
    /// <summary>
    /// Indica si el carrito ha sido convertido en pedido comprado.
    /// </summary>
    /// <value>True si el carrito ha sido comprado (pagado), False si está activo.</value>
    /// <remarks>
    /// Cuando un carrito se compra, se marca como Purchased = true y se crea
    /// un pedido formal. Los carritos comprados no pueden ser modificados.
    /// </remarks>
    bool Purchased,
    
    /// <summary>
    /// Datos del cliente para envío del pedido.
    /// </summary>
    /// <value>Instancia de <see cref="ClientDto"/> con la información de envío.</value>
    /// <remarks>
    /// Contiene la dirección de envío, teléfono y email del cliente.
    /// Se cumplimenta cuando el usuario finaliza la compra.
    /// </remarks>
    ClientDto Client,
    
    /// <summary>
    /// Colección de líneas de productos en el carrito.
    /// </summary>
    /// <value>Lista de objetos <see cref="SaleLineDto"/> representando cada producto.</value>
    /// <remarks>
    /// Cada línea representa un producto diferente en el carrito con su cantidad,
    /// precio y estado. La lista puede estar vacía si el carrito no tiene productos.
    /// </remarks>
    List<SaleLineDto> CartLines,
    
    /// <summary>
    /// Cantidad total de productos en el carrito.
    /// </summary>
    /// <value>Entero que representa la suma de cantidades de todas las líneas.</value>
    /// <remarks>
    /// Se calcula sumando la propiedad Quantity de cada línea en CartLines.
    /// Es útil para mostrar badges o indicadores de número de artículos.
    /// </remarks>
    int TotalItems,
    
    /// <summary>
    /// Importe total del carrito (suma de todas las líneas).
    /// </summary>
    /// <value>Decimal con el precio total incluyendo descuentos e impuestos.</value>
    /// <remarks>
    /// Se calcula sumando TotalPrice de todas las líneas del carrito.
    /// Este es el amount que el usuario deberá pagar al comprar.
    /// </remarks>
    double Total
);