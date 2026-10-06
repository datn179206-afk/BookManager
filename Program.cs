using System.Globalization;
using BookManager.Data;
using BookManager.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Dùng dấu chấm cho số thập phân để form nhập giá không bị lỗi
var culture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Tự động áp dụng migration -> đồng bộ CSDL SQL Server khi chạy app
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseStaticFiles();
app.UseRouting();

// Middleware ghi log + chặn id không hợp lệ (phải đặt TRƯỚC MapControllerRoute)
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllerRoute(name: "default", pattern: "{controller=Book}/{action=Index}/{id?}");
app.Run();
