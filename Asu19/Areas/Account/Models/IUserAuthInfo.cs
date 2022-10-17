namespace Asu19.Areas.Account.Models
{
    public interface IUserAuthInfo
    {
        public string? Login { get; set; }
        public string? Password { get; set; }
    }
}
