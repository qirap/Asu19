using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asu19.Database
{
    [Table("user_car")]
    public class UserCar
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("car_id")]
        public int CarId { get; set; }
    }
}
