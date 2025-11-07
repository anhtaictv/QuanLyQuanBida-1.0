using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.Application.Services;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.UI.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace QuanLyQuanBida.UI.ViewModels;
public partial class MainWindowViewModel : ObservableObject
{
    private readonly ITableService _tableService;
    private readonly ISessionService _sessionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;

    [ObservableProperty]
    private ObservableCollection<Table> _tables = new();

    [ObservableProperty]
    private Table? _selectedTable;

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<Order> _currentSessionOrders = new();

    [ObservableProperty]
    private Session? _currentSession;

    [ObservableProperty]
    private bool _isSessionActive = false;

    [ObservableProperty]
    private bool _isLoading = false;

    // Cập nhật constructor
    public MainWindowViewModel(
        ITableService tableService,
        ISessionService sessionService,
        ICurrentUserService currentUserService,
        IProductService productService,
        IOrderService orderService,
        IBillingService billingService)
    {
        _tableService = tableService;
        _sessionService = sessionService;
        _currentUserService = currentUserService;
        _productService = productService;
        _orderService = orderService;
        _billingService = billingService;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            await LoadTablesAsync();
            await LoadProductsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTablesAsync()
    {
        var tablesFromDb = await _tableService.GetAllTablesAsync();
        Tables.Clear();

        foreach (var table in tablesFromDb)
        {
            Tables.Add(table);
        }
    }

    private async Task LoadProductsAsync()
    {
        var productsFromDb = await _productService.GetAllProductsAsync();
        Products.Clear();

        foreach (var product in productsFromDb)
        {
            Products.Add(product);
        }
    }

    [RelayCommand]
    private void SelectTable(Table table)
    {
        SelectedTable = table;

        // Load session for this table if exists
        _ = LoadSessionForTableAsync(table.Id);
    }

    private async Task LoadSessionForTableAsync(int tableId)
    {
        // We need to implement GetActiveSessionByTableId in SessionService
        var session = await _sessionService.GetActiveSessionByTableIdAsync(tableId);

        if (session != null)
        {
            CurrentSession = session;
            IsSessionActive = true;

            // Load orders for this session
            await LoadOrdersForSessionAsync(session.Id);
        }
        else
        {
            CurrentSession = null;
            IsSessionActive = false;
            CurrentSessionOrders.Clear();
        }
    }

    private async Task LoadOrdersForSessionAsync(int sessionId)
    {
        // We need to implement GetOrdersBySessionId in OrderService
        var orders = await _orderService.GetOrdersBySessionIdAsync(sessionId);

        CurrentSessionOrders.Clear();
        foreach (var order in orders)
        {
            CurrentSessionOrders.Add(order);
        }
    }

    [RelayCommand]
    private async Task StartSession()
    {
        if (SelectedTable == null) return;
        if (_currentUserService.CurrentUser == null) return;

        // Get default rate (we might need a rate selection UI)
        var rates = await _rateService.GetAllRatesAsync();
        var defaultRate = rates.FirstOrDefault(r => r.IsDefault) ?? rates.FirstOrDefault();

        if (defaultRate == null)
        {
            MessageBox.Show("Không có giá giờ nào được cấu hình.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var newSession = await _sessionService.StartSessionAsync(
            SelectedTable.Id,
            _currentUserService.CurrentUser.Id,
            defaultRate.Id);

        if (newSession != null)
        {
            await LoadTablesAsync();
            await LoadSessionForTableAsync(SelectedTable.Id);
        }
        else
        {
            MessageBox.Show("Không thể bắt đầu phiên chơi. Bàn có thể đã được sử dụng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task PauseSession()
    {
        if (CurrentSession == null) return;

        var success = await _sessionService.PauseSessionAsync(
            CurrentSession.Id,
            _currentUserService.CurrentUser?.Id ?? 0);

        if (success)
        {
            await LoadSessionForTableAsync(SelectedTable?.Id ?? 0);
        }
    }

    [RelayCommand]
    private async Task ResumeSession()
    {
        if (CurrentSession == null) return;

        var success = await _sessionService.ResumeSessionAsync(
            CurrentSession.Id,
            _currentUserService.CurrentUser?.Id ?? 0);

        if (success)
        {
            await LoadSessionForTableAsync(SelectedTable?.Id ?? 0);
        }
    }

    [RelayCommand]
    private async Task AddOrder(Product product)
    {
        if (SelectedTable == null || CurrentSession == null) return;

        var newOrder = await _orderService.CreateOrderAsync(new OrderDto
        {
            SessionId = CurrentSession.Id,
            ProductId = product.Id,
            Quantity = 1,
            Price = product.Price,
            Note = ""
        });

        if (newOrder != null)
        {
            CurrentSessionOrders.Add(newOrder);
        }
    }

    [RelayCommand]
    private async Task CloseSession()
    {
        if (CurrentSession == null) return;

        // Close the session
        var closedSession = await _sessionService.CloseSessionAsync(
            CurrentSession.Id,
            _currentUserService.CurrentUser?.Id ?? 0);

        if (closedSession != null)
        {
            // Generate invoice
            var invoice = await _billingService.GenerateInvoiceAsync(CurrentSession.Id);

            // Show payment window
            var paymentWindow = new PaymentWindow(invoice);
            paymentWindow.ShowDialog();

            // Refresh tables
            await LoadTablesAsync();
            await LoadSessionForTableAsync(SelectedTable?.Id ?? 0);
        }
    }
    [RelayCommand]
    private void ShowUserManagement()
    {
        var userManagementWindow = new UserManagementView();
        userManagementWindow.ShowDialog();
    }

    [RelayCommand]
    private void ShowProductManagement()
    {
        var productManagementWindow = new ProductManagementView();
        productManagementWindow.ShowDialog();
    }

    [RelayCommand]
    private void ShowReports()
    {
        var reportsWindow = new ReportsView();
        reportsWindow.ShowDialog();
    }

    [RelayCommand]
    private void ShowRateSettings()
    {
        // Create and show rate settings window
        MessageBox.Show("Tính năng cài đặt giá giờ sẽ được triển khai sau.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ShowSystemSettings()
    {
        // Create and show system settings window
        MessageBox.Show("Tính năng cài đặt hệ thống sẽ được triển khai sau.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void Logout()
    {
        if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            // Clear current user
            if (App.Services.GetService(typeof(ICurrentUserService)) is ICurrentUserService currentUserService)
            {
                currentUserService.CurrentUser = null;
            }

            // Close main window and show login
            Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
            var loginView = App.Services.GetRequiredService<LoginView>();
            loginView.Show();
        }
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }
}