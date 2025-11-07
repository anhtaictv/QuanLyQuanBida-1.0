using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace QuanLyQuanBida.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Khi click vào một bàn, gọi command của ViewModel
        if (sender is System.Windows.Controls.Border border && border.DataContext is Core.Entities.Table table)
        {
            if (this.DataContext is ViewModels.MainWindowViewModel viewModel)
            {
                viewModel.SelectTableCommand.Execute(table);
            }
        }
    }
}