using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Cart.Models;

/// <summary>
/// Modelo de dominio que representa una dirección postal completa.
/// </summary>
/// <remarks>
/// Esta clase contiene todos los campos necesarios para identificar una ubicación
/// de envío de pedido.
///
/// <para><b>Relaciones:</b></para>
/// Se composiciona dentro de <see cref="Client"/>, no es una tabla independiente.
///
/// <para><b>Patrón de uso:</b></para>
/// La dirección se "copia" en el pedido en el momento de la compra.
/// Esto garantiza que aunque el cliente cambie su dirección después,
/// los pedidos históricos mantienen la dirección correcta.
///
/// <para><b>Validaciones:</b></para>
/// <list type="bullet">
///     <item>Number: Entero no negativo</item>
///     <item>Street: Requerida, nombre de la calle</item>
///     <item>City: Requerida, ciudad de envío</item>
///     <item>Province: Requerida, provincia/región</item>
///     <item>Country: Requerido, país</item>
///     <item>PostalCode: Entero entre 0 y 63000</item>
/// </list>
/// </remarks>
public class Address
{
    /// <summary>
    /// Número de la calle, portal o vivienda.
    /// </summary>
    /// <value>Número entero no negativo.</value>
    /// <remarks>
    /// Puede ser 0 en casos especiales (apartado de correos, etc.).
    /// </remarks>
    [Range(0,int.MaxValue)]
    public int Number { get; set; }

    /// <summary>
    /// Nombre de la calle o vía pública.
    /// </summary>
    /// <value>Nombre de la calle.</value>
    /// <remarks>
    /// Puede incluir tipo de vía (Calle, Avenida, Plaza, etc.).
    /// </remarks>
    [Required]
    public string Street {get; set;} = string.Empty;

    /// <summary>
    /// Ciudad o población de destino.
    /// </summary>
    /// <value>Nombre de la ciudad.</value>
    /// <remarks>
    /// Se utiliza para calcular costes de envío y para el清洗.
    /// </remarks>
    [Required]
    public string City {get; set;} = string.Empty;

    /// <summary>
    /// Provincia o región administrativa.
    /// </summary>
    /// <value>Nombre de la provincia.</value>
    /// <remarks>
    /// Necesaria para el cálculo de impuestos y restricciones de envío.
    /// </remarks>
    [Required]
    public string Province {get; set;} = string.Empty;

    /// <summary>
    /// País de destino.
    /// </summary>
    /// <value>Nombre del país.</value>
    /// <remarks>
    /// Necesario para envíos internacionales y cálculos de aduanas.
    /// </remarks>
    [Required]
    public string Country {get; set;} = string.Empty;

    /// <summary>
    /// Código postal.
    /// </summary>
    /// <value>Entero entre 0 y 63000.</value>
    /// <remarks>
    /// Se utiliza para identificar la zona de entrega y calcular el envío.
    /// En España, los códigos postas van de 01000 a 52019 aproximadamente.
    /// </remarks>
    [Range(0,63000)]
    public int PostalCode { get; set; } 
    
}