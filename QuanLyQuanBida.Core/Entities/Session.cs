using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace QuanLyQuanBida.Core.Entities;
public class Session
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public Table Table { get; set; } = null!;
    public int UserOpenId { get; set; }
    public User UserOpen { get; set; } = null!;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int RateId { get; set; }
    public Rate Rate { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime? PauseAt { get; set; }
    public DateTime? ResumeAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string Status { get; set; } = "Started";
    public int TotalMinutes { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
