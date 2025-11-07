using System.ComponentModel.DataAnnotations;

public class Rate
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal PricePerHour { get; set; }

    public TimeOnly StartTimeWindow { get; set; }
    public TimeOnly EndTimeWindow { get; set; }

    public bool IsWeekendRate { get; set; }
}