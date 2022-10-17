using System.ComponentModel.DataAnnotations;

namespace Asu19.Areas.Account.Models
{
    public class UserRegInfo : IUserAuthInfo
    {
        [Required]
        public string? Login { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? Address { get; set; }
    }
}
