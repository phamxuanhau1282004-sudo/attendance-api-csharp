using System.ComponentModel.DataAnnotations;

namespace AttendanceApi.DTOs
{
    public class RegisterRequest
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }
}