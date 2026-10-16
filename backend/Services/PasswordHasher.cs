using System.Security.Cryptography;
using System.Text;

namespace SwedenStart;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 210_000;

    public static string Hash(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        var parts = new List<string>
        {
            Iterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key)
        };

        return string.Join(".", parts);
    }

    public static bool Verify(string password, string hash)
    {
        try
        {
            var parts = hash.Split('.');
            if (parts.Length != 3) return false;

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var attempted = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, key.Length);
            return CryptographicOperations.FixedTimeEquals(attempted, key);
        }
        catch
        {
            return false;
        }
    }
}
