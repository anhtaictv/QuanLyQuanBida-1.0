using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class ProductManagementViewModel : ObservableObject
    {
        private readonly IProductService _productService;

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

        public ProductManagementViewModel(IProductService productService)
        {
            _productService = productService;
            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            Products.Clear();

            var productsFromDb = await _productService.GetAllProductsAsync();

            foreach (var product in productsFromDb)
            {
                Products.Add(product);
            }
        }

        [RelayCommand]
        private async Task Search()
        {
            // Implement search logic
            await LoadProductsAsync();
        }

        [RelayCommand]
        private void AddNew()
        {
            SelectedProduct = null;
            ProductForm = new ProductDto();
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

            try
            {
                if (IsEditing)
                {
                    // Update existing product
                    // Implement update logic
                }
                else
                {
                    // Create new product
                    // Implement create logic
                }

                MessageBox.Show("Lưu thông tin sản phẩm thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadProductsAsync();
                AddNew(); // Reset form
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            AddNew(); // Reset form
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
                    // Implement delete logic
                    await LoadProductsAsync();
                    MessageBox.Show("Xóa sản phẩm thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void ImportFromExcel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Title = "Chọn file Excel để nhập sản phẩm"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Implement Excel import logic
                    MessageBox.Show("Nhập sản phẩm từ Excel thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    _ = LoadProductsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi nhập sản phẩm từ Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public string? Unit { get; set; }
        public bool IsInventoryTracked { get; set; }
    }
}