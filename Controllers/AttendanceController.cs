using AttendanceApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Status/{userId}")]
        public async Task<IActionResult> GetAttendanceStatus(Guid userId)
        {
            // 1. Lấy dòng chấm công mới nhất của nhân viên này
            var latestLog = await _context.AttendanceLogs
                .Where(l => l.EmployeeId == userId)
                .OrderByDescending(l => l.CheckInTime)
                .FirstOrDefaultAsync();

            int status = 0; // Mặc định: 0 (Chưa vào ca / Hôm nay chưa chấm công)
            string? logId = null;

            if (latestLog != null && latestLog.CheckInTime.HasValue)
            {
                // 2. Chuyển giờ CheckIn (UTC lưu trên DB) sang giờ Việt Nam (+7) để so sánh chuẩn ngày
                var checkInLocal = latestLog.CheckInTime.Value.AddHours(7);
                var todayLocal = DateTime.UtcNow.AddHours(7);

                // 3. Kiểm tra xem dòng chấm công mới nhất này có phải thuộc ngày hôm nay không
                if (checkInLocal.Date == todayLocal.Date)
                {
                    if (latestLog.CheckOutTime == null)
                    {
                        status = 1; // Đang trong ca (Đã check-in nhưng chưa check-out)
                        logId = latestLog.Id.ToString();
                    }
                    else
                    {
                        status = 0; // Đã hoàn thành (Có đủ cả check-in và check-out trong hôm nay)
                        logId = null;
                    }
                }
            }

            return Ok(new { status = status, logId = logId });
        }
    }
}