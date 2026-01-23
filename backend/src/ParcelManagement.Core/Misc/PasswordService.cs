using Microsoft.AspNetCore.Identity;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Misc
{
    public class PasswordService
    {
        public static string HashPlainPasswordOrToken(User user, string plainPassword)
        {
            var hasher = new PasswordHasher<User>();
            return hasher.HashPassword(user, plainPassword);
        }

        public static bool VerifyPassword(User user, string hashedPassword, string providedPassword)
        {
            var hasher = new PasswordHasher<User>();
            return hasher.VerifyHashedPassword(user, hashedPassword, providedPassword) == PasswordVerificationResult.Success ||
                hasher.VerifyHashedPassword(user, hashedPassword, providedPassword) == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}