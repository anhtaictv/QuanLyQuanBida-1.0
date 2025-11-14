using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces;
public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();

    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(ProductDto productDto);
    Task<bool> UpdateProductAsync(ProductDto productDto);
    Task<bool> DeleteProductAsync(int id);
}