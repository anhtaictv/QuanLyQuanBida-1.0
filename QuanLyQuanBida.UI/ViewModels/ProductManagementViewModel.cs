using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;
using QuanLyQuanBida.Core.DTOs; // <-- THÊM
using System.Linq; // <-- THÊM
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class ProductManagementViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService; 

        [ObservableProperty]
        private ObservableCollection<Product> _products = new();
        [ObservableProperty]
        private Product? _selectedProduct;
        [ObservableProperty]
        private ProductDto _productForm = new();
        [ObservableProperty]
        private string _searchText = string.Empty;
        [ObservableProperty]
        private bool _isEditing = false;

        public ProductManagementViewModel(IProductService productService, IAuditService auditService, ICurrentUserService currentUserService)
        {
            _productService = productService;
            _auditService = auditService; 
            _currentUserService = currentUserService;
            _ = LoadProductsAsync();
        }
        private async Task LoadProductsAsync()
        {
            Products.Clear();
            var productsFromDb = await _productService.GetAllProductsAsync();

            var filteredList = string.IsNullOrWhiteSpace(SearchText)
                ? productsFromDb
                : productsFromDb.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var product in filteredList)
            {
                Products.Add(product);
            }
        }
        [RelayCommand]
        private async Task Search()
        {
            await LoadProductsAsync();
        }

        [RelayCommand]
        private void AddNew()
        {
            SelectedProduct = null;
            ProductForm = new ProductDto { Unit = "Cái", IsInventoryTracked = true }; 
            IsEditing = false;
        }
        [RelayCommand]
        private void EditProduct(Product product)
        {
            if (product == null) return;
            SelectedProduct = product;
            ProductForm = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Category = product.Category,
                Unit = product.Unit,
                IsInventoryTracked = product.IsInventoryTracked
            };
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveProduct()
        {
            if (string.IsNullOrWhiteSpace(ProductForm.Name) || ProductForm.Price <= 0)
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin bắt buộc.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var currentUserId = _currentUserService.CurrentUser?.Id ?? 0;
            string oldValue = null;

            try
            {
                if (IsEditing)
                {
                    var existing = await _productService.GetProductByIdAsync(ProductForm.Id);
                    oldValue = $"Name: {existing.Name}, Price: {existing.Price}";

                    bool success = await _productService.UpdateProductAsync(ProductForm);
                    if (success)
                    {
                        await _auditService.LogActionAsync(currentUserId, "UPDATE_PRODUCT", "Products", ProductForm.Id, oldValue, $"Name: {ProductForm.Name}, Price: {ProductForm.Price}");
                    }
                }
                else
                {
                    var newProduct = await _productService.CreateProductAsync(ProductForm);
                    if (newProduct != null)
                    {
                        await _auditService.LogActionAsync(currentUserId, "CREATE_PRODUCT", "Products", newProduct.Id, newValue: $"Name: {newProduct.Name}, Price: {newProduct.Price}");
                    }
                }
                MessageBox.Show("Lưu thông tin sản phẩm thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadProductsAsync();
                AddNew(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        [RelayCommand]
        private void Cancel()
        {
            AddNew();
        }
        [RelayCommand]
        private async Task DeleteProduct(Product product)
        {
            if (product == null) return;
            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa sản phẩm '{product.Name}'?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _productService.DeleteProductAsync(product.Id);
                    if (success)
                    {
                        await _auditService.LogActionAsync(_currentUserService.CurrentUser?.Id ?? 0, "DELETE_PRODUCT", "Products", product.Id, oldValue: $"Name: {product.Name}");
                        await LoadProductsAsync();
                        MessageBox.Show("Xóa sản phẩm thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Không thể xóa sản phẩm. (Có thể sản phẩm đã được sử dụng trong hóa đơn).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa sản phẩm: {ex.Message} (Sản phẩm có thể đang được sử dụng)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        [RelayCommand]
        private void ImportFromExcel()
        {
            MessageBox.Show("TODO: Chức năng Import Excel");
        }
    }
}