using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
}