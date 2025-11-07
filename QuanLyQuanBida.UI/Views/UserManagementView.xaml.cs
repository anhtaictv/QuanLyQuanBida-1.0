using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using System.Windows;

namespace QuanLyQuanBida.UI.Views;
public partial class UserManagementView : Window
{
    private readonly IAuthService _authService;
    private readonly QuanLyBidaDbContext _context;

    public UserManagementView(IAuthService authService, QuanLyBidaDbContext context)
    {
        InitializeComponent();
        _authService = authService;
        _context = context;
        LoadUsers();
        LoadRoles();
    }

    private async void LoadUsers()
    {
        var users = await _context.Users.Include(u => u.Role).ToListAsync();
        UsersDataGrid.ItemsSource = users;
    }

    private async void LoadRoles()
    {
        var roles = await _context.Roles.ToListAsync();
        RoleComboBox.ItemsSource = roles;
        if (roles.Any())
        {
            RoleComboBox.SelectedIndex = 0; // Chọn role đầu tiên mặc định
        }
    }

    private async void CreateUserButton_Click(object sender, RoutedEventArgs e)
    {
        string username = NewUsernameTextBox.Text;
        string password = NewPasswordBox.Password;
        string fullName = NewFullNameTextBox.Text;

        if (RoleComboBox.SelectedValue is int roleId)
        {
            bool success = await _authService.CreateUserAsync(username, password, fullName, roleId);

            if (success)
            {
                MessageBox.Show("Tạo người dùng thành công!");
                // Xóa form
                NewUsernameTextBox.Clear();
                NewPasswordBox.Clear();
                NewFullNameTextBox.Clear();
                // Tải lại danh sách
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại hoặc có lỗi xảy ra.");
            }
        }
        else
        {
            MessageBox.Show("Vui lòng chọn vai trò.");
        }
    }
}