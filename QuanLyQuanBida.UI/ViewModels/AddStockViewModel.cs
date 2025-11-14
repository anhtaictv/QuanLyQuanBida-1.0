using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class AddStockViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;
        private readonly ICurrentUserService _currentUserService;
        public Action? CloseAction { get; set; }
        [ObservableProperty]
        private ObservableCollection<Product> _products = new();
        [ObservableProperty]
        private int _selectedProductId;
        [ObservableProperty]
        private int _quantity;
        [ObservableProperty]
        private string _reference = string.Empty;
        public AddStockViewModel(
            IProductService productService,
            IInventoryService inventoryService,
            ICurrentUserService currentUserService)
        {
            _productService = productService;
            _inventoryService = inventoryService;
            _currentUserService = currentUserService;
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
        private async Task Save()
        {
            if (SelectedProductId <= 0 || Quantity <= 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm và nhập số lượng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                var success = await _inventoryService.UpdateStockAsync(
                    SelectedProductId,
                    Quantity,
                    "IN", 
                    Reference,
                    _currentUserService.CurrentUser?.Id ?? 0);
                if (success)
                {
                    MessageBox.Show("Nhập hàng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.CloseAction?.Invoke();
                }
                else
                {
                    MessageBox.Show("Nhập hàng thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nhập hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        [RelayCommand]
        private void Cancel()
        {
            CloseAction?.Invoke();
        }
    }
}