using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Thêm using này
using System.Collections.Generic; // Thêm using này

namespace QuanLyQuanBida.Core.Entities;

public class Session
{
    public int Id { get; set; }

    public int TableId { get; set; }
    public Table Table { get; set; } = null!;

    public int UserOpenId { get; set; }
    public User UserOpen { get; set; } = null!;

    public DateTime StartAt { get; set; }

    public DateTime? PauseAt { get; set; }
    public DateTime? ResumeAt { get; set; }
    public DateTime? EndAt { get; set; }

    public string Status { get; set; } = "Started";

    public int TotalMinutes { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;


    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
   
}