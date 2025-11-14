using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuanLyQuanBida.Application.Services;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using QuanLyQuanBida.UI.ViewModels;
using QuanLyQuanBida.UI.Views;
using Serilog;
using System;
using QuanLyQuanBida.Application.Services;
using System.Threading;
using System.Windows;
using Timer = System.Threading.Timer;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI
{
    public partial class App : System.Windows.Application
    {
        private static System.Threading.Timer? _timer;
        public static string CurrentTime => DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
        private static IHost? _host;
        public static IServiceProvider Services => _host!.Services;
        public static event EventHandler? CurrentTimeChanged;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Cấu hình Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                _host = CreateHostBuilder(Array.Empty<string>()).Build();

                using (var scope = _host.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<QuanLyBidaDbContext>();
                    await dbContext.Database.MigrateAsync();
                }

                _timer = new Timer(_ =>
                {
                    CurrentTimeChanged?.Invoke(null, EventArgs.Empty);
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

                var loginView = _host.Services.GetRequiredService<LoginView>();
                loginView.Show();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start.");
                MessageBox.Show($"Lỗi nghiêm trọng khi khởi động: {ex.Message}", "Lỗi Khởi động", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                var connectionString = BuildConnectionString();

                services.AddDbContextFactory<QuanLyBidaDbContext>(options =>
                    options.UseSqlServer(connectionString));

                // --- Services ---
                services.AddTransient<IAuthService, AuthService>();
                services.AddSingleton<ICurrentUserService, CurrentUserService>();
                services.AddTransient<ITableService, TableService>();
                services.AddTransient<ISessionService, SessionService>();
                services.AddTransient<IProductService, ProductService>();
                services.AddTransient<IOrderService, OrderService>();
                services.AddTransient<IBillingService, BillingService>();
                services.AddTransient<ISettingService, SettingService>();
                services.AddTransient<IRateService, RateService>();
                services.AddTransient<ICustomerService, CustomerService>();
                services.AddTransient<IReportService, ReportService>();
                services.AddTransient<IPrintService, PrintService>();
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddTransient<IInventoryService, InventoryService>();
                services.AddTransient<IShiftService, ShiftService>();
                services.AddTransient<IAuditService, AuditService>();
                services.AddSingleton<IBackupService>(new BackupService(connectionString));
                services.AddTransient<IRoleService, RoleService>();
                services.AddTransient<IPermissionService, PermissionService>();
                services.AddTransient<IUserService, UserService>();


                // --- Views & ViewModels ---
                services.AddTransient<MainWindow>();
                services.AddTransient<MainWindowViewModel>();

                services.AddTransient<LoginView>();
                services.AddTransient<LoginViewModel>();

                services.AddTransient<PaymentWindow>();
                services.AddTransient<PaymentViewModel>();

                services.AddTransient<BackupWindow>();
                services.AddTransient<BackupViewModel>();

                services.AddTransient<AddStockWindow>();
                services.AddTransient<AddStockViewModel>();

                services.AddTransient<CustomerManagementView>();
                services.AddTransient<CustomerManagementViewModel>();

                services.AddTransient<RateSettingView>();
                services.AddTransient<RateSettingViewModel>();

                services.AddTransient<ShiftManagementView>();
                services.AddTransient<ShiftManagementViewModel>();

                services.AddTransient<InventoryManagementView>();
                services.AddTransient<InventoryManagementViewModel>();

                services.AddTransient<ProductManagementView>();
                services.AddTransient<ProductManagementViewModel>();

                services.AddTransient<ReportsView>();
                services.AddTransient<ReportsViewModel>();

                services.AddTransient<UserManagementView>();
                services.AddTransient<UserManagementViewModel>();

            });
        private static string BuildConnectionString()
        {
            var possibleServers = new[]
            {
                "MEOBEO",
                Environment.MachineName,
                $"{Environment.MachineName}\\SQLEXPRESS",
                "(localdb)\\MSSQLLocalDB",
                ".",
                "localhost"
            };
            string dbName = "QuanLyBidaDB";
            foreach (var server in possibleServers)
            {
                try
                {
                    string trustedConn = $"Server={server};Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5";
                    using (var conn = new Microsoft.Data.SqlClient.SqlConnection(trustedConn))
                    {
                        conn.Open();
                        Log.Information($"✔ Connected using Windows Authentication: {server}");
                        return trustedConn;
                    }
                }
                catch (Exception)
                {
                    Log.Warning($"❌ Failed Windows Auth: {server}");
                }
                try
                {
                    string saConn = $"Server={server};Database={dbName};User Id=sa;Password=123456;TrustServerCertificate=True;Connect Timeout=5";
                    using (var conn = new Microsoft.Data.SqlClient.SqlConnection(saConn))
                    {
                        conn.Open();
                        Log.Information($"✔ Connected using sa account: {server}");
                        return saConn;
                    }
                }
                catch (Exception)
                {
                    Log.Warning($"❌ Failed SA Auth: {server}");
                }
            }

            MessageBox.Show("Không tìm thấy SQL Server phù hợp. Vui lòng nhập thủ công!", "Lỗi kết nối",
            MessageBoxButton.OK, MessageBoxImage.Error);
            var inputServer = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên server (VD: MEOBEO):", "Nhập server");
            if (string.IsNullOrWhiteSpace(inputServer))
            {
                System.Windows.Application.Current.Shutdown();
            }
            return $"Server={inputServer};Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";
        }
    }
}