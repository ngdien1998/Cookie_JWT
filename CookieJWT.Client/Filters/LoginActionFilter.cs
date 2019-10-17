using CookieJWT.Client.Common;
using CookieJWT.Client.Controllers;
using CookieJWT.Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CookieJWT.Client.Filters
{
    public class LoginActionFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //var requestCookies = context.HttpContext.Request.Cookies;
            //if (!requestCookies.ContainsKey(Consts.LoginToken) || !requestCookies.ContainsKey(Consts.Username))
            //{
            //    context.HttpContext.Session.Remove(Consts.LoginSession);
            //}

            var sessionUser = context.HttpContext.Session.GetObject<LoginedUser>(Consts.LoginSession);
            if (sessionUser == null)
            {
                context.Result = new RedirectToActionResult(nameof(AccountController.Login), ControllerName.Of<AccountController>(), null);
            }
        }
    }
}