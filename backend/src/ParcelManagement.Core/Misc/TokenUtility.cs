using System.Security.Cryptography;
using System.Text;

namespace ParcelManagement.Core.Misc
{
    public static class TokenUtility
    {
        public static string HashToken(string token)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }
    }
}