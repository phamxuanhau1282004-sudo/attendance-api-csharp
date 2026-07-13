using AttendanceApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký Database (giữ nguyên)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // BẮT BUỘC CÓ DÒNG NÀY
builder.Services.AddSwaggerGen();           // Dùng Swagger chuẩn

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Giao diện Swagger cũ mà chắc chắn chạy
}

app.UseAuthorization();
app.MapControllers();
app.Run();