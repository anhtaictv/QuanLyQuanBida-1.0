using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;

namespace QuanLyQuanBida.UI.Views
{
    public partial class MoveTableView : Window
    {
        public MoveTableView(MoveTableViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            viewModel.CloseAction = () => this.Close();
        }
    }
}