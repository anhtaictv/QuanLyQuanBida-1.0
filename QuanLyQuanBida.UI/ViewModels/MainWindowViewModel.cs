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
using System;
using System.Windows.Threading; // SỬA: Thêm
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // ... (Các service đã có) ...
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
        [NotifyPropertyChangedFor(nameof(CanStartSession))] // SỬA: Thêm
        [NotifyPropertyChangedFor(nameof(CanPauseSession))] // SỬA: Thêm
        private Table? _selectedTable;

        [ObservableProperty]
        private ObservableCollection<Product> _products = new();
        [ObservableProperty]
        private ObservableCollection<Order> _currentSessionOrders = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSessionActive))] // SỬA: Thêm
        [NotifyPropertyChangedFor(nameof(CanStartSession))] // SỬA: Thêm
        [NotifyPropertyChangedFor(nameof(CanPauseSession))] // SỬA: Thêm
        private Session? _currentSession;

        [ObservableProperty]
        private bool _isLoading = false;
        [ObservableProperty]
        private User? _currentUser;

        // SỬA: Thêm CurrentTime cho StatusBar
        [ObservableProperty]
        private string _currentTime = App.CurrentTime;

        // SỬA: Thêm các thuộc tính logic
        public bool IsSessionActive => CurrentSession != null && CurrentSession.Status != "Finished";
        public bool CanStartSession => SelectedTable != null && !IsSessionActive;
        public bool CanPauseSession => IsSessionActive && CurrentSession?.Status == "Started";

        // === Thuộc tính phân quyền (từ các bước trước) ===
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

            // SỬA: Đăng ký sự kiện đồng hồ
            App.CurrentTimeChanged += (s, e) => CurrentTime = App.CurrentTime;

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
        private async Task SelectTable(Table table) // SỬA: Thêm async
        {
            if (table == null) return;
            SelectedTable = table;
            await LoadSessionForTableAsync(table.Id);
        }

        private async Task LoadSessionForTableAsync(int tableId)
        {
            var session = await _sessionService.GetActiveSessionByTableIdAsync(tableId);
            if (session != null)
            {
                CurrentSession = session;
                await LoadOrdersForSessionAsync(session.Id);
            }
            else
            {
                CurrentSession = null;
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

        [RelayCommand(CanExecute = nameof(CanStartSession))] // SỬA: Thêm CanExecute
        private async Task StartSession()
        {
            if (SelectedTable == null || _currentUserService.CurrentUser == null) return;

            // Lấy giá giờ
            var rate = await _rateService.GetApplicableRateAsync(DateTime.Now);
            if (rate == null)
            {
                MessageBox.Show("Không có giá giờ nào được cấu hình cho thời điểm này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newSession = await _sessionService.StartSessionAsync(
                SelectedTable.Id,
                _currentUserService.CurrentUser.Id,
                rate.Id);

            if (newSession != null)
            {
                await LoadTablesAsync(); // Tải lại trạng thái bàn
                // Chọn lại bàn vừa mở
                SelectTable(Tables.FirstOrDefault(t => t.Id == SelectedTable.Id)!);
            }
            else
            {
                MessageBox.Show("Không thể bắt đầu phiên chơi. Bàn có thể đã được sử dụng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanPauseSession))] // SỬA: Thêm CanExecute
        private async Task PauseSession()
        {
            if (CurrentSession == null) return;

            // SỬA: Thêm logic Resume
            if (CurrentSession.Status == "Paused")
            {
                await _sessionService.ResumeSessionAsync(CurrentSession.Id, _currentUserService.CurrentUser?.Id ?? 0);
            }
            else
            {
                await _sessionService.PauseSessionAsync(CurrentSession.Id, _currentUserService.CurrentUser?.Id ?? 0);
            }
            await LoadSessionForTableAsync(SelectedTable!.Id);
        }

        [RelayCommand(CanExecute = nameof(IsSessionActive))] // SỬA: Thêm CanExecute
        private async Task AddOrder(Product product)
        {
            if (product == null || SelectedTable == null || CurrentSession == null) return;

            var newOrder = await _orderService.CreateOrderAsync(new OrderDto
            {
                SessionId = CurrentSession.Id,
                ProductId = product.Id,
                Quantity = 1,
                Price = product.Price, // Giá được lấy từ Product
                Note = ""
            });

            if (newOrder != null)
            {
                // Tải lại danh sách order
                await LoadOrdersForSessionAsync(CurrentSession.Id);
            }
        }

        [RelayCommand(CanExecute = nameof(IsSessionActive))] // SỬA: Thêm CanExecute
        private async Task CloseSession()
        {
            if (CurrentSession == null) return;

            var closedSession = await _sessionService.CloseSessionAsync(
                CurrentSession.Id,
                _currentUserService.CurrentUser?.Id ?? 0);

            if (closedSession != null)
            {
                var invoice = await _billingService.GenerateInvoiceAsync(CurrentSession.Id);

                // Mở cửa sổ thanh toán
                var paymentWindow = App.Services.GetRequiredService<PaymentWindow>();

                // Gán ViewModel và Invoice
                var pvm = (PaymentViewModel)paymentWindow.DataContext;
                pvm.Invoice = invoice;

                paymentWindow.ShowDialog();

                // Tải lại
                await LoadTablesAsync();
                await LoadSessionForTableAsync(SelectedTable?.Id ?? 0);
            }
        }

        // === CÁC LỆNH MỞ CỬA SỔ (Giữ nguyên) ===
        [RelayCommand] private void ShowCustomerManagement() => App.Services.GetRequiredService<CustomerManagementView>().ShowDialog();
        [RelayCommand] private void ShowRateSettings() => App.Services.GetRequiredService<RateSettingView>().ShowDialog();
        [RelayCommand] private void ShowSystemSettings() => App.Services.GetRequiredService<BackupWindow>().ShowDialog();
        [RelayCommand] private void SetLightTheme() => MessageBox.Show("Chuyển sang giao diện Sáng [TODO]");
        [RelayCommand] private void SetDarkTheme() => MessageBox.Show("Chuyển sang giao diện Tối [TODO]");
        [RelayCommand] private void ShowUserManagement() => App.Services.GetRequiredService<UserManagementView>().ShowDialog();
        [RelayCommand] private void ShowProductManagement() => App.Services.GetRequiredService<ProductManagementView>().ShowDialog();
        [RelayCommand] private void ShowReports() => App.Services.GetRequiredService<ReportsView>().ShowDialog();
        [RelayCommand] private void ShowInventoryManagement() => App.Services.GetRequiredService<InventoryManagementView>().ShowDialog();
        [RelayCommand] private void ShowShiftManagement() => App.Services.GetRequiredService<ShiftManagementView>().ShowDialog();
        [RelayCommand] private void ShowBackupWindow() => App.Services.GetRequiredService<BackupWindow>().ShowDialog();

        [RelayCommand]
        private void Logout()
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _currentUserService.CurrentUser = null;
                _currentUserService.Permissions.Clear();

                var currentWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                currentWindow?.Close();

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
}