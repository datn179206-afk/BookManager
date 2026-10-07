using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookManager.Models;

public class Book
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên sách")]
    [StringLength(200)]
    [Display(Name = "Tên sách")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập tác giả")]
    [StringLength(100)]
    [Display(Name = "Tác giả")]
    public string Author { get; set; } = "";

    [StringLength(100)]
    [Display(Name = "Thể loại")]
    public string? Category { get; set; }

    [Range(1, 100000000, ErrorMessage = "Giá phải lớn hơn 0")]
    [Display(Name = "Giá (VNĐ)")]
    public decimal Price { get; set; }

    [Range(1000, 2100, ErrorMessage = "Năm xuất bản không hợp lệ")]
    [Display(Name = "Năm xuất bản")]
    public int PublishedYear { get; set; } = DateTime.Now.Year;

    [Range(0, int.MaxValue, ErrorMessage = "Số lượng không hợp lệ")]
    [Display(Name = "Số lượng")]
    public int Quantity { get; set; }

    [StringLength(1000)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    // Danh sách hình ảnh của sách (1 hoặc nhiều ảnh)
    [ValidateNever]
    public List<BookImage> Images { get; set; } = new();
}
