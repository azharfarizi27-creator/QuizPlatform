using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuizPlatform.API.Helpers
{
    public class JwtHelper
    {
        private static string Secret =
            "THIS_IS_MY_SUPER_SECRET_KEY_FOR_JWT_TOKEN_123456789";

        public static string GenerateToken(
            int userId,
            string role)
        {
            var tokenHandler =
                new JwtSecurityTokenHandler();

            var key =
                Encoding.ASCII.GetBytes(Secret);

            var tokenDescriptor =
                new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(
                        new[]
                        {
                            new Claim(
                                "userId",
                                userId.ToString()
                            ),

                            new Claim(
                                "role",
                                role
                            )
                        }),

                    Expires =
                        DateTime.UtcNow.AddHours(2),

                    SigningCredentials =
                        new SigningCredentials(
                            new SymmetricSecurityKey(key),
                            SecurityAlgorithms.HmacSha256Signature
                        )
                };

            var token =
                tokenHandler.CreateToken(
                    tokenDescriptor
                );

            return tokenHandler.WriteToken(
                token
            );
        }
    }
}