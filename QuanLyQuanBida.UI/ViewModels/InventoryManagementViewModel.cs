using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.DTOs; // Cần tạo InventoryAdjustmentDto
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    // DTO cho Form điều chỉnh
    public partial class InventoryAdjustmentDto : ObservableObject
    {
        [ObservableProperty]
        private int _productId;
        [ObservableProperty]
        private string _transactionType = "IN"; // "IN", "OUT", "ADJUST"
        [ObservableProperty]
        private int _quantity;
        [ObservableProperty]
        private string? _reference;
    }

    public partial class InventoryManagementViewModel : ObservableObject
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;
        private readonly ICurrentUserService _currentUserService;

        [ObservableProperty]
        private ObservableCollection<Product> _trackedProducts = new(); // DS Sản phẩm được theo dõi

        [ObservableProperty]
        private InventoryAdjustmentDto _adjustmentForm = new();

        public ObservableCollection<string> TransactionTypes { get; } = new() { "IN", "OUT", "ADJUST" };

        public InventoryManagementViewModel(IInventoryService inventoryService, IProductService productService, ICurrentUserService currentUserService)
        {
            _inventoryService = inventoryService;
            _productService = productService;
            _currentUserService = currentUserService;
            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            TrackedProducts.Clear();
            var allProducts = await _productService.GetAllProductsAsync();
            // Chỉ hiển thị các sản phẩm có theo dõi tồn kho
            foreach (var product in allProducts.Where(p => p.IsInventoryTracked))
            {
                TrackedProducts.Add(product);
            }
        }

        [RelayCommand]
        private async Task AdjustStock()
        {
            if (_currentUserService.CurrentUser == null)
            {
                MessageBox.Show("Lỗi: Không tìm thấy người dùng hiện tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (AdjustmentForm.ProductId == 0 || AdjustmentForm.Quantity <= 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm và nhập số lượng hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var userId = _currentUserService.CurrentUser.Id;

            bool success = await _inventoryService.UpdateStockAsync(
                AdjustmentForm.ProductId,
                AdjustmentForm.Quantity,
                AdjustmentForm.TransactionType,
                AdjustmentForm.Reference ?? "Điều chỉnh thủ công",
                userId
            );

            if (success)
            {
                MessageBox.Show("Cập nhật tồn kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadProductsAsync(); // Tải lại danh sách
                // Reset form
                AdjustmentForm = new InventoryAdjustmentDto { TransactionType = AdjustmentForm.TransactionType };
            }
            else
            {
                MessageBox.Show("Cập nhật tồn kho thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}