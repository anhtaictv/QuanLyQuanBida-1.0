using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanBida.Core.Entities
{
    public class InventoryTransaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // "IN", "OUT", "ADJUST", "SALE"

        public int Quantity { get; set; }

        [MaxLength(255)]
        public string? Reference { get; set; } // Ghi chú: "Phiếu nhập 123", "Order 456"

        public int CreatedBy { get; set; } // UserId
        public User Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}