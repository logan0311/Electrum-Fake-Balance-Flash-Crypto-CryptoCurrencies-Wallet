using System.Security.Cryptography;

namespace Vulpes.Electrum.Domain.Security;
public interface IKnoxHasher
{
    string HashPassword(string password);
    bool CompareHash(string retrievedHash, string providedPassword);
}

public class KnoxHasher : IKnoxHasher
{
    public string HashPassword(string password)
    {
        var salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);

        var hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    public bool CompareHash(string retrievedHash, string providedPassword)
    {
        var hashBytes = Convert.FromBase64String(retrievedHash);
        var salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, salt, 100000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);

        // Compare hash.
        for (var i = 0; i < 32; i++)
        {
            if (hashBytes[i + 16] != hash[i])
            {
                throw new UnauthorizedAccessException();
            }
        }

        // If the user has made it this far, then the password was correct.
        return true;
    }
}
