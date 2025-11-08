using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.Application.Services;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.UI.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyQuanBida.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ITableService _tableService;
    private readonly ISessionService _sessionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IBillingService _billingService;
    private readonly IRateService _rateService;

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

    // === THÊM MỚI: Thuộc tính CurrentUser để giao diện có thể bind vào ===
    // Lỗi 'CurrentUser' property not found sẽ được khắc phục bởi thuộc tính này.
    [ObservableProperty]
    private User? _currentUser;

    public MainWindowViewModel(
        ITableService tableService,
        ISessionService sessionService,
        ICurrentUserService currentUserService,
        IProductService productService,
        IOrderService orderService,
        IBillingService billingService,
        IRateService rateService)
    {
        _tableService = tableService;
        _sessionService = sessionService;
        _currentUserService = currentUserService;
        _productService = productService;
        _orderService = orderService;
        _billingService = billingService;
        _rateService = rateService;

        // === THÊM MỚI: Lấy thông tin user từ dịch vụ khi khởi tạo ===
        _currentUser = _currentUserService.CurrentUser;

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
        catch (Exception ex)
        {
            // Hiển thị lỗi ra Output hoặc MessageBox
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            MessageBox.Show($"Không thể tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Tables.Add(table);
    }

    private async Task LoadProductsAsync()
    {
        var productsFromDb = await _productService.GetAllProductsAsync();
        Products.Clear();
        foreach (var product in productsFromDb)
            Products.Add(product);
    }

    [RelayCommand]
    private void SelectTable(Table table)
    {
        SelectedTable = table;
        _ = LoadSessionForTableAsync(table.Id);
    }

    private async Task LoadSessionForTableAsync(int tableId)
    {
        var session = await _sessionService.GetActiveSessionByTableIdAsync(tableId);
        if (session != null)
        {
            CurrentSession = session;
            IsSessionActive = true;
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
        var orders = await _orderService.GetOrdersBySessionIdAsync(sessionId);
        CurrentSessionOrders.Clear();
        foreach (var order in orders)
            CurrentSessionOrders.Add(order);
    }

    [RelayCommand]
    private async Task StartSession()
    {
        if (SelectedTable == null || _currentUserService.CurrentUser == null) return;

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
            await LoadSessionForTableAsync(SelectedTable?.Id ?? 0);
    }

    [RelayCommand]
    private async Task ResumeSession()
    {
        if (CurrentSession == null) return;

        var success = await _sessionService.ResumeSessionAsync(
            CurrentSession.Id,
            _currentUserService.CurrentUser?.Id ?? 0);

        if (success)
            await LoadSessionForTableAsync(SelectedTable?.Id ?? 0);
    }

    // === THÊM MỚI: Các lệnh còn thiếu mà giao diện đang tìm kiếm ===
    // Các lỗi về '...Command' not found sẽ được khắc phục bởi các phương thức này.

    [RelayCommand]
    private void ShowCustomerManagement()
    {
        // TODO: Viết logic để mở cửa sổ Quản lý Khách hàng
        MessageBox.Show("Mở cửa sổ Quản lý Khách hàng");
    }

    [RelayCommand]
    private void ShowRateSettings()
    {
        // TODO: Viết logic để mở cửa sổ Cài đặt Giá giờ
        MessageBox.Show("Mở cửa sổ Cài đặt Giá giờ");
    }

    [RelayCommand]
    private void ShowSystemSettings()
    {
        // TODO: Viết logic để mở cửa sổ Cài đặt Hệ thống
        MessageBox.Show("Mở cửa sổ Cài đặt Hệ thống");
    }

    [RelayCommand]
    private void SetLightTheme()
    {
        // TODO: Viết logic để chuyển sang giao diện Sáng
        MessageBox.Show("Chuyển sang giao diện Sáng");
    }

    [RelayCommand]
    private void SetDarkTheme()
    {
        // TODO: Viết logic để chuyển sang giao diện Tối
        MessageBox.Show("Chuyển sang giao diện Tối");
    }

    // === CÁC LỆNH CÓ SẴN CỦA BẠN ===

    [RelayCommand]
    private void ShowUserManagement()
    {
        new UserManagementView().ShowDialog();
    }

    [RelayCommand]
    private void ShowProductManagement()
    {
        new ProductManagementView().ShowDialog();
    }

    [RelayCommand]
    private void ShowReports()
    {
        new ReportsView().ShowDialog();
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
            CurrentSessionOrders.Add(newOrder);
    }

    [RelayCommand]
    private async Task CloseSession()
    {
        if (CurrentSession == null) return;

        var closedSession = await _sessionService.CloseSessionAsync(
            CurrentSession.Id,
            _currentUserService.CurrentUser?.Id ?? 0);

        if (closedSession != null)
        {
            var invoice = await _billingService.GenerateInvoiceAsync(CurrentSession.Id);
            var paymentWindow = new PaymentWindow(invoice);
            paymentWindow.ShowDialog();

            await LoadTablesAsync();
            await LoadSessionForTableAsync(SelectedTable?.Id ?? 0);
        }
    }

    [RelayCommand]
    private void Logout()
    {
        if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            if (App.Services.GetService(typeof(ICurrentUserService)) is ICurrentUserService currentUserService)
                currentUserService.CurrentUser = null;

            System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault()?.Close();
            var loginView = App.Services.GetRequiredService<LoginView>();
            loginView.Show();
        }
    }

    [RelayCommand]
    private void ShowInventoryManagement()
    {
        var inventoryView = new InventoryManagementView();
        inventoryView.ShowDialog();
    }

    [RelayCommand]
    private void ShowShiftManagement()
    {
        var shiftView = new ShiftManagementView();
        shiftView.ShowDialog();
    }

    [RelayCommand]
    private void Exit()
    {
        System.Windows.Application.Current.Shutdown();
    }

    [RelayCommand]
    private void ShowBackupWindow()
    {
        var backupWindow = new BackupWindow();
        backupWindow.ShowDialog();
    }
}