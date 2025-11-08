using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Infrastructure.Data.Context
{
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
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === CẤU HÌNH QUAN HỆ ===
            modelBuilder.Entity<Session>()
                .HasMany(s => s.Orders)
                .WithOne(o => o.Session)
                .HasForeignKey(o => o.SessionId);

            // ⚙️ Sửa lỗi Multiple Cascade Paths ở đây:
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Creator)          // liên kết đến User
                .WithMany()                      // không cần navigation ngược
                .HasForeignKey(i => i.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict); // không cascade để tránh xung đột

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Session)
                .WithMany()
                .HasForeignKey(i => i.SessionId)
                .OnDelete(DeleteBehavior.Cascade); // cho phép cascade 1 chiều này

            modelBuilder.Entity<Table>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasDefaultValue("Free");
            });

            modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Owner" },
            new Role { Id = 2, Name = "Manager" },
            new Role { Id = 3, Name = "Cashier" },
            new Role { Id = 4, Name = "Staff" }
        );

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.Property(e => e.Discount).HasPrecision(18, 2);
                entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
                entity.Property(e => e.ServiceFee).HasPrecision(18, 2);
                entity.Property(e => e.SubTotal).HasPrecision(18, 2);
                entity.Property(e => e.Tax).HasPrecision(18, 2);
                entity.Property(e => e.Total).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Rate>(entity =>
            {
                entity.Property(e => e.PricePerHour).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            });
            // Cấu hình cho InventoryTransaction
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.Property(e => e.Quantity).HasPrecision(18, 2);
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.InventoryTransactions)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình cho Shift
            modelBuilder.Entity<Shift>(entity =>
            {
                entity.Property(e => e.OpeningCash).HasPrecision(18, 2);
                entity.Property(e => e.ClosingCash).HasPrecision(18, 2);
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình cho AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.Property(e => e.OldValue).HasColumnType("nvarchar(max)");
                entity.Property(e => e.NewValue).HasColumnType("nvarchar(max)");
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            var adminPasswordHash = "$2y$12$RQm3JerPsue4jHpbFe2mKuPQuvhPQbY4/8ZZaVC3nNiPqtIoZ3R06";
        }
    }
}
