using AttendanceApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkShiftsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorkShiftsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetWorkShifts()
        {
            var shifts = _context.WorkShifts
                .Select(s => new
                {
                    id = s.Id,
                    shift_name = s.ShiftName,
                    start_time = s.StartTime.ToString(@"hh\:mm"),
                    end_time = s.EndTime.ToString(@"hh\:mm"),
                    // Bổ sung xuất dữ liệu 2 cột mới ra JSON
                    late_after_minute = s.LateAfterMinute,
                    early_leave_minute = s.EarlyLeaveMinute
                })
                .ToList();

            return Ok(shifts);
        }
    }
}