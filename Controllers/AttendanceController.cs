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
            var todayLocal = DateTime.UtcNow.AddHours(7).Date;

            // 1. ƯU TIÊN SỐ 1: Tìm ca làm đang "mở" (chưa checkout)
            var activeLog = await _context.AttendanceLogs
    .Where(l => l.EmployeeId == userId && l.CheckOutTime == null)
    .OrderByDescending(l => l.CheckInTime)
    .FirstOrDefaultAsync();

            if (activeLog != null)
            {
                // Có ca đang mở -> Bắt buộc hiện nút TAN CA (Status = 1)
                return Ok(new { status = 1, logId = activeLog.Id.ToString() });
            }

            // 2. NẾU KHÔNG CÓ CA NÀO MỞ: Kiểm tra xem đã hoàn thành hết các ca trong ngày chưa
            var finishedLogs = await _context.AttendanceLogs
                .Where(l => l.EmployeeId == userId && l.WorkDate == todayLocal && l.CheckOutTime != null)
                .ToListAsync();

            if (finishedLogs.Count > 0)
            {
                // Đã có ca làm xong -> Hiện trạng thái hoàn thành (Status = 2)
                return Ok(new { status = 2, logId = (string?)null });
            }

            // 3. Nếu không có gì hết -> Hiện nút VÀO CA (Status = 0)
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