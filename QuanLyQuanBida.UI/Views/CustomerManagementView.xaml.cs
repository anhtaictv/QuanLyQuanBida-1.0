using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;

namespace QuanLyQuanBida.UI.Views
{
    public partial class CustomerManagementView : Window
    {
        public CustomerManagementView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<CustomerManagementViewModel>();
        }
    }
}