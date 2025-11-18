using System.Linq;
using System.Security.Cryptography;

namespace ST10439055_POE_PROG6212.Services
{
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            passwordSalt = RandomNumberGenerator.GetBytes(SaltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, passwordSalt, Iterations, HashAlgorithmName.SHA256);
            passwordHash = pbkdf2.GetBytes(KeySize);
        }

        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (storedHash.Length == 0 || storedSalt.Length == 0)
            {
                return false;
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, Iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(KeySize);
            return computedHash.SequenceEqual(storedHash);
        }
    }
}

