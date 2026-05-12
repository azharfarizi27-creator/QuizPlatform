using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System.Text;
using Microsoft.IdentityModel.Tokens;

[assembly: OwinStartup(typeof(QuizPlatform.API.Startup))]

namespace QuizPlatform.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var secret =
                "THIS_IS_MY_SUPER_SECRET_KEY_FOR_JWT_TOKEN_123456789";

            var key =
                Encoding.ASCII.GetBytes(secret);

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode =
                        AuthenticationMode.Active,

                    TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateIssuerSigningKey = true,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(key),

                            ValidateLifetime = true
                        }
                });
        }
    }
}