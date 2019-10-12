using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookieJWT.Server.Models.DataModels
{
    [Table("UserAccount")]
    public class UserAccount
    {
        [Key]
        [Column(TypeName = "varchar(50)")]
        public string Email { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Password { get; set; }
    }
}
