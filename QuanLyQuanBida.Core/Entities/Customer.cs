using System.ComponentModel.DataAnnotations;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string Type { get; set; } = "Walk-in"; // Walk-in, VIP

    [MaxLength(50)]
    public string? VipCardNumber { get; set; }

    public int Points { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}