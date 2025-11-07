using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class UserManagementViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        [ObservableProperty]
        private ObservableCollection<Role> _roles = new();

        [ObservableProperty]
        private User? _selectedUser;

        [ObservableProperty]
        private UserDto _userForm = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isEditing = false;

        public UserManagementViewModel(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadUsersAsync();
            await LoadRolesAsync();
        }

        private async Task LoadUsersAsync()
        {
            Users.Clear();

            // Get all users from service
            // For now, we'll use a placeholder
            var usersFromDb = await Task.FromResult(new List<User>());

            foreach (var user in usersFromDb)
            {
                Users.Add(user);
            }
        }

        private async Task LoadRolesAsync()
        {
            Roles.Clear();

            // Get all roles from service
            // For now, we'll use a placeholder
            var rolesFromDb = await Task.FromResult(new List<Role>());

            foreach (var role in rolesFromDb)
            {
                Roles.Add(role);
            }
        }

        [RelayCommand]
        private async Task Search()
        {
            // Implement search logic
            await LoadUsersAsync();
        }

        [RelayCommand]
        private void AddNew()
        {
            SelectedUser = null;
            UserForm = new UserDto();
            IsEditing = false;
        }

        [RelayCommand]
        private void EditUser(User user)
        {
            if (user == null) return;

            SelectedUser = user;
            UserForm = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Phone = user.Phone,
                RoleId = user.RoleId,
                IsActive = user.IsActive
            };
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveUser()
        {
            if (string.IsNullOrWhiteSpace(UserForm.Username) ||
                string.IsNullOrWhiteSpace(UserForm.Password) && !IsEditing)
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin bắt buộc.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (IsEditing)
                {
                    // Update existing user
                    // Implement update logic
                }
                else
                {
                    // Create new user
                    var success = await _authService.CreateUserAsync(
                        UserForm.Username,
                        UserForm.Password,
                        UserForm.FullName,
                        UserForm.RoleId);

                    if (!success)
                    {
                        MessageBox.Show("Tên đăng nhập đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                MessageBox.Show("Lưu thông tin người dùng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadUsersAsync();
                AddNew(); // Reset form
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public UserManagementViewModel() { }

        [RelayCommand]
        private void Cancel()
        {
            AddNew(); // Reset form
        }

        [RelayCommand]
        private async Task DeleteUser(User user)
        {
            if (user == null) return;

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa người dùng '{user.Username}'?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    // Implement delete logic
                    await LoadUsersAsync();
                    MessageBox.Show("Xóa người dùng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}