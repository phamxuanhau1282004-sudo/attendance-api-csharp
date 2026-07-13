using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceApi.Models
{
    [Table("employees")]
    public class Employee
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("employee_code")]
        public string? EmployeeCode { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("phone")]
        public string? PhoneNumber { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("role")]
        public int? Role { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [Column("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}