namespace ST10439055_POE_PROG6212.Services
{
    public interface IPasswordService
    {
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt);
    }
}

