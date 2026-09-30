# BookManager – ASP.NET Core MVC + EF Core (Code First) + SQL Server

## Yêu cầu
- .NET 8 SDK, SQL Server (LocalDB / Express / Docker đều được)

## Chạy
1. Sửa chuỗi kết nối trong `appsettings.json` (mục DefaultConnection).
2. Cài công cụ EF (1 lần): `dotnet tool install --global dotnet-ef`
3. Tạo migration (Code First): `dotnet ef migrations add InitialCreate`
4. Chạy: `dotnet run` (app tự gọi `Database.Migrate()` để tạo/cập nhật DB).
   Hoặc tạo DB thủ công: `dotnet ef database update`

## Khi sửa Model
`dotnet ef migrations add TenThayDoi` -> chạy lại app (hoặc `dotnet ef database update`).

## Dùng CSDL khác
Đổi package `...EntityFrameworkCore.SqlServer` sang provider khác (Npgsql, Pomelo MySQL, Sqlite)
và thay `UseSqlServer` trong Program.cs.
