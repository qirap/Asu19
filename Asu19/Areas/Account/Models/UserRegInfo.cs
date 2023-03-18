using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using System.ComponentModel.DataAnnotations;

namespace Asu19.Areas.Account.Models
{
    public class UserRegInfo : IUserAuthInfo
    {
        [Required(ErrorMessage = "Введите логин")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Длина логина должна быть от 4 до 32 символов")]
        public string? Login { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Длина пароля должна быть от 4 до 32 символов")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Длина имени должна быть от 4 до 32 символов")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Длина фамилии должна быть от 4 до 32 символов")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Введите адрес")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Длина адреса должна быть от 4 до 32 символов")]
        public string? Address { get; set; }
    }
}
