using Microsoft.AspNetCore.Mvc;

namespace CookieJWT.Client.Common
{
    public static class ControllerName
    {
        public static string Of<TController>() where TController : Controller
        {
            return typeof(TController).Name.Replace("Controller", "");
        }
    }
}
