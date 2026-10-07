using BookManager.Data;
using BookManager.Models;
using BookManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Controllers.Api;

// Web API: trả JSON, URL bắt đầu bằng /api/book
[ApiController]
[Route("api/book")]
public class BookApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ImageService _images;

    public BookApiController(AppDbContext db, ImageService images)
    {
        _db = db;
        _images = images;
    }

    // GET /api/book  -> 200 OK (danh sách sách)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
    {
        var books = await _db.Books.Include(b => b.Images).OrderBy(b => b.Id).ToListAsync();
        return Ok(books.Select(BookDto.From));
    }

    // GET /api/book/5  -> 200 OK hoặc 404 Not Found
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await _db.Books.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
        return book == null ? NotFound() : Ok(BookDto.From(book));
    }

    // POST /api/book (Content-Type: application/json) -> 201 Created
    [HttpPost, Consumes("application/json")]
    public Task<ActionResult<BookDto>> CreateJson(Book book) => CreateCore(book, new());

    // POST /api/book (Content-Type: multipart/form-data, form web có ảnh) -> 201 Created
    [HttpPost, Consumes("multipart/form-data")]
    public Task<ActionResult<BookDto>> CreateForm([FromForm] Book book, [FromForm] List<IFormFile>? uploadFiles)
        => CreateCore(book, uploadFiles ?? new());

    private async Task<ActionResult<BookDto>> CreateCore(Book book, List<IFormFile> uploadFiles)
    {
        var files = uploadFiles.Where(f => f.Length > 0).ToList();

        // File không phải jpg/png -> 400 Bad Request
        var error = await _images.ValidateAsync(files);
        if (error != null)
        {
            ModelState.AddModelError("uploadFiles", error);
            return ValidationProblem(ModelState);
        }

        // Dữ liệu sai (giá = 0, thiếu tên...) đã được [ApiController] tự trả 400 trước khi vào đây
        book.Id = 0; // để database tự sinh Id
        book.Images = await _images.SaveAsync(files);

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        // 201 Created + header Location trỏ tới GET /api/book/{id}
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, BookDto.From(book));
    }
}

// Dữ liệu trả về cho client (tránh vòng lặp Book <-> BookImage khi chuyển sang JSON)
public record BookDto(
    int Id, string Title, string Author, string? Category,
    decimal Price, int PublishedYear, int Quantity, string? Description,
    List<string> ImageUrls)
{
    public static BookDto From(Book b) => new(
        b.Id, b.Title, b.Author, b.Category, b.Price, b.PublishedYear, b.Quantity,
        b.Description, b.Images.Select(i => i.Url).ToList());
}
