using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Asu19.Areas.Account.Models
{
    public class UserLogInfo : IUserAuthInfo
    {
        [Required (ErrorMessage = "Введите логин")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Длина логина должна быть от 4 до 32 символов")]
        public string? Login { get; set; }

        [Required (ErrorMessage = "Введите пароль")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Длина пароля должна быть от 4 до 32 символов")]
        public string? Password { get; set; }
    }
}
