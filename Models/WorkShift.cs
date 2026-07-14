using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceApi.Models
{
    [Table("work_shifts")] // Tên bảng trong Supabase
    public class WorkShift
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("shift_name")]
        public string ShiftName { get; set; } = string.Empty;

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        // --- THÊM 2 CỘT MỚI VÀO ĐÂY ---
        [Column("late_after_minute")]
        public int LateAfterMinute { get; set; }

        [Column("early_leave_minute")]
        public int EarlyLeaveMinute { get; set; }
    }
}