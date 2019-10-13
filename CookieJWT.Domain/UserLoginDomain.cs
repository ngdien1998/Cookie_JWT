using System;

namespace CookieJWT.Domain
{
    public class UserLoginDomain
    {
        public UserLoginDomain()
        {
        }

        public UserLoginDomain(string username)
        {
            Username = username;
        }

        public string Username { get; set; }
        public string Token { get; set; }
        public LoginStatus LoginStatus { get; set; }
    }

    public enum LoginStatus
    {
        Default,
        Successfull,
        InvalidUsernameOrPassword,
        InvalidToken,
        TokenExpired
    }
}
