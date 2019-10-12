using CookieJWT.Domain;
using CookieJWT.Server.Common;
using CookieJWT.Server.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        [HttpGet("signin")]
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

            var token = tokenManager.GenerateToken(username);
            return Ok(new UserLoginDomain(username)
            {
                LoginStatus = LoginStatus.Successfull,
                Token = token
            });
        }

        [HttpGet("validate")]
        public IActionResult LoginWithToken(string username, string token)
        {
            if (IsvalidToken(username, token))
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

        private bool IsvalidToken(string username, string token)
        {
            return tokenManager.GetUsername(token) == username;
        }
    }
}