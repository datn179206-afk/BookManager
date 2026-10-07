using BookManager.Models;

namespace BookManager.Services;

// Xử lý upload ảnh: chỉ cho jpg/png, tối đa 5MB, lưu vào wwwroot/uploads/books
public class ImageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/jpg", "image/png" };
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _env;
    public ImageService(IWebHostEnvironment env) => _env = env;

    private string UploadFolder =>
        Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "books");

    // Trả về thông báo lỗi nếu có file không hợp lệ; null nếu tất cả hợp lệ
    public async Task<string?> ValidateAsync(List<IFormFile> files)
    {
        foreach (var f in files)
        {
            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(ext) || !AllowedContentTypes.Contains(f.ContentType.ToLowerInvariant()))
                return $"File \"{f.FileName}\" không hợp lệ. Chỉ cho phép upload ảnh jpg/png.";

            if (f.Length > MaxFileSize)
                return $"File \"{f.FileName}\" vượt quá 5MB.";

            if (!await HasValidSignatureAsync(f, ext))
                return $"File \"{f.FileName}\" không phải ảnh jpg/png thật. Chỉ cho phép upload ảnh jpg/png.";
        }
        return null;
    }

    // Kiểm tra nội dung file thật sự là jpg/png (chống đổi đuôi file)
    private static async Task<bool> HasValidSignatureAsync(IFormFile file, string ext)
    {
        var header = new byte[8];
        using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header, 0, header.Length);

        if (ext == ".png")
            return read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;

        return read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF; // jpg/jpeg
    }

    // Lưu file với tên ngẫu nhiên (tránh trùng tên), trả về danh sách BookImage
    public async Task<List<BookImage>> SaveAsync(List<IFormFile> files)
    {
        Directory.CreateDirectory(UploadFolder);
        var result = new List<BookImage>();
        foreach (var f in files)
        {
            var name = Guid.NewGuid().ToString("N") + Path.GetExtension(f.FileName).ToLowerInvariant();
            using var stream = new FileStream(Path.Combine(UploadFolder, name), FileMode.Create);
            await f.CopyToAsync(stream);
            result.Add(new BookImage { FileName = name });
        }
        return result;
    }

    public void Delete(string fileName)
    {
        var path = Path.Combine(UploadFolder, fileName);
        if (File.Exists(path)) File.Delete(path);
    }
}
