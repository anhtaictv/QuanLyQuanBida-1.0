🚀 Phần mềm Quản lý Quán Bida (WPF)
Dự án này là một ứng dụng Desktop (WPF) được xây dựng trên nền tảng .NET, sử dụng kiến trúc phân lớp (Layered Architecture) và mô hình MVVM (Model-View-ViewModel) để quản lý toàn bộ hoạt động của một quán bida.

🌟 Tính năng Chính
Phần mềm bao gồm đầy đủ các nghiệp vụ cốt lõi và nghiệp vụ quản lý :

1. Nghiệp vụ Cốt lõi (POS)
Sơ đồ bàn Trực quan: Hiển thị trạng thái các bàn (Trống, Đang chơi, Bảo trì) theo thời gian thực và phân nhóm theo Khu vực (Tầng 1, Tầng 2, VIP...).

Quản lý Phiên chơi (Session):

Mở phiên (Bắt đầu tính giờ) khi khách vào .

Tạm dừng / Tiếp tục phiên.

Đếm thời gian chơi (ElapsedTime) hiển thị trực tiếp trên thẻ bàn.


Gọi món (POS): Thêm sản phẩm (đồ uống, thức ăn) vào phiên chơi, tự động phân nhóm sản phẩm theo danh mục .


Gán Khách hàng: Cho phép gán khách hàng (Vãng lai, VIP) vào phiên chơi để áp dụng chiết khấu .


Thanh toán & Hóa đơn: Tự động tính tiền giờ + tiền đồ uống và khởi tạo hóa đơn thanh toán .

2. Nghiệp vụ Quản lý (Admin)
Quản lý Bàn: Thêm, Sửa, Xóa các bàn. Hỗ trợ tạo bàn hàng loạt (ví dụ: tạo 10 bàn cho "Tầng 2" chỉ bằng một cú click) .

Quản lý Sản phẩm: Quản lý danh sách món ăn, đồ uống. Hỗ trợ thêm nhanh Danh mục và Đơn vị tính mới (bằng nút +).


Quản lý Khách hàng: Quản lý thông tin khách hàng thân thiết và khách VIP.


Quản lý Người dùng & Phân quyền: Tạo tài khoản nhân viên (Owner, Manager, Staff) và giới hạn quyền truy cập các tính năng .


Quản lý Tồn kho: Chức năng "Nhập hàng" và "Điều chỉnh Tồn kho" (dành cho Manager) .


Báo cáo: Xem các báo cáo doanh thu (đang trong quá trình phát triển) .

3. Tính năng Hệ thống
Đăng nhập & Xác thực: Sử dụng BCrypt.Net để băm và xác thực mật khẩu.

Tự động cập nhật (Real-time): Sử dụng IMessenger (MVVM Toolkit) để tự động làm mới Sơ đồ bàn và Danh sách POS khi có thay đổi từ các cửa sổ Quản lý.


Ghi nhật ký (Audit Log): Tự động ghi lại các hành động quan trọng (Tạo bàn, Xóa sản phẩm, Mở phiên) vào CSDL .


Sao lưu & Phục hồi: Chức năng cho phép "Owner" sao lưu (backup) và phục hồi (restore) cơ sở dữ liệu SQL Server.

🛠️ Công nghệ sử dụng
.NET 8

WPF (Windows Presentation Foundation): Cho giao diện người dùng Desktop.

MVVM (Model-View-ViewModel): Sử dụng thư viện CommunityToolkit.Mvvm (cho [ObservableProperty], [RelayCommand], IMessenger).

Entity Framework Core (EF Core): Làm việc với CSDL (ORM).

SQL Server: Cơ sở dữ liệu chính (SQL Server Express / LocalDB).

Dependency Injection (DI): Sử dụng Microsoft.Extensions.DependencyInjection (cấu hình trong App.xaml.cs).

Logging: Serilog (ghi lỗi ra file text).

🏗️ Cấu trúc Dự án
Dự án được xây dựng theo kiến trúc phân lớp (Layered Architecture) để đảm bảo tính độc lập và dễ bảo trì .


QuanLyQuanBida.Core: Chứa các đối tượng thuần túy (Entities, DTOs) và các Hợp đồng (Interfaces). Không phụ thuộc vào bất cứ project nào khác.

QuanLyQuanBida.Infrastructure: Chịu trách nhiệm về kỹ thuật. Chứa DbContext, các Migration (cấu trúc CSDL), và triển khai các interface liên quan đến CSDL.

QuanLyQuanBida.Application: Chứa logic nghiệp vụ (Business Logic). Đây là nơi các Service (ví dụ: BillingService, SessionService) được triển khai.

QuanLyQuanBida.UI: Là project WPF (giao diện người dùng). Chứa các cửa sổ (Views) và các ViewModel (logic giao diện).

QuanLyQuanBida.Tests: Chứa các Unit Test (sử dụng xUnit và Moq).

⚙️ Cài đặt & Khởi chạy
Yêu cầu
Visual Studio 2022: (với workload ".NET desktop development").


SQL Server: (SQL Server Express hoặc LocalDB được cài sẵn cùng Visual Studio là đủ).

Hướng dẫn Khởi chạy
Clone mã nguồn:

Bash

git clone [repository-url]
Cấu hình Kết nối CSDL (Connection String): Phần mềm được thiết kế để tự động tìm CSDL của bạn.

Mở tệp QuanLyQuanBida.UI/App.xaml.cs.

Tìm phương thức BuildConnectionString() .

Nó sẽ tự động thử các server name phổ biến (MEOBEO, TenMayCuaBan\SQLEXPRESS, (localdb)\MSSQLLocalDB, .) .

Nếu muốn chỉ định rõ, bạn có thể sửa trực tiếp tại đây.

Tạo Cơ sở dữ liệu (Migration):

Mở Package Manager Console trong Visual Studio.

Chọn QuanLyQuanBida.Infrastructure làm "Default project".

Chạy lệnh sau để áp dụng cấu trúc CSDL và seed tài khoản admin:

PowerShell

Update-Database
Chạy ứng dụng:

Chọn QuanLyQuanBida.UI làm project khởi động (Startup Project).

Nhấn F5 (hoặc nút Start) để chạy.

Tài khoản Đăng nhập Mặc định
Tài khoản này được tạo tự động khi bạn chạy Update-Database.

Username: admin

Password: admin
