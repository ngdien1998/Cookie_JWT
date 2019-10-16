using System.ComponentModel.DataAnnotations;

namespace CookieJWT.Client.Models
{
    public class UserAccountViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [Required]
        [MaxLength(50), MinLength(1)]
        public string Password { get; set; }

        public bool RememberLogin { get; set; }
    }
}
