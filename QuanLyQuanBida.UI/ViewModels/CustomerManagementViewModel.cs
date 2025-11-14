using Castle.Core.Resource;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class CustomerManagementViewModel : ObservableObject
    {
        private readonly ICustomerService _customerService;

        [ObservableProperty]
        private ObservableCollection<Customer> _customers = new();

        [ObservableProperty]
        private CustomerDto _customerForm = new();

        [ObservableProperty]
        private Customer? _selectedCustomer;

        [ObservableProperty]
        private bool _isEditing = false;

        public CustomerManagementViewModel(ICustomerService customerService)
        {
            _customerService = customerService;
            _ = LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync()
        {
            Customers.Clear();
            var list = await _customerService.GetAllCustomersAsync();
            foreach (var c in list)
            {
                Customers.Add(c);
            }
        }

        [RelayCommand]
        private void AddNew()
        {
            SelectedCustomer = null;
            CustomerForm = new CustomerDto { Type = "Walk-in" }; // Set default type
            IsEditing = false;
        }

        [RelayCommand]
        private void EditCustomer(Customer customer)
        {
            if (customer == null) return;
            SelectedCustomer = customer;
            CustomerForm = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.Phone,
                Address = customer.Address,
                Type = customer.Type,
                VipCardNumber = customer.VipCardNumber,
            };
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveCustomer()
        {
            if (string.IsNullOrWhiteSpace(CustomerForm.Name) || string.IsNullOrWhiteSpace(CustomerForm.Phone))
            {
                MessageBox.Show("Vui lòng điền tên và số điện thoại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool success = false;
            try
            {
                if (IsEditing)
                {
                    success = await _customerService.UpdateCustomerAsync(CustomerForm);
                }
                else
                {
                    await _customerService.CreateCustomerAsync(CustomerForm);
                    success = true;
                }

                if (success)
                {
                    MessageBox.Show("Lưu thông tin khách hàng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadCustomersAsync();
                    AddNew();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu khách hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteCustomer(Customer customer)
        {
            if (customer == null || MessageBox.Show($"Bạn có chắc muốn xóa khách hàng '{customer.Name}'?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            if (await _customerService.DeleteCustomerAsync(customer.Id))
            {
                MessageBox.Show("Xóa khách hàng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadCustomersAsync();
                AddNew();
            }
        }
    }
}