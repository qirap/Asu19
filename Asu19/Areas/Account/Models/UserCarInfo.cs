using System.ComponentModel.DataAnnotations;

namespace Asu19.Areas.Account.Models
{
    public class UserCarInfo
    {
        [Required]
        public string? Brand { get; set; }

        [Required]
        public string? Model { get; set; }
    }
}
