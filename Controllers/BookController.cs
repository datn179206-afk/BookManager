using BookManager.Data;
using BookManager.Models;
using BookManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Controllers;

public class BookController : Controller
{
    private readonly AppDbContext _db;
    private readonly ImageService _images;

    public BookController(AppDbContext db, ImageService images)
    {
        _db = db;
        _images = images;
    }

    // READ (danh sách + tìm kiếm) - hiển thị Tên, Giá, Hình ảnh
    public async Task<IActionResult> Index(string? search)
    {
        var q = _db.Books.Include(b => b.Images).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
        ViewData["Search"] = search;
        return View(await q.OrderBy(b => b.Id).ToListAsync());
    }

    // READ (chi tiết)
    public async Task<IActionResult> Detail(int? id)
    {
        var book = id == null ? null : await _db.Books.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
        return book == null ? NotFound() : View(book);
    }

    // CREATE
    public IActionResult Create() => View(new Book());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book, List<IFormFile>? uploadFiles)
    {
        var files = (uploadFiles ?? new()).Where(f => f.Length > 0).ToList();

        var error = await _images.ValidateAsync(files);
        if (error != null) ModelState.AddModelError(string.Empty, error);

        if (!ModelState.IsValid) return View(book);

        book.Images = await _images.SaveAsync(files);
        _db.Add(book);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Thêm sách thành công!";
        return RedirectToAction(nameof(Index));
    }

    // UPDATE
    public async Task<IActionResult> Edit(int? id)
    {
        var book = id == null ? null : await _db.Books.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
        return book == null ? NotFound() : View(book);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book, List<IFormFile>? uploadFiles, int[]? deleteImageIds)
    {
        if (id != book.Id) return NotFound();

        var files = (uploadFiles ?? new()).Where(f => f.Length > 0).ToList();
        var error = await _images.ValidateAsync(files);
        if (error != null) ModelState.AddModelError(string.Empty, error);

        if (!ModelState.IsValid)
        {
            book.Images = await _db.BookImages.Where(i => i.BookId == id).ToListAsync();
            return View(book);
        }

        try
        {
            _db.Update(book);

            // Xóa các ảnh được tích chọn (xóa cả file trên server)
            if (deleteImageIds is { Length: > 0 })
            {
                var toDelete = await _db.BookImages
                    .Where(i => i.BookId == id && deleteImageIds.Contains(i.Id)).ToListAsync();
                foreach (var img in toDelete) _images.Delete(img.FileName);
                _db.BookImages.RemoveRange(toDelete);
            }

            // Thêm các ảnh mới
            foreach (var img in await _images.SaveAsync(files))
            {
                img.BookId = id;
                _db.BookImages.Add(img);
            }

            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Books.AnyAsync(b => b.Id == id)) return NotFound();
            throw;
        }
        TempData["Msg"] = "Cập nhật thành công!";
        return RedirectToAction(nameof(Index));
    }

    // DELETE
    public async Task<IActionResult> Delete(int? id)
    {
        var book = id == null ? null : await _db.Books.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
        return book == null ? NotFound() : View(book);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _db.Books.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
        if (book != null)
        {
            foreach (var img in book.Images) _images.Delete(img.FileName);
            _db.Books.Remove(book); // ảnh trong DB tự xóa theo (cascade)
            await _db.SaveChangesAsync();
        }
        TempData["Msg"] = "Đã xóa sách!";
        return RedirectToAction(nameof(Index));
    }
}
