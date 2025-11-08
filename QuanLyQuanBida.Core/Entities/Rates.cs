using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanBida.Core.Entities
{
    public class Rate
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal PricePerHour { get; set; }

        public TimeOnly? StartTimeWindow { get; set; }
        public TimeOnly? EndTimeWindow { get; set; }

        public bool IsWeekendRate { get; set; } = false;

        public bool IsDefault { get; set; } = false;
    }
}
