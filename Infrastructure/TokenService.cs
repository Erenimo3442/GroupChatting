using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application;
using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
            var key = config["AppSettings:Token"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException(
                    "Token key is not configured in appsettings.json."
                );
            }
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.Username),
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

            if (
                !int.TryParse(
                    _config["AppSettings:AccessTokenLifetimeDays"],
                    out var lifetimeInDays
                )
            )
            {
                lifetimeInDays = 7;
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(lifetimeInDays),
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
