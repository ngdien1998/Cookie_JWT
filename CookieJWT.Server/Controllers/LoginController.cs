﻿using CookieJWT.Domain;
using CookieJWT.Server.Common;
using CookieJWT.Server.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
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
                var claims = new Claim[]
                {
                    new Claim(ClaimTypes.Email, username)
                };
                var newToken = tokenManager.GenerateToken(claims);

                return Ok(new UserLoginDomain
                {
                    LoginStatus = LoginStatus.TokenExpiredWithNewToken,
                    Token = newToken
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
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
                var principal = tokenManager.GetPrincipal(token, out var securityToken);
                if (principal == null)
                {
                    return false;
                }

                if (principal.Identity is ClaimsIdentity identity)
                {
                    var userClaim = identity.FindFirst(ClaimTypes.Email);
                    if (userClaim?.Value == email)
                    {
                        if (securityToken != null && securityToken.ValidTo.Date < DateTime.Now.Date)
                        {
                            throw new SecurityTokenExpiredException();
                        }
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}