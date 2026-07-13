using AttendanceApi.Data;
using AttendanceApi.DTOs;
using AttendanceApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (_context.Employees.Any(e => e.Email == request.Email))
                return BadRequest(new { status = "error", message = "Email đã tồn tại!" });

            var emp = new Employee
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                EmployeeCode = "EMP" + DateTime.Now.Ticks.ToString().Substring(10),
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };

            _context.Employees.Add(emp);
            _context.SaveChanges();

            return Ok(new { status = "success", employeeId = emp.Id });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Employees.FirstOrDefault(e => e.Email == request.Email && e.Password == request.Password);
            if (user == null) return Unauthorized(new { status = "error", message = "Sai email hoặc mật khẩu" });

            return Ok(new
            {
                status = "success",
                token = "fake-jwt-token",
                employeeId = user.Id,
                user = new
                {
                    id = user.Id,
                    fullName = user.FullName
                }
            });
        }
    }
}