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
using System; // SỬA: Thêm using System (cần cho Exception)
using MessageBox = System.Windows.MessageBox;

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

    [ObservableProperty]
    private User? _currentUser;

    // === THÊM MỚI: Thuộc tính phân quyền (từ các bước trước) ===
    public bool CanManageUsers => _currentUserService.HasPermission("ManageUsers");
    public bool CanManageProducts => _currentUserService.HasPermission("ManageProducts");
    public bool CanManageCustomers => _currentUserService.HasPermission("ManageCustomers");
    public bool CanManageInventory => _currentUserService.HasPermission("ManageInventory");
    public bool CanManageRates => _currentUserService.HasPermission("ManageRates");
    public bool CanViewReports => _currentUserService.HasPermission("ViewReports");
    public bool CanManageSettings => _currentUserService.HasPermission("ManageSettings");


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

    // === CÁC LỆNH MỞ CỬA SỔ (ĐÃ SỬA LẠI TOÀN BỘ) ===

    [RelayCommand]
    private void ShowCustomerManagement()
    {
        // Cách làm này (dùng App.Services) là ĐÚNG
        var customerWindow = App.Services.GetRequiredService<CustomerManagementView>();
        customerWindow.ShowDialog();
    }

    [RelayCommand]
    private void ShowRateSettings()
    {
        // Cách làm này (dùng App.Services) là ĐÚNG
        var rateSettingWindow = App.Services.GetRequiredService<RateSettingView>();
        rateSettingWindow.ShowDialog();
    }

    [RelayCommand]
    private void ShowSystemSettings()
    {
        // SỬA LỖI: Dùng App.Services
        var backupWindow = App.Services.GetRequiredService<BackupWindow>();
        backupWindow.ShowDialog();
    }

    [RelayCommand]
    private void SetLightTheme()
    {
        MessageBox.Show("Chuyển sang giao diện Sáng [TODO]");
    }

    [RelayCommand]
    private void SetDarkTheme()
    {
        MessageBox.Show("Chuyển sang giao diện Tối [TODO]");
    }

    [RelayCommand]
    private void ShowUserManagement()
    {
        // SỬA LỖI: Không dùng 'new'
        // new UserManagementView().ShowDialog(); 
        var view = App.Services.GetRequiredService<UserManagementView>();
        view.ShowDialog();
    }

    [RelayCommand]
    private void ShowProductManagement()
    {
        // SỬA LỖI: Không dùng 'new'
        // new ProductManagementView().ShowDialog();
        var view = App.Services.GetRequiredService<ProductManagementView>();
        view.ShowDialog();
    }

    [RelayCommand]
    private void ShowReports()
    {
        // SỬA LỖI: Không dùng 'new'
        // new ReportsView().ShowDialog();
        var view = App.Services.GetRequiredService<ReportsView>();
        view.ShowDialog();
    }

    [RelayCommand]
    private void ShowInventoryManagement()
    {
        var inventoryWindow = App.Services.GetRequiredService<InventoryManagementView>();
        inventoryWindow.ShowDialog();
    }

    [RelayCommand]
    private void ShowShiftManagement()
    {
        // SỬA LỖI: Không dùng 'new'
        // var shiftView = new ShiftManagementView();
        var shiftView = App.Services.GetRequiredService<ShiftManagementView>();
        shiftView.ShowDialog();
    }

    [RelayCommand]
    private void ShowBackupWindow()
    {
        // SỬA LỖI: Không dùng 'new'
        // var backupWindow = new BackupWindow();
        var backupWindow = App.Services.GetRequiredService<BackupWindow>();
        backupWindow.ShowDialog();
    }

    // === CÁC LỆNH NGHIỆP VỤ KHÁC ===

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

            // SỬA LỖI: Phải lấy PaymentWindow từ DI
            var paymentWindow = App.Services.GetRequiredService<PaymentWindow>();
            // Truyền invoice DTO vào ViewModel của cửa sổ thanh toán
            if (paymentWindow.DataContext is PaymentViewModel pvm)
            {
                pvm.Invoice = invoice;
            }
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
            // SỬA LỖI: Dùng service đã được inject (an toàn hơn)
            // if (App.Services.GetService(typeof(ICurrentUserService)) is ICurrentUserService currentUserService)
            //     currentUserService.CurrentUser = null;
            _currentUserService.CurrentUser = null;
            _currentUserService.Permissions.Clear();


            // Đóng cửa sổ hiện tại (MainWindow)
            // (Cách này an toàn hơn là FirstOrDefault)
            var currentWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            currentWindow?.Close();

            // Mở cửa sổ Login
            var loginView = App.Services.GetRequiredService<LoginView>();
            loginView.Show();
        }
    }

    [RelayCommand]
    private void Exit()
    {
        System.Windows.Application.Current.Shutdown();
    }

}