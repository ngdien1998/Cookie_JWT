using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CookieJWT.Server.Common
{
    public class JwtTokenManager
    {
        private readonly string secret;

        public JwtTokenManager(string secret)
        {
            this.secret = secret;
        }

        public string GenerateToken(string username)
        {
            var key = Convert.FromBase64String(secret);
            var securityKey = new SymmetricSecurityKey(key);
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Email, username)
            };

            var tokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(10),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateJwtSecurityToken(tokenDesc);
            return handler.WriteToken(token);
        }

        public string GetUsername(string token)
        {
            var principal = GetPrincipal(token);
            if (principal == null)
            {
                return string.Empty;
            }

            if (principal.Identity is ClaimsIdentity identity)
            {
                var userClaim = identity.FindFirst(ClaimTypes.Email);
                return userClaim?.Value;
            }
            return string.Empty;
        }

        private ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                if (jwtToken == null)
                {
                    return null;
                }

                var key = Convert.FromBase64String(secret);
                var tokenParams = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                return tokenHandler.ValidateToken(token, tokenParams, out _);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
