using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuanLyQuanBida.Application.Services;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using QuanLyQuanBida.UI.ViewModels;
using QuanLyQuanBida.UI.Views;
using System.Windows;
using Serilog;

namespace QuanLyQuanBida.UI
{
    public partial class App : System.Windows.Application
    {
        private static Timer? _timer; // ✅ cho phép nullable

        public static string CurrentTime => DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");

        private static IHost? _host;

        public static IServiceProvider Services => _host!.Services;

        public static event EventHandler? CurrentTimeChanged;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Cấu hình Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs\\log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // ✅ Khởi tạo host trước
            _host = CreateHostBuilder(Array.Empty<string>()).Build();

            // ✅ Cập nhật thời gian mỗi giây
            _timer = new Timer(_ =>
            {
                CurrentTimeChanged?.Invoke(null, EventArgs.Empty);
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            // ✅ Lấy LoginView từ DI
            var loginView = _host.Services.GetRequiredService<LoginView>();
            loginView.Show();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog() // Thêm Serilog
        .ConfigureServices((context, services) =>
        {
            // ⚙️ Tự động chọn chuỗi kết nối
            var connectionString = BuildConnectionString();

            services.AddDbContext<QuanLyBidaDbContext>(options =>
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

            // --- Views & ViewModels ---
            services.AddSingleton<MainWindow>();
            services.AddTransient<LoginView>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainWindowViewModel>();
        });


        /// <summary>
        /// Tự động xây dựng chuỗi kết nối phù hợp với từng máy.
        /// </summary>
        private static string BuildConnectionString()
        {
            // 🔹 Danh sách các server phổ biến (tuỳ máy)
            var possibleServers = new[]
            {
            "MEOBEO",
            Environment.MachineName,                  // Ex: "LAPTOP-TTAI"
             $"{Environment.MachineName}\\SQLEXPRESS", // Ex: "LAPTOP-TTAI\\SQLEXPRESS"
            "(localdb)\\MSSQLLocalDB",               // LocalDB mặc định Visual Studio
            ".",                                     // SQL cài trực tiếp (localhost)
            "localhost"
    };

            string dbName = "QuanLyBidaDB";

            foreach (var server in possibleServers)
            {
                try
                {
                    // ✅ Thử kết nối bằng Windows Authentication
                    string trustedConn = $"Server={server};Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";
                    using (var conn = new Microsoft.Data.SqlClient.SqlConnection(trustedConn))
                    {
                        conn.Open();
                        Console.WriteLine($"✔ Connected using Windows Authentication: {server}");
                        return trustedConn;
                    }
                }
                catch (Exception ex) {
                
                    Console.WriteLine($"❌ Failed: {server} - {ex.Message}");
                }

                try
                {
                    // ✅ Thử kết nối bằng tài khoản sa
                    string saConn = $"Server={server};Database={dbName};User Id=sa;Password=123456;TrustServerCertificate=True;";
                    using (var conn = new Microsoft.Data.SqlClient.SqlConnection(saConn))
                    {
                        conn.Open();
                        Console.WriteLine($"✔ Connected using sa account: {server}");
                        return saConn;
                    }
                }
                catch
                {
                    // Ignore, tiếp tục
                }
            }

            // ❌ Nếu không kết nối được server nào
            MessageBox.Show("Không tìm thấy SQL Server phù hợp. Vui lòng nhập thủ công!", "Lỗi kết nối",
                MessageBoxButton.OK, MessageBoxImage.Error);

            var inputServer = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên server (VD: MEOBEO):", "Nhập server");
            return $"Server={inputServer};Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";

        }
    }
}
