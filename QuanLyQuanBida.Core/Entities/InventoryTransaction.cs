using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanBida.Core.Entities;

public class InventoryTransaction
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Type { get; set; } = string.Empty; // IN, OUT, ADJUST

    public decimal Quantity { get; set; }

    [MaxLength(255)]
    public string? Reference { get; set; } // Reference to order, invoice, etc.

    public int CreatedBy { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}