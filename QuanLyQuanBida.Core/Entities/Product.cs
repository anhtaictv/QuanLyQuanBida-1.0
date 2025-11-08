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
    public bool IsInventoryTracked { get; set; } = true;

    public int Quantity { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Category { get; set; } 

    public string? ImageUrl { get; set; }

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
}
}