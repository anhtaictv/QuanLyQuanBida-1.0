using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;

namespace QuanLyQuanBida.UI.Views
{
    public partial class RateSettingView : Window
    {
        public RateSettingView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<RateSettingViewModel>();
        }
    }
}