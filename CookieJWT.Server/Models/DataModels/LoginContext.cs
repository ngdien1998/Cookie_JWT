using Microsoft.EntityFrameworkCore;

namespace CookieJWT.Server.Models.DataModels
{
    public class LoginContext : DbContext
    {
        public DbSet<UserAccount> UserAccounts { get; set; }

        public LoginContext(DbContextOptions options) : base(options)
        {
        }
    }
}
