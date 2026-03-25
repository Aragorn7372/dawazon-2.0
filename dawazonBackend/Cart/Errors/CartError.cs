using dawazonBackend.Common.Error;

namespace dawazonBackend.Cart.Errors;

/// <summary>
/// Record base que representa un error genérico relacionado con operaciones del carrito de compras.
/// </summary>
/// <remarks>
/// Esta clase abstracta forma parte de la jerarquía de errores del dominio.
/// Hereda de <see cref="DomainError"/> y proporciona la base para todos los errores
/// específicos del módulo de carrito.
///
/// <para><b>Jerarquía de errores:</b></para>
/// <code>
/// DomainError (base abstracta)
///   └── CartError (base del módulo)
///       ├── CartNotFoundError
///       ├── CartProductQuantityExceededError
///       ├── CartAttemptAmountExceededError
///       ├── CartUnauthorizedError
///       └── CartMinQuantityError
/// </code>
/// 
/// <para><b>Uso típico:</b></para>
/// Los errores del carrito se utilizan en el servicio <c>CartService</c> para manejar
/// casos de error y devolver respuestas significativas al cliente. Se combinan con
/// el patrón Result para operaciones fluidas.
/// </remarks>
public record CartError(string Message) : DomainError (Message);


/// <summary>
/// Error específico cuando no se encuentra un carrito en el sistema.
/// </summary>
/// <remarks>
/// Este error se lanza cuando se intenta acceder, modificar o eliminar un carrito
/// que no existe en la base de datos.
///
/// <para><b>Escenarios comunes:</b></para>
/// <list type="bullet">
///     <item>GET /cart/{id} - El carrito con el ID proporcionado no existe</item>
///     <item>PUT /cart/{id} - Actualización de un carrito eliminado previamente</item>
///     <item>DELETE /cart/{id} - Intento de eliminar un carrito ya eliminado</item>
/// </list>
/// 
/// <para><b>Manejo recomendado:</b></para>
/// Devolver un HTTP 404 (Not Found) al cliente con el mensaje descriptivo.
/// </remarks>
public record CartNotFoundError(string Message) : CartError(Message);


/// <summary>
/// Error específico cuando la cantidad solicitada de un producto excede el stock disponible.
/// </summary>
/// <remarks>
/// Este error se lanza cuando un usuario intenta añadir más unidades de un producto
/// de las que hay disponibles en inventario.
///
/// <para><b>Escenarios comunes:</b></para>
/// <list type="bullet">
///     <item>Añadir producto al carrito con cantidad mayor al stock</item>
///     <item>Incrementar cantidad de producto en el carrito</item>
///     <item>Finalizar compra con cantidad no disponible</item>
/// </list>
/// 
/// <para><b>Propiedades del error:</b></para>
/// <list type="bullet">
///     <item>Message por defecto: "La cantidad de producto supera el stock"</item>
/// </list>
/// 
/// <para><b>Manejo recomendado:</b></para>
/// Mostrar al usuario la cantidad máxima disponible y permitir ajustar la cantidad.
/// </remarks>
public record CartProductQuantityExceededError(
    string Message = "La cantidad de producto supera el stock"
) : CartError(Message);

/// <summary>
/// Error específico cuando se excede el número máximo de intentos permitidos por concurrencia.
/// </summary>
/// <remarks>
/// Este error protege contra problemas de concurrencia y race conditions cuando
/// múltiples usuarios intentan modificar el mismo carrito simultáneamente.
///
/// <para><b>Escenarios comunes:</b></para>
/// <list type="bullet">
///     <item>Dos usuarios intentan comprar el último producto al mismo tiempo</item>
///     <item>Modificaciones simultáneas del mismo carrito</item>
///     <item>Conflictos de actualización en operaciones de stock</item>
/// </list>
/// 
/// <para><b>Propiedades del error:</b></para>
/// <list type="bullet">
///     <item>Message por defecto: "La cantidad de tries superado, vuelva a intentarlo"</item>
/// </list>
/// 
/// <para><b>Manejo recomendado:</b></para>
/// Sugerir al usuario que vuelva a intentar la operación después de unos segundos,
/// o mostrar un mensaje indicando alta demanda del producto.
/// </remarks>
public record CartAttemptAmountExceededError(
    string Message = "La cantidad de tries superado, vuelva a intentarlo"
) : CartError(Message);

/// <summary>
/// Error específico cuando un usuario no tiene autorización para realizar una operación.
/// </summary>
/// <remarks>
/// Este error se lanza cuando un usuario intenta acceder o modificar un carrito
/// que no le pertenece, sin tener permisos de administrador.
///
/// <para><b>Escenarios comunes:</b></para>
/// <list type="bullet">
///     <item>Usuario intenta ver el carrito de otro usuario</item>
///     <item>Usuario intenta modificar el carrito de otro cliente</item>
///     <item>Acceso a carritos de otros usuarios sin permisos de admin</item>
/// </list>
/// 
/// <para><b>Manejo recomendado:</b></para>
/// Devolver HTTP 403 (Forbidden) o HTTP 401 (Unauthorized) según corresponda,
/// mostrando un mensaje genérico por seguridad.
/// </remarks>
public record CartUnauthorizedError(string Message) : CartError(Message);

/// <summary>
/// Error específico cuando la cantidad mínima permitida (1 unidad) no se cumple.
/// </summary>
/// <remarks>
/// Este error se lanza cuando se intenta establecer una cantidad de producto
/// menor a 1 en el carrito.
///
/// <para><b>Escenarios comunes:</b></para>
/// <list type="bullet">
///     <item>Intentar establecer cantidad 0 o negativa</item>
///     <item>Decrementar cantidad por debajo de 1</item>
/// </list>
/// 
/// <para><b>Nota:</b></para>
/// Para eliminar un producto del carrito, en lugar de establecer cantidad 0,
/// se debe utilizar la operación de eliminación de línea.
/// 
/// <para><b>Manejo recomendado:</b></para>
/// Indicar al usuario que use la función de eliminar producto del carrito.
/// </remarks>
public record CartMinQuantityError(string Message) : CartError(Message);



