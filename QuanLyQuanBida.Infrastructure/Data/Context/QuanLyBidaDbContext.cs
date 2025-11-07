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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Table entity nếu cần
        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasDefaultValue("Available");
            //entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        });
    }
    public DbSet<Session> Sessions { get; set; }
}