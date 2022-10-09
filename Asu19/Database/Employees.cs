using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asu19.Database
{
    [Table("employees")]
    public class Employees
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("year")]
        public DateTime Year { get; set; }
    }
}
