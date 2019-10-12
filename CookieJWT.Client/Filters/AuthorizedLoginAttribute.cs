using Microsoft.AspNetCore.Mvc;
using System;

namespace CookieJWT.Client.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizedLoginAttribute : TypeFilterAttribute
    {
        public AuthorizedLoginAttribute() : base(typeof(LoginActionFilter))
        {
        }
    }
}
