using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyQuanBida.Core.DTOs;
public class RevenueByDayDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int SessionsCount { get; set; }
}
public class RevenueByTableDto
{
    public string TableCode { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int SessionsCount { get; set; }
    public decimal UtilizationRate { get; set; }
}
public class RevenueByProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}
public class CustomerDebtDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal DebtAmount { get; set; }
    public DateTime LastTransactionDate { get; set; }
}
public class InventoryReportDto
{
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinStock { get; set; }
    public bool IsLowStock { get; set; }
}
public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TimeCharge { get; set; }
    public decimal OrderTotal { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal Total { get; set; }
    public List<OrderDto> Orders { get; set; } = new(); 
}
public class PaymentDto
{
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TransactionRef { get; set; }
}
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Type { get; set; } = "Walk-in";
    public string? VipCardNumber { get; set; }
}
public class OrderDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; } 
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string Note { get; set; } = string.Empty;
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsInventoryTracked { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; 
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
}