using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace dawazonBackend.Common.Utils;

public class IdGenerator : ValueGenerator<string>
{
    private const string Chars = "QWRTYPSDFGHJKLZXCVBNMqwrtypsdfghjklzxcvbnm1234567890-_";

    private const int Length = 12;
    public override bool GeneratesTemporaryValues => false;

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