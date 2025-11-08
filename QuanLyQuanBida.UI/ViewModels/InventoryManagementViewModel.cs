using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class InventoryManagementViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;
        private readonly ICurrentUserService _currentUserService;

        [ObservableProperty]
        private ObservableCollection<Product> _products = new();

        public InventoryManagementViewModel(
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
        private void AddStock()
        {
            var addStockWindow = new AddStockWindow();
            if (addStockWindow.ShowDialog() == true)
            {
                _ = LoadProductsAsync();
            }
        }

        [RelayCommand]
        private void RemoveStock()
        {
            var removeStockWindow = new RemoveStockWindow();
            if (removeStockWindow.ShowDialog() == true)
            {
                _ = LoadProductsAsync();
            }
        }

        [RelayCommand]
        private async Task ShowLowStock()
        {
            var lowStockProducts = await _inventoryService.GetLowStockProductsAsync();

            if (lowStockProducts.Count == 0)
            {
                MessageBox.Show("Không có sản phẩm nào có tồn kho thấp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var lowStockWindow = new LowStockWindow(lowStockProducts);
            lowStockWindow.ShowDialog();
        }

        [RelayCommand]
        private void ShowTransactionHistory(int productId)
        {
            var historyWindow = new TransactionHistoryWindow(productId);
            historyWindow.ShowDialog();
        }
    }
}