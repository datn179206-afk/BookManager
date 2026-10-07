using System.Diagnostics;

namespace BookManager.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // ===== Chức năng 1: ghi log request (TRƯỚC khi vào Controller) =====
        var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); // .fff = milliseconds
        var method = context.Request.Method;
        var path = context.Request.Path.ToString();
        var stopwatch = Stopwatch.StartNew(); // đo thời gian xử lý (ms)

        Console.WriteLine($"[{time}] Method: {method} - Path: {path}");

        // ===== Chức năng 3: chặn /Book/Detail/0, /Book/Detail/-1 =====
        if (IsInvalidDetailId(path))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("Book id không hợp lệ");

            Console.WriteLine($"Status Code: {context.Response.StatusCode} - {stopwatch.ElapsedMilliseconds} ms");
            return; // không gọi _next => request KHÔNG đi tiếp vào Controller
        }

        // Cho request đi tiếp tới middleware kế tiếp / Controller
        await _next(context);

        // ===== Chức năng 2: ghi log status code (SAU khi xử lý xong) =====
        Console.WriteLine($"Status Code: {context.Response.StatusCode} - {stopwatch.ElapsedMilliseconds} ms");
    }

    // Đúng dạng /Book/Detail/{id} và id là số <= 0 thì không hợp lệ
    private static bool IsInvalidDetailId(string path)
    {
        var segments = path.Trim('/').Split('/');

        return segments.Length == 3
            && segments[0].Equals("Book", StringComparison.OrdinalIgnoreCase)
            && segments[1].Equals("Detail", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(segments[2], out var id)
            && id <= 0;
    }
}
