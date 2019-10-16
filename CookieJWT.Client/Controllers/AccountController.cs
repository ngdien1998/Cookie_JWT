using CookieJWT.Client.Common;
using CookieJWT.Client.Filters;
using CookieJWT.Client.Models;
using CookieJWT.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CookieJWT.Client.Controllers
{
    public class AccountController : Controller
    {
        public static CultureInfo Format = CultureInfo.CreateSpecificCulture("vi-VN");

        private IResponseCookies ResponseCookies => HttpContext.Response.Cookies;
        private IRequestCookieCollection RequestCookies => HttpContext.Request.Cookies;
        private ISession Session => HttpContext.Session;

        private readonly IConfiguration configuration;

        public AccountController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var savedtoken = RequestCookies[Consts.LoginToken];
            var username = RequestCookies[Consts.Username];
            if (!string.IsNullOrWhiteSpace(savedtoken) && !string.IsNullOrWhiteSpace(username))
            {
                var (loginSuccess, token) = await AuthorizeWithTokenAsync(username, savedtoken);
                if (loginSuccess)
                {
                    // Rewrite Cookie
                    ResponseCookies.Delete(Consts.LoginToken);
                    ResponseCookies.Delete(Consts.Username);

                    var savingTime = GetCookieOptions(DateTime.Now);
                    ResponseCookies.Append(Consts.LoginToken, token, savingTime);
                    ResponseCookies.Append(Consts.Username, username, savingTime);

                    // Current login state
                    Session.SetObject(Consts.LoginSession, new LoginedUser(username));
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
                ResponseCookies.Append(Consts.LoginToken, savedToken, savingTime);
                ResponseCookies.Append(Consts.Username, acc.Email, savingTime);
            }

            Session.SetObject(Consts.LoginSession, new LoginedUser(acc.Email));
            return RedirectToAction(nameof(HomeController.Index), ControllerName.Of<HomeController>());
        }

        private async Task<(bool, string)> AuthorizeWithTokenAsync(string username, string token)
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
            Session.Remove(Consts.LoginSession);
            foreach (var cookie in RequestCookies.Keys)
            {
                ResponseCookies.Delete(cookie);
            }
            return RedirectToAction(nameof(AccountController.Login), ControllerName.Of<AccountController>());
        }

        private CookieOptions GetCookieOptions(DateTime now) => new CookieOptions
        {
            MaxAge = TimeSpan.FromDays(15),
            Expires = now.AddDays(15)
        };
    }
}