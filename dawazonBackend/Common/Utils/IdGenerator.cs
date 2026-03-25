using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace dawazonBackend.Common.Utils;

using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
/// Generador de identificadores personalizados cortos y aleatorios.
/// </summary>
/// <remarks>
/// Implementa <see cref="ValueGenerator{TValue}"/> de Entity Framework Core.
/// Se utiliza para generar IDs de productos, categorías y carritos de forma automática.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Longitud: 12 caracteres</item>
///     <item>Caracteres: alfanuméricos + guiones (mayúsculas, minúsculas, números)</item>
///     <item>Criptográficamente seguro: usa RandomNumberGenerator</item>
///     <item>Permanente: GeneratesTemporaryValues = false</item>
/// </list>
/// 
/// <para><b>Ejemplo de uso en EF Core:</b></para>
/// <code>
/// modelBuilder.Entity&lt;Product&gt;()
///     .Property(p => p.Id)
///     .HasValueGenerator&lt;IdGenerator&gt;();
/// </code>
/// </remarks>
public class IdGenerator : ValueGenerator<string>
{
    /// <summary>
    /// Caracteres permitidos para generar IDs.
    /// </summary>
    /// <remarks>
    /// Incluye: QWRTYPSDFGHJKLZXCVBNM (mayúsculas), qwrtypsdfghjklzxcvbnm (minúsculas), 1234567890-_ (números y guiones)
    /// </remarks>
    private const string Chars = "QWRTYPSDFGHJKLZXCVBNMqwrtypsdfghjklzxcvbnm1234567890-_";

    /// <summary>
    /// Longitud del ID generado.
    /// </summary>
    private const int Length = 12;

    /// <inheritdoc/>
    /// <summary>
    /// Indica que este generador crea valores permanentes (no temporales).
    /// </summary>
    public override bool GeneratesTemporaryValues => false;

    /// <inheritdoc/>
    /// <summary>
    /// Genera un nuevo ID aleatorio de 12 caracteres.
    /// </summary>
    /// <param name="entry">Entrada de la entidad en EF Core.</param>
    /// <returns>Una cadena aleatoria única de 12 caracteres.</returns>
    /// <remarks>
    /// Usa <see cref="RandomNumberGenerator.Fill"/> para generar bytes criptográficamente seguros.
    /// </remarks>
    public override string Next(EntityEntry entry)
    {
        var bytes= new byte[Length];
        RandomNumberGenerator.Fill(bytes);
        var id= new char[Length];
        for (int i = 0; i<Length; i++)
        {
            id[i]=Chars[bytes[i] % Chars.Length];
        }
        return new string(id);
    }
}