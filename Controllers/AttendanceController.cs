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
            // Lấy ngày hôm nay theo giờ VN dưới dạng chuỗi "yyyy-MM-dd"
            string todayStr = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd");

            // Lấy tất cả log của nhân viên này
            var logs = await _context.AttendanceLogs
                .Where(l => l.EmployeeId == userId)
                .ToListAsync();

            // So sánh bằng chuỗi để tránh lệch múi giờ
            var activeLog = logs.FirstOrDefault(l =>
                l.WorkDate.Value.ToString("yyyy-MM-dd") == todayStr &&
                l.CheckOutTime == null
            );

            if (activeLog != null)
            {
                return Ok(new { status = 1, logId = activeLog.Id.ToString() });
            }

            var finishedLogs = logs.Where(l =>
                l.WorkDate.Value.ToString("yyyy-MM-dd") == todayStr &&
                l.CheckOutTime != null
            ).ToList();

            if (finishedLogs.Count > 0)
            {
                return Ok(new { status = 2, logId = (string?)null });
            }

            return Ok(new { status = 0, logId = (string?)null });
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