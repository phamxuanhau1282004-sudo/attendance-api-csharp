using AttendanceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkShift> WorkShifts { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }
    }
}