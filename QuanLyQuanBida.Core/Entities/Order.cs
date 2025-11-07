using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanBida.Core.Entities;

public class Order
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; } // Giá tại thời điểm gọi món

    [MaxLength(255)]
    public string? Note { get; set; } // Ghi chú: "ít đường", "không đá"...

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}