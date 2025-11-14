using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Data;
using System.Windows;
using System.Windows.Input;
using QuanLyQuanBida.Core.DTOs;
using System; 
using System.Threading.Tasks; 
using System.Collections.Generic; 
using System.Linq;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class UserManagementViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRoleService _roleService;
        private readonly IAuditService _auditService;
        private readonly IUserService _userService; 

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

        public UserManagementViewModel(
            IAuthService authService,
            ICurrentUserService currentUserService,
            IRoleService roleService,
            IAuditService auditService,
            IUserService userService) 
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _roleService = roleService;
            _auditService = auditService;
            _userService = userService; 
            _ = LoadDataAsync();
        }
        private async Task LoadDataAsync()
        {
            await LoadUsersAsync();
            await LoadRolesAsync();
        }
        private async Task LoadRolesAsync()
        {
            Roles.Clear();
            var rolesFromDb = await _roleService.GetAllRolesAsync();
            foreach (var role in rolesFromDb)
            {
                Roles.Add(role);
            }
        }

        private async Task LoadUsersAsync()
        {
            Users.Clear();
            var usersFromDb = await _userService.GetAllUsersAsync();

            var filteredList = string.IsNullOrWhiteSpace(SearchText)
                ? usersFromDb
                : usersFromDb.Where(u => u.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                         (u.FullName != null && u.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

            foreach (var user in filteredList)
            {
                Users.Add(user);
            }
        }

        [RelayCommand]
        private async Task Search()
        {
            await LoadUsersAsync();
        }
        [RelayCommand]
        private void AddNew()
        {
            SelectedUser = null;
            UserForm = new UserDto() { RoleId = 4, IsActive = true };
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
                FullName = user.FullName ?? string.Empty,
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
                (string.IsNullOrWhiteSpace(UserForm.Password) && !IsEditing))
            {
                MessageBox.Show("Vui lòng điền đầy đủ Tên đăng nhập (và Mật khẩu cho người dùng mới).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool success = false;
            string action = "UPDATE_USER";
            int? targetId = UserForm.Id;
            string oldValue = null;

            try
            {
                if (IsEditing)
                {

                    var existing = await _userService.GetUserByIdAsync(UserForm.Id);
                    oldValue = $"Username: {existing.Username}, Role: {existing.RoleId}";

                    success = await _userService.UpdateUserAsync(UserForm);
                    if (!success)
                    {
                        MessageBox.Show("Cập nhật thất bại. Tên đăng nhập có thể đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    action = "CREATE_USER";
                    success = await _authService.CreateUserAsync(
                        UserForm.Username,
                        UserForm.Password,
                        UserForm.FullName,
                        UserForm.RoleId);
                    if (!success)
                    {
                        MessageBox.Show("Tên đăng nhập đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    targetId = null;
                }

                if (success)
                {
                    await _auditService.LogActionAsync(
                        _currentUserService.CurrentUser?.Id ?? 0,
                        action,
                        "Users",
                        targetId,
                        oldValue,
                        newValue: $"Username: {UserForm.Username}, RoleId: {UserForm.RoleId}");

                    MessageBox.Show("Lưu thông tin người dùng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadUsersAsync();
                    AddNew();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            AddNew();
        }
        [RelayCommand]
        private async Task DeleteUser(User user)
        {
            if (user == null) return;
            if (user.Username == "admin")
            {
                MessageBox.Show("Không thể xóa tài khoản Administrator.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa người dùng '{user.Username}'?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = await _userService.DeleteUserAsync(user.Id);
                    if (success)
                    {
                        await _auditService.LogActionAsync(
                            _currentUserService.CurrentUser?.Id ?? 0,
                            "DELETE_USER",
                            "Users",
                            user.Id,
                            oldValue: $"Username: {user.Username}");

                        await LoadUsersAsync();
                        MessageBox.Show("Xóa người dùng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Xóa người dùng thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}