using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanBida.Core.Entities;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; } // Ví dụ: "Đồ uống", "Thức ăn", "Thuốc lá"

    public string? ImageUrl { get; set; } // Đường dẫn ảnh (làm sau)
}