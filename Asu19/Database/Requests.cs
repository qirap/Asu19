using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asu19.Database
{
    [Table("requests")]
    public class Requests
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("car_id")]
        public int CarId { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("service_id")]
        public int ServiceId { get; set; }

        [Column("starttime")]
        public DateTime StartTime { get; set; }

        [Column("endtime")]
        public DateTime EndTime { get; set; }
    }
}
