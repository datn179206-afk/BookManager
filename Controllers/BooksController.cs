using BookManager.Data;
using BookManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Controllers;

public class BooksController : Controller
{
    private readonly AppDbContext _db;
    public BooksController(AppDbContext db) => _db = db;

    // READ (danh sách + tìm kiếm)
    public async Task<IActionResult> Index(string? search)
    {
        var q = _db.Books.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
        ViewData["Search"] = search;
        return View(await q.OrderBy(b => b.Id).ToListAsync());
    }

    // READ (chi tiết)
    public async Task<IActionResult> Details(int? id)
    {
        var book = id == null ? null : await _db.Books.FindAsync(id);
        return book == null ? NotFound() : View(book);
    }

    // CREATE
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        if (!ModelState.IsValid) return View(book);
        _db.Add(book);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Thêm sách thành công!";
        return RedirectToAction(nameof(Index));
    }

    // UPDATE
    public async Task<IActionResult> Edit(int? id)
    {
        var book = id == null ? null : await _db.Books.FindAsync(id);
        return book == null ? NotFound() : View(book);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book)
    {
        if (id != book.Id) return NotFound();
        if (!ModelState.IsValid) return View(book);
        try
        {
            _db.Update(book);
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
        var book = id == null ? null : await _db.Books.FindAsync(id);
        return book == null ? NotFound() : View(book);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book != null)
        {
            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
        }
        TempData["Msg"] = "Đã xóa sách!";
        return RedirectToAction(nameof(Index));
    }
}
