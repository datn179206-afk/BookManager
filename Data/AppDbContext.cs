using BookManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookImage> BookImages => Set<BookImage>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Book>().Property(b => b.Price).HasColumnType("decimal(18,0)");
        mb.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Dế Mèn phiêu lưu ký", Author = "Tô Hoài", Category = "Thiếu nhi", Price = 55000, PublishedYear = 1941, Quantity = 20 },
            new Book { Id = 2, Title = "Clean Code", Author = "Robert C. Martin", Category = "Lập trình", Price = 320000, PublishedYear = 2008, Quantity = 10 }
        );
    }
}
