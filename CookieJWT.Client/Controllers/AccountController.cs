using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using CookieJWT.Client.Common;
using CookieJWT.Client.Filters;
using CookieJWT.Client.Models;
using CookieJWT.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CookieJWT.Client.Controllers
{
    public class AccountController : Controller
    {
        public static CultureInfo Format = CultureInfo.CreateSpecificCulture("vi-VN");
        private const string LoginScheme = "DefaultScheme";

        private readonly IConfiguration configuration;

        public AccountController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var savedtoken = HttpContext.Request.Cookies[Consts.LoginToken];
            var username = HttpContext.Request.Cookies[Consts.Username];
            if (!string.IsNullOrWhiteSpace(savedtoken) && !string.IsNullOrWhiteSpace(username))
            {
                var (loginSuccess, token) = await LoginWithTokenSuccessAsync(username, savedtoken);
                if (loginSuccess)
                {
                    // Rewrite Cookie
                    HttpContext.Response.Cookies.Delete(Consts.LoginToken);
                    HttpContext.Response.Cookies.Delete(Consts.Username);

                    var savingTime = GetCookieOptions(DateTime.Now);
                    HttpContext.Response.Cookies.Append(Consts.LoginToken, token, savingTime);
                    HttpContext.Response.Cookies.Append(Consts.Username, username, savingTime);

                    // Current login state
                    HttpContext.Session.SetObject(Consts.LoginSession, new LoginedUser(username));
                    return RedirectToAction(nameof(HomeController.Index), ControllerName.Of<HomeController>());
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserAccountViewModel acc)
        {
            if (!ModelState.IsValid)
            {
                return View(acc);
            }

            var (loginSuccess, savedToken) = await AuthorizeWithUsernameAndPasswordAsync(acc.Email, acc.Password);
            if (!loginSuccess)
            {
                ModelState.AddModelError("InvalidLogin", "Username or password is invalid. Please check again.");
                return View(acc);
            }

            if (acc.RememberLogin)
            {
                var savingTime = GetCookieOptions(DateTime.Now);
                HttpContext.Response.Cookies.Append(Consts.LoginToken, savedToken, savingTime);
                HttpContext.Response.Cookies.Append(Consts.Username, acc.Email, savingTime);
            }

            HttpContext.Session.SetObject(Consts.LoginSession, new LoginedUser(acc.Email));
            return RedirectToAction(nameof(HomeController.Index), ControllerName.Of<HomeController>());
        }

        private async Task<(bool, string)> LoginWithTokenSuccessAsync(string username, string token)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(configuration["ApiAddress"])
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var resp = await client.GetAsync($"api/Login/validate?username={username}&token={token}");
                if (resp.IsSuccessStatusCode)
                {
                    var userLoginString = await resp.Content.ReadAsStringAsync();
                    var userLogin = JsonConvert.DeserializeObject<UserLoginDomain>(userLoginString);
                    if (userLogin != null && userLogin.LoginStatus == LoginStatus.Successfull)
                    {
                        return (true, userLogin.Token);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return (false, string.Empty);
        }

        private async Task<(bool, string)> AuthorizeWithUsernameAndPasswordAsync(string username, string password)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(configuration["ApiAddress"])
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var resp = await client.GetAsync($"api/Login/signin?username={username}&password={password}");
                if (resp.IsSuccessStatusCode)
                {
                    var userLoginString = await resp.Content.ReadAsStringAsync();
                    var userLogin = JsonConvert.DeserializeObject<UserLoginDomain>(userLoginString);
                    if (userLogin != null && userLogin.LoginStatus == LoginStatus.Successfull)
                    {
                        return (true, userLogin.Token);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return (false, string.Empty);
        }

        [HttpGet]
        [AuthorizedLogin]
        public IActionResult Logout()
        {
            var res = SignOut(LoginScheme);
            return RedirectToAction(nameof(Index));
        }

        public CookieOptions GetCookieOptions(DateTime now) => new CookieOptions
        {
            MaxAge = TimeSpan.FromDays(15),
            Expires = now.AddDays(15)
        };
    }
}