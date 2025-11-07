using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace QuanLyQuanBida.UI.ViewModels;
public partial class MainWindowViewModel : ObservableObject
{
    private readonly ITableService _tableService;
    private readonly ISessionService _sessionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProductService _productService;

    [ObservableProperty]
    private ObservableCollection<Table> _tables = new();

    [ObservableProperty]
    private Table? _selectedTable;

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<Order> _currentSessionOrders = new();

    // Cập nhật constructor
    public MainWindowViewModel(ITableService tableService, ISessionService sessionService, ICurrentUserService currentUserService, IProductService productService)
    {
        _tableService = tableService;
        _sessionService = sessionService;
        _currentUserService = currentUserService;
        _productService = productService;
        _ = LoadDataAsync(); // Tải cả bàn và sản phẩm
    }

    private async Task LoadDataAsync()
    {
        await LoadTablesAsync();
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        var productsFromDb = await _productService.GetAllProductsAsync();
        if (productsFromDb != null)
        {
            foreach (var product in productsFromDb)
            {
                Products.Add(product);
            }
        }
    }

    [RelayCommand]
    private void SelectTable(Table table)
    {
        SelectedTable = table;
    }

    [RelayCommand]
    private async Task StartSession()
    {
        if (SelectedTable == null) return;
        if (_currentUserService.CurrentUser == null) return;

        var newSession = await _sessionService.StartSessionAsync(SelectedTable.Id, _currentUserService.CurrentUser.Id);

        if (newSession != null)
        {
            Tables.Clear();
            await LoadTablesAsync();
        }
        else
        {
            System.Windows.MessageBox.Show("Không thể bắt đầu phiên chơi. Bàn có thể đã được sử dụng.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
    [RelayCommand]
    private void AddOrder(Product product)
    {
        if (SelectedTable == null || SelectedTable.Status != "Occupied") return;

        // Tạo một order mới (chưa lưu vào DB)
        var newOrder = new Order
        {
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            Price = product.Price,
            CreatedAt = DateTime.UtcNow
        };

        CurrentSessionOrders.Add(newOrder);
    }
}