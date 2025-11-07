using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanBida.Core.Entities;

public class Session
{
    public int Id { get; set; }

    public int TableId { get; set; }
    public Table Table { get; set; } = null!;

    public int UserOpenId { get; set; } // Nhân viên mở bàn
    public User UserOpen { get; set; } = null!;

    public DateTime StartAt { get; set; }

    public DateTime? PauseAt { get; set; }
    public DateTime? ResumeAt { get; set; }
    public DateTime? EndAt { get; set; }

    public string Status { get; set; } = "Started"; // Started, Paused, Finished

    public int TotalMinutes { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;
}