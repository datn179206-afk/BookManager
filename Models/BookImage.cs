using System.ComponentModel.DataAnnotations.Schema;

namespace BookManager.Models;

public class BookImage
{
    public int Id { get; set; }
    public int BookId { get; set; }

    // Tên file đã lưu trên server (wwwroot/uploads/books)
    public string FileName { get; set; } = "";

    public Book? Book { get; set; }

    [NotMapped]
    public string Url => $"/uploads/books/{FileName}";
}
