using System.Security.Cryptography;

namespace PandaClaus.Web.Core;

public static class HashGenerator
{
    // Excludes ambiguous characters (0/O, 1/I) to keep hashes easy to read/type.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int Length = 6;

    public static string GenerateHash()
    {
        Span<char> chars = stackalloc char[Length];
        for (var i = 0; i < Length; i++)
        {
            var index = RandomNumberGenerator.GetInt32(Alphabet.Length);
            chars[i] = Alphabet[index];
        }

        return new string(chars);
    }
}
