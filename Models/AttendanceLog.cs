using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceApi.Models
{
    [Table("attendance_logs")]
    public class AttendanceLog
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        [Column("work_date")]
        public DateTime? WorkDate { get; set; }

        [Column("check_in_time")]
        public DateTime? CheckInTime { get; set; }

        [Column("check_out_time")]
        public DateTime? CheckOutTime { get; set; }

        [Column("location")]
        public string? Location { get; set; }

        [Column("latitude")]
        public decimal? Latitude { get; set; }

        [Column("longitude")]
        public decimal? Longitude { get; set; }

        [Column("similarity_score")]
        public decimal? SimilarityScore { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("note")]
        public string? Note { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("shift_id")]
        public Guid? ShiftId { get; set; }
    }
}