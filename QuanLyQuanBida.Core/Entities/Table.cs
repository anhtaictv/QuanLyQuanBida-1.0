using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanBida.Core.Entities;

public class Table
{
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Zone { get; set; }

    public int Seats { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Free";

    [MaxLength(255)]
    public string? Note { get; set; }
}