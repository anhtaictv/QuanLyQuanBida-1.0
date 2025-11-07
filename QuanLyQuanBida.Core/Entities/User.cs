using System.ComponentModel.DataAnnotations;
using System.Data;

namespace QuanLyQuanBida.Core.Entities;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FullName { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}