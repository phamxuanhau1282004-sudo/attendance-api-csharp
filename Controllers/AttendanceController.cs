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

            // Tìm ca làm nào trong ngày hôm nay mà NHÂN VIÊN VẪN CHƯA TAN CA (CheckOutTime là null)
            var activeLog = await _context.AttendanceLogs
                .Where(l => l.EmployeeId == userId && l.WorkDate == todayLocal && l.CheckOutTime == null)
                .FirstOrDefaultAsync();

            if (activeLog != null)
            {
                // Có ca đang mở -> Đang trong ca (Status = 1)
                return Ok(new { status = 1, logId = activeLog.Id.ToString() });
            }

            // Nếu không có ca nào đang mở, kiểm tra xem đã hết ca hôm nay chưa
            // (Tuỳ logic công ty: Nếu đã có ca check-out rồi thì trả về 2)
            var finishedLogs = await _context.AttendanceLogs
                .Where(l => l.EmployeeId == userId && l.WorkDate == todayLocal && l.CheckOutTime != null)
                .ToListAsync();

            // Nếu hôm nay đã có ít nhất 1 ca đã hoàn thành
            if (finishedLogs.Count > 0)
            {
                return Ok(new { status = 2, logId = (string?)null });
            }

            // Nếu chưa có gì cả -> Trạng thái sẵn sàng vào ca mới (Status = 0)
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