using CookieJWT.Domain;
using CookieJWT.Server.Common;
using CookieJWT.Server.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CookieJWT.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LoginContext context;
        private readonly JwtTokenManager tokenManager;

        public LoginController(LoginContext context, IConfiguration configuration)
        {
            this.context = context;
            tokenManager = new JwtTokenManager(configuration["Secret"]);
        }

        [HttpGet("Signin")]
        public async Task<IActionResult> LoginWithUsernameAndPassword(string username, string password)
        {
            var found = await context.UserAccounts.AnyAsync(e => e.Email == username && e.Password == password);
            if (!found)
            {
                return Ok(new UserLoginDomain
                {
                    LoginStatus = LoginStatus.InvalidUsernameOrPassword
                });
            }
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Email, username)
            };
            var token = tokenManager.GenerateToken(claims);
            return Ok(new UserLoginDomain(username)
            {
                LoginStatus = LoginStatus.Successfull,
                Token = token
            });
        }

        [HttpGet("Validate")]
        public IActionResult LoginWithToken(string username, string token)
        {
            bool accept;
            try
            {
                accept = IsvalidToken(username, token);
            }
            catch (SecurityTokenExpiredException)
            {
                return Ok(new UserLoginDomain
                {
                    LoginStatus = LoginStatus.TokenExpired
                });
            }

            if (accept)
            {
                return Ok(new UserLoginDomain(username)
                {
                    Token = token,
                    LoginStatus = LoginStatus.Successfull
                });
            }

            return Ok(new UserLoginDomain
            {
                LoginStatus = LoginStatus.InvalidToken
            });
        }

        private bool IsvalidToken(string email, string token)
        {
            try
            {
                var principal = tokenManager.GetPrincipal(token);
                if (principal == null)
                {
                    return false;
                }

                if (principal.Identity is ClaimsIdentity identity)
                {
                    var userClaim = identity.FindFirst(ClaimTypes.Email);
                    return userClaim?.Value == email;
                }
                return false;
            }
            catch (SecurityTokenExpiredException e)
            {
                throw e;
            }
        }
    }
}