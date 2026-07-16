using AttendanceApi.Data;
using AttendanceApi.Models; // Thêm dòng này để C# hiểu AttendanceLog
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceApi.Controllers
{
    // 1. Tạo Class DTO để nhận cục JSON từ Flutter gửi lên
    public class AttendanceRequest
    {
        public Guid EmployeeId { get; set; }
        public string? ShiftId { get; set; }
        public bool IsCheckOut { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- HÀM 1: LẤY TRẠNG THÁI CA LÀM (Giữ nguyên của bạn) ---
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

        // --- HÀM 2: LƯU CHẤM CÔNG (ĐOẠN MỚI) ---
        [HttpPost("Log")]
        public async Task<IActionResult> LogAttendance([FromBody] AttendanceRequest request)
        {
            var nowUtc = DateTime.UtcNow;

            // 1. NẾU LÀ TAN CA
            if (request.IsCheckOut)
            {
                var openLog = await _context.AttendanceLogs
                    .Where(l => l.EmployeeId == request.EmployeeId && l.CheckOutTime == null)
                    .OrderByDescending(l => l.CheckInTime)
                    .FirstOrDefaultAsync();

                if (openLog == null)
                    return BadRequest(new { message = "Không tìm thấy ca làm việc nào đang mở để Tan ca!" });

                openLog.CheckOutTime = nowUtc;
                _context.AttendanceLogs.Update(openLog);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Tan ca thành công!" });
            }

            // 2. NẾU LÀ VÀO CA
            else
            {
                if (string.IsNullOrEmpty(request.ShiftId))
                    return BadRequest(new { message = "Lỗi: Không có thông tin ca làm việc!" });

                // Đoạn bọc thép: Kiểm tra xem có ca nào chưa check-out không
                var existingLog = await _context.AttendanceLogs
                    .Where(l => l.EmployeeId == request.EmployeeId && l.CheckOutTime == null)
                    .FirstOrDefaultAsync();

                if (existingLog != null)
                    return BadRequest(new { message = "Bạn đang có ca làm việc chưa kết thúc!" });

                var newLog = new AttendanceLog
                {
                    EmployeeId = request.EmployeeId,
                    ShiftId = Guid.Parse(request.ShiftId),
                    CheckInTime = nowUtc,
                    WorkDate = nowUtc.AddHours(7).Date, // Đưa về chuẩn ngày Việt Nam
                    Status = "Đúng giờ"
                };

                _context.AttendanceLogs.Add(newLog);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Vào ca thành công!" });
            }
        }
    }
}