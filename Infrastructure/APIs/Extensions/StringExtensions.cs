using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.APIs.Extensions
{
    public static class StringExtensions
    {

        public static string HmacSHA256 (this string value)
        {
            byte[]? data = Encoding.ASCII.GetBytes(value);
            byte[]? hashData = new SHA1Managed().ComputeHash(data);
            string? hash = string.Empty;
            foreach (var b in hashData)
            {
                hash += b.ToString("X2");
            }
            return hash;
        }

    }
}
