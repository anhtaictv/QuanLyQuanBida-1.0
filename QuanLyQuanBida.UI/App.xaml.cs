using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuanLyQuanBida.Application.Services;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using QuanLyQuanBida.UI.ViewModels;
using QuanLyQuanBida.UI.Views;
using System.Windows;

namespace QuanLyQuanBida.UI;

public partial class App : System.Windows.Application
{
    private static Timer _timer;

    public static string CurrentTime => DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Update time every second
        _timer = new Timer(_ =>
        {
            // Notify UI that time has changed
            CurrentTimeChanged?.Invoke(null, EventArgs.Empty);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        _host = CreateHostBuilder(Array.Empty<string>()).Build();
        var loginView = _host.Services.GetRequiredService<LoginView>();
        loginView.Show();
    }

    public static event EventHandler? CurrentTimeChanged;

    private static IHost? _host;
    public static IServiceProvider Services => _host!.Services;
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // --- Đăng ký DbContext ---
                services.AddDbContext<QuanLyBidaDbContext>(options =>
                    options.UseSqlServer("Server=MEOBEO;Database=QuanLyBidaDB;Trusted_Connection=True;TrustServerCertificate=True;"));

                // --- Đăng ký Services ---
                services.AddTransient<IAuthService, AuthService>();
                services.AddSingleton<ICurrentUserService, CurrentUserService>();
                services.AddTransient<ITableService, TableService>();
                services.AddTransient<ISessionService, SessionService>();
                services.AddTransient<IProductService, ProductService>();
                services.AddTransient<IOrderService, OrderService>();
                services.AddTransient<IBillingService, BillingService>();

                // === ĐĂNG KÝ CÁC SERVICE MỚI ===
                services.AddTransient<ISettingService, SettingService>();
                services.AddTransient<IRateService, RateService>();
                services.AddTransient<ICustomerService, CustomerService>();
                services.AddTransient<IReportService, ReportService>();
                services.AddTransient<IPrintService, PrintService>();
                services.AddSingleton<IThemeService, ThemeService>();
                // ==============================

                // --- Đăng ký Views và ViewModels ---
                services.AddSingleton<MainWindow>();
                services.AddTransient<LoginView>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<MainWindowViewModel>();

                // === ĐĂNG KÝ CÁC VIEW/VIEWMODEL MỚI ===
                services.AddTransient<PaymentWindow>();
                services.AddTransient<PaymentViewModel>();
                services.AddTransient<UserManagementView>();
                services.AddTransient<UserManagementViewModel>();
                services.AddTransient<ProductManagementView>();
                services.AddTransient<ProductManagementViewModel>();
                services.AddTransient<ReportsView>();
                services.AddTransient<ReportsViewModel>();

            });

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _host = CreateHostBuilder(Array.Empty<string>()).Build();

        // Mở cửa sổ Login đầu tiên
        var loginView = _host.Services.GetRequiredService<LoginView>();
        loginView.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }
}