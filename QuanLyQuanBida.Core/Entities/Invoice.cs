using QuanLyQuanBida.Core.Entities;
using System.ComponentModel.DataAnnotations;

public class Invoice
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }

    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    public int CreatedBy { get; set; }
    public User Creator { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}