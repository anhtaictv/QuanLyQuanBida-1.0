using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.DTOs; 
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services;
public class ProductService : IProductService
{

    private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

    public ProductService(IDbContextFactory<QuanLyBidaDbContext> contextFactory) 
    {
        _contextFactory = contextFactory; 
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products.ToListAsync();
    }


    public async Task<Product?> GetProductByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products.FindAsync(id);
    }

    public async Task<Product> CreateProductAsync(ProductDto productDto)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var newProduct = new Product
        {
            Name = productDto.Name,
            Price = productDto.Price,
            Category = productDto.Category,
            Unit = productDto.Unit,
            IsInventoryTracked = productDto.IsInventoryTracked,
            Quantity = 0
        };
        context.Products.Add(newProduct);
        await context.SaveChangesAsync();
        return newProduct;
    }

    public async Task<bool> UpdateProductAsync(ProductDto productDto)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var product = await context.Products.FindAsync(productDto.Id);
        if (product == null) return false;

        product.Name = productDto.Name;
        product.Price = productDto.Price;
        product.Category = productDto.Category;
        product.Unit = productDto.Unit;
        product.IsInventoryTracked = productDto.IsInventoryTracked;

        context.Products.Update(product);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var product = await context.Products.FindAsync(id);
        if (product == null) return false;

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return true;
    }
}