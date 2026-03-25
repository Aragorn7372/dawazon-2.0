namespace dawazonBackend.Common.Attribute;
using System;

/// <summary>
/// Atributo personalizado que marca propiedades que deben generar un ID único automáticamente.
/// </summary>
/// <remarks>
/// Este atributo se utiliza para indicar a Entity Framework Core que debe generar
/// un ID personalizado para la propiedad decorada.
///
/// <para><b>Uso:</b></para>
/// Se aplica a propiedades de tipo string en entidades del modelo de datos.
/// El generador de ID debe estar configurado en el DbContext.
///
/// <para><b>Ejemplo de uso:</b></para>
/// <code>
/// public class Cart
/// {
///     [Key]
///     [GenerateCustomIdAtribute]
///     public string Id { get; set; } = string.Empty;
/// }
/// </code>
/// 
/// <para><b>Nota:</b></para>
/// Este es un atributo de marca (marker attribute). La lógica de generación
/// de ID se implementa en el DbContext o en un Value Converter de EF Core.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class GenerateCustomIdAtribute: Attribute
{
    /// <summary>
    /// Constructor público del atributo.
    /// </summary>
    /// <remarks>
    /// No requiere parámetros ya que es un marker attribute.
    /// </remarks>
}