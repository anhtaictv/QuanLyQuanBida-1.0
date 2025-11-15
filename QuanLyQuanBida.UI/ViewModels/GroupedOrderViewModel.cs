using CommunityToolkit.Mvvm.ComponentModel;
using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class GroupedOrderViewModel : ObservableObject
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }

        [ObservableProperty]
        private int _quantity;

        // Lưu danh sách các Order ID gốc
        public List<int> OrderIds { get; set; } = new List<int>();

        public GroupedOrderViewModel(Order order)
        {
            ProductId = order.ProductId;
            ProductName = order.Product.Name;
            Price = order.Price;
            Quantity = order.Quantity;
            OrderIds.Add(order.Id);
        }

        public void AddOrder(Order order)
        {
            Quantity += order.Quantity;
            OrderIds.Add(order.Id);
        }
    }
}