using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Infrastructure.Data.Context;

public class QuanLyBidaDbContext : DbContext
{
    public QuanLyBidaDbContext(DbContextOptions<QuanLyBidaDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Rate> Rates { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Setting> Settings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === THÊM CẤU HÌNH QUAN HỆ NÀY ===
        modelBuilder.Entity<Session>()
            .HasMany(s => s.Orders)
            .WithOne(o => o.Session)
            .HasForeignKey(o => o.SessionId);
        // ==================================

        // Các cấu hình khác có thể để ở đây
        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasDefaultValue("Free");
        });
    }
}