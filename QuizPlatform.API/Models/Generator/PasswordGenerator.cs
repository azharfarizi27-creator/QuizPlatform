using System.Security.Cryptography;
using System.Text;

namespace QuizPlatform.API.Models.Generator
{
    public class PasswordGenerator
    {
        public static string GenerateHash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);

                var hash = sha.ComputeHash(bytes);

                StringBuilder builder =
                    new StringBuilder();

                foreach (var item in hash)
                {
                    builder.Append(
                        item.ToString("x2")
                    );
                }

                return builder.ToString();
            }
        }
    }
}