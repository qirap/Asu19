using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asu19.Database
{
    [Table("cars")]
    public class Cars
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("brand")]
        public string? Brand { get; set; }

        [Column("model")]
        public string? Model { get; set; }
    }
}
