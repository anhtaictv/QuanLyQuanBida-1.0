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

                // --- Đăng ký Views và ViewModels ---
                services.AddSingleton<MainWindow>();
                services.AddTransient<LoginView>();
                services.AddTransient<LoginViewModel>();

                // -- Đăng ký Table---
                services.AddTransient<ITableService, TableService>();

                services.AddTransient<MainWindowViewModel>();

                services.AddTransient<ISessionService, SessionService>();

                services.AddTransient<IProductService, ProductService>();

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