using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asu19.Database
{
    [Table("users")]
    public class Users
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("login")]
        public string? Login { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public  string? LastName { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("role")]
        public  string? Role { get; set; }
    }
}
