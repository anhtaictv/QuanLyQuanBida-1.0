using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using QuanLyQuanBida.Core.DTOs; // <-- THÊM
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Migrations;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq; // <-- THÊM
using System.Windows;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;


namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class ProductManagementViewModel : ObservableObject, IRecipient<ProductsChangedMessage>
    {
        private readonly IProductService _productService;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMessenger _messenger;

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
        [ObservableProperty]
        private ObservableCollection<string> _categories = new();
        [ObservableProperty]
        private ObservableCollection<string> _units = new();

        public ProductManagementViewModel(IProductService productService, IAuditService auditService, ICurrentUserService currentUserService, IMessenger messenger)
        {
            _productService = productService;
            _auditService = auditService; 
            _currentUserService = currentUserService;
            _messenger = messenger; // <-- THÊM
            _messenger.RegisterAll(this);
            _ = LoadProductsAsync();
        }
        private async Task LoadProductsAsync()
        {
            Products.Clear();
            var productsFromDb = await _productService.GetAllProductsAsync();
            Categories.Clear();
            Units.Clear();
            var distinctCategories = productsFromDb
        .Where(p => !string.IsNullOrEmpty(p.Category))
        .Select(p => p.Category!)
        .Distinct()
        .OrderBy(c => c);

            var distinctUnits = productsFromDb
                .Where(p => !string.IsNullOrEmpty(p.Unit))
                .Select(p => p.Unit)
                .Distinct()
                .OrderBy(u => u);

            foreach (var category in distinctCategories)
            {
                Categories.Add(category);
            }
            if (!distinctUnits.Contains("Cái")) Units.Add("Cái");
            if (!distinctUnits.Contains("Lon")) Units.Add("Lon");
            if (!distinctUnits.Contains("Chai")) Units.Add("Chai");
            if (!distinctUnits.Contains("Đĩa")) Units.Add("Đĩa");
            if (!distinctUnits.Contains("Thùng")) Units.Add("Thùng");

            foreach (var unit in distinctUnits.Where(u => !Units.Contains(u)))
            {
                Units.Add(unit);
            }

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
            ProductForm = new ProductDto { Unit = "Cái", IsInventoryTracked = true, Price = 10000 };
            IsEditing = false;
            SearchText = string.Empty; // <-- SỬA LỖI: Xóa bộ lọc tìm kiếm
        }
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
                _messenger.Send(new ProductsChangedMessage());
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
                        MessageBox.Show("Xóa sản phẩm thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        _messenger.Send(new ProductsChangedMessage());
                        
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

        [RelayCommand]
        private void AddNewCategory()
        {
            // Sử dụng InputBox của VisualBasic
            string newCategory = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên danh mục mới:", "Tạo Danh mục mới");

            if (!string.IsNullOrWhiteSpace(newCategory))
            {
                // 1. Thêm vào danh sách ComboBox
                if (!Categories.Contains(newCategory))
                {
                    Categories.Add(newCategory);
                }
                // 2. Tự động chọn danh mục đó
                ProductForm.Category = newCategory;
            }
        }

        [RelayCommand]
        private void AddNewUnit()
        {
            string newUnit = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên đơn vị mới (Vd: Thùng, Gói):", "Tạo Đơn vị mới");

            if (!string.IsNullOrWhiteSpace(newUnit))
            {
                if (!Units.Contains(newUnit))
                {
                    Units.Add(newUnit);
                }
                ProductForm.Unit = newUnit;
            }
        }
        public async void Receive(ProductsChangedMessage message)
        {
            // Khi nhận được tin nhắn, tải lại danh sách sản phẩm
            await LoadProductsAsync();
        }
    }
}