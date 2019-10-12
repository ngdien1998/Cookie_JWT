using System;

namespace CookieJWT.Client.Models
{
    public class LoginedUser
    {
        public LoginedUser(string username)
        {
            Username = username;
        }

        public string Username { get; set; }
    }
}
