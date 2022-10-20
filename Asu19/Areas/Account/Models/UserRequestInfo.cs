namespace Asu19.Areas.Account.Models
{
    public class UserRequestInfo
    {
        public int? Id { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Car { get; set; }
        public string? Service { get; set; }
        public string? Price { get; set; }
        public string? Employee { get; set; }
        public string? Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
