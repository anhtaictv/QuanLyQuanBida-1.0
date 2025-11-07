using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanBida.Core.Entities;

public class Payment
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    [MaxLength(50)]
    public string Method { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [MaxLength(100)]
    public string? TransactionRef { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}