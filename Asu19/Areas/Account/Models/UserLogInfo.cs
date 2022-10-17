using System.ComponentModel.DataAnnotations;

namespace Asu19.Areas.Account.Models
{
    public class UserLogInfo : IUserAuthInfo
    {
        [Required]
        public string? Login { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
