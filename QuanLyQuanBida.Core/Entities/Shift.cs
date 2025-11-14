using System.ComponentModel.DataAnnotations;
namespace QuanLyQuanBida.Core.Entities;
public class Shift
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal ClosingCash { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // THÊM DÒNG NÀY VÀO:
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "OPEN";
}