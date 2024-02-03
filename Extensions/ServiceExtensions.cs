using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ang_auth_api_2024.Extensions
{
    public static class ServiceExtensions
    {

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSecretSigningKey = configuration["JwtSecrets:SecretSigningKey"];

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretSigningKey)),
                    ValidateAudience = false,
                    ValidateIssuer = false
                };
            });


        }
    }
}
