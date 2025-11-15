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
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Windows.Data;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject, IRecipient<TablesChangedMessage>, IRecipient<ProductsChangedMessage>
    {
        private readonly ITableService _tableService;
        private readonly ISessionService _sessionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IBillingService _billingService;
        private readonly IRateService _rateService;
        private readonly IMessenger _messenger;
        private DispatcherTimer _timer;

        [ObservableProperty]
        private ObservableCollection<TableViewModel> _tables = new();

        [ObservableProperty]
        private ICollectionView? _tablesView;

        [ObservableProperty]
        private TableViewModel? _selectedTable;

        [ObservableProperty]
        private ObservableCollection<Product> _products = new();

        [ObservableProperty]
        private ICollectionView? _productsView;

        [ObservableProperty]
        private ObservableCollection<Order> _currentSessionOrders = new();

        [ObservableProperty]
        private Session? _currentSession;

        [ObservableProperty]
        private bool _isLoading = false;
        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _currentTime = App.CurrentTime;

        // Logic bật/tắt nút (PHẦN NÀY LÀ QUAN TRỌNG)
        public bool IsSessionActive => CurrentSession != null && CurrentSession.Status != "Finished";
        public bool CanStartSession => SelectedTable != null && !IsSessionActive;
        public bool CanPauseSession => IsSessionActive && CurrentSession?.Status == "Started";

        // === Phân quyền ===
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
            IRateService rateService,
            IMessenger messenger)
        {
            _tableService = tableService;
            _sessionService = sessionService;
            _currentUserService = currentUserService;
            _productService = productService;
            _orderService = orderService;
            _billingService = billingService;
            _rateService = rateService;
            _messenger = messenger;
            _messenger.RegisterAll(this);

            _currentUser = _currentUserService.CurrentUser;

            App.CurrentTimeChanged += (s, e) => CurrentTime = App.CurrentTime;

            SetupTimer();
            _ = LoadDataAsync();
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            foreach (var tableVM in Tables.Where(t => t.IsSessionActive))
            {
                tableVM.UpdateElapsedTime();
            }
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
            {
                var tableVM = new TableViewModel(table);
                var activeSession = await _sessionService.GetActiveSessionByTableIdAsync(table.Id);
                tableVM.CurrentSession = activeSession;
                Tables.Add(tableVM);
            }

            var collectionViewSource = new CollectionViewSource { Source = Tables };
            collectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Zone"));
            TablesView = collectionViewSource.View;
            OnPropertyChanged(nameof(TablesView));
        }

        private async Task LoadProductsAsync()
        {
            var productsFromDb = await _productService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in productsFromDb)
                Products.Add(product);

            var productsViewSource = new CollectionViewSource { Source = Products };
            productsViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            ProductsView = productsViewSource.View;
            OnPropertyChanged(nameof(ProductsView));
        }

        [RelayCommand]
        private async Task SelectTable(TableViewModel tableVM)
        {
            if (tableVM == null) return;
            SelectedTable = tableVM;
            CurrentSession = tableVM.CurrentSession;

            if (CurrentSession != null)
            {
                await LoadOrdersForSessionAsync(CurrentSession.Id);
            }
            else
            {
                CurrentSessionOrders.Clear();
            }

            // SỬA LỖI: Thông báo cho các nút cập nhật trạng thái
            StartSessionCommand.NotifyCanExecuteChanged();
            PauseSessionCommand.NotifyCanExecuteChanged();
            CloseSessionCommand.NotifyCanExecuteChanged();
            AddOrderCommand.NotifyCanExecuteChanged();
        }

        private async Task LoadSessionForTableAsync(int tableId)
        {
            var tableVM = Tables.FirstOrDefault(t => t.Table.Id == tableId);
            if (tableVM == null) return;

            var session = await _sessionService.GetActiveSessionByTableIdAsync(tableId);
            tableVM.CurrentSession = session;

            if (SelectedTable == tableVM)
            {
                CurrentSession = session;
                if (CurrentSession != null)
                {
                    await LoadOrdersForSessionAsync(CurrentSession.Id);
                }
                else
                {
                    CurrentSessionOrders.Clear();
                }
            }
        }

        private async Task LoadOrdersForSessionAsync(int sessionId)
        {
            var orders = await _orderService.GetOrdersBySessionIdAsync(sessionId);
            CurrentSessionOrders.Clear();
            foreach (var order in orders)
                CurrentSessionOrders.Add(order);
        }

        [RelayCommand(CanExecute = nameof(CanStartSession))]
        private async Task StartSession()
        {
            if (SelectedTable == null || _currentUserService.CurrentUser == null) return;

            var rate = await _rateService.GetApplicableRateAsync(DateTime.Now);
            if (rate == null)
            {
                MessageBox.Show("Không có giá giờ nào được cấu hình cho thời điểm này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newSession = await _sessionService.StartSessionAsync(
                SelectedTable.Table.Id,
                _currentUserService.CurrentUser.Id,
                rate.Id);

            if (newSession != null)
            {
                await LoadSessionForTableAsync(SelectedTable.Table.Id);

                // SỬA LỖI: Thông báo cho các nút cập nhật
                StartSessionCommand.NotifyCanExecuteChanged();
                PauseSessionCommand.NotifyCanExecuteChanged();
                CloseSessionCommand.NotifyCanExecuteChanged();
                AddOrderCommand.NotifyCanExecuteChanged();
            }
            else
            {
                MessageBox.Show("Không thể bắt đầu phiên chơi. Bàn có thể đã được sử dụng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanPauseSession))]
        private async Task PauseSession()
        {
            if (CurrentSession == null || SelectedTable == null) return;

            if (CurrentSession.Status == "Paused")
            {
                await _sessionService.ResumeSessionAsync(CurrentSession.Id, _currentUserService.CurrentUser?.Id ?? 0);
            }
            else
            {
                await _sessionService.PauseSessionAsync(CurrentSession.Id, _currentUserService.CurrentUser?.Id ?? 0);
            }
            await LoadSessionForTableAsync(SelectedTable.Table.Id);

            // SỬA LỖI: Thông báo cho nút Tạm dừng
            PauseSessionCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(IsSessionActive))]
        private async Task AddOrder(Product product)
        {
            if (product == null || SelectedTable == null || CurrentSession == null) return;

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
                await LoadOrdersForSessionAsync(CurrentSession.Id);
            }
        }

        [RelayCommand(CanExecute = nameof(IsSessionActive))]
        private async Task CloseSession()
        {
            if (CurrentSession == null || SelectedTable == null) return;

            var closedSession = await _sessionService.CloseSessionAsync(
                CurrentSession.Id,
                _currentUserService.CurrentUser?.Id ?? 0);

            if (closedSession != null)
            {
                var invoice = await _billingService.GenerateInvoiceAsync(CurrentSession.Id);

                var paymentWindow = App.Services.GetRequiredService<PaymentWindow>();

                var pvm = (PaymentViewModel)paymentWindow.DataContext;
                pvm.Invoice = invoice;

                paymentWindow.ShowDialog();

                await LoadSessionForTableAsync(SelectedTable.Table.Id);

                // SỬA LỖI: Thông báo cho các nút cập nhật
                StartSessionCommand.NotifyCanExecuteChanged();
                PauseSessionCommand.NotifyCanExecuteChanged();
                CloseSessionCommand.NotifyCanExecuteChanged();
                AddOrderCommand.NotifyCanExecuteChanged();
            }
        }

        // === CÁC LỆNH MỞ CỬA SỔ ===
        [RelayCommand] private void ShowCustomerManagement() => App.Services.GetRequiredService<CustomerManagementView>().ShowDialog();
        [RelayCommand] private void ShowTableManagement() => App.Services.GetRequiredService<TableManagementView>().ShowDialog();
        [RelayCommand] private void ShowRateSettings() => App.Services.GetRequiredService<RateSettingView>().ShowDialog();
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

        public async void Receive(TablesChangedMessage message)
        {
            await LoadTablesAsync();
        }

        public async void Receive(ProductsChangedMessage message)
        {
            await LoadProductsAsync();
        }
    }
}