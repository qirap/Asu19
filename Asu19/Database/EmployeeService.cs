using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asu19.Database
{
    [Table("employee_service")]
    public class EmployeeService
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("service_id")]
        public int ServiceId { get; set; }
    }
}
