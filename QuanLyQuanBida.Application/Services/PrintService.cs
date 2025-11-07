using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Interfaces;
using System.Drawing;
using System.Drawing.Printing;

namespace QuanLyQuanBida.Application.Services
{
    public class PrintService : IPrintService
    {
        public async Task<bool> PrintInvoiceAsync(InvoiceDto invoice)
        {
            return await Task.Run(() =>
            {
                try
                {
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += (sender, e) => PrintInvoicePage(e, invoice);
                    pd.Print();
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> PrintReportAsync<T>(List<T> data, string reportTitle)
        {
            return await Task.Run(() =>
            {
                try
                {
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += (sender, e) => PrintReportPage(e, data, reportTitle);
                    pd.Print();
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private void PrintInvoicePage(PrintPageEventArgs e, InvoiceDto invoice)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Arial", 10);
            Font boldFont = new Font("Arial", 10, FontStyle.Bold);
            Font titleFont = new Font("Arial", 16, FontStyle.Bold);
            Brush brush = Brushes.Black;
            float yPos = 20;
            float leftMargin = e.MarginBounds.Left;

            // Header
            g.DrawString("QUÁN BIDA XYZ", titleFont, brush, leftMargin + 150, yPos);
            yPos += 30;
            g.DrawString("Địa chỉ: 123 ABC, Q.1, TP.HCM", font, brush, leftMargin, yPos);
            yPos += 20;
            g.DrawString("Điện thoại: 0909123456", font, brush, leftMargin, yPos);
            yPos += 30;

            // Invoice Info
            g.DrawString($"HÓA ĐƠN: {invoice.InvoiceNumber}", boldFont, brush, leftMargin, yPos);
            yPos += 25;
            g.DrawString($"Bàn: {invoice.TableName}", font, brush, leftMargin, yPos);
            yPos += 20;
            g.DrawString($"Bắt đầu: {invoice.StartTime:HH:mm dd/MM/yyyy}", font, brush, leftMargin, yPos);
            yPos += 20;
            g.DrawString($"Kết thúc: {invoice.EndTime:HH:mm dd/MM/yyyy}", font, brush, leftMargin, yPos);
            yPos += 20;
            g.DrawString($"Thời gian: {invoice.DurationMinutes} phút", font, brush, leftMargin, yPos);
            yPos += 30;

            // Line separator
            g.DrawString("----------------------------------------", font, brush, leftMargin, yPos);
            yPos += 20;

            // Items
            g.DrawString("Tiền giờ:", font, brush, leftMargin, yPos);
            g.DrawString($"{invoice.TimeCharge:N0} VNĐ", font, brush, leftMargin + 200, yPos);
            yPos += 20;

            g.DrawString("Đồ uống/Thức ăn:", font, brush, leftMargin, yPos);
            g.DrawString($"{invoice.OrderTotal:N0} VNĐ", font, brush, leftMargin + 200, yPos);
            yPos += 20;

            // Line separator
            g.DrawString("----------------------------------------", font, brush, leftMargin, yPos);
            yPos += 20;

            // Totals
            g.DrawString("Tạm tính:", font, brush, leftMargin, yPos);
            g.DrawString($"{invoice.SubTotal:N0} VNĐ", font, brush, leftMargin + 200, yPos);
            yPos += 20;

            g.DrawString("Thuế (10%):", font, brush, leftMargin, yPos);
            g.DrawString($"{invoice.Tax:N0} VNĐ", font, brush, leftMargin + 200, yPos);
            yPos += 20;

            g.DrawString("Phí dịch vụ (5%):", font, brush, leftMargin, yPos);
            g.DrawString($"{invoice.ServiceFee:N0} VNĐ", font, brush, leftMargin + 200, yPos);
            yPos += 20;

            // Line separator
            g.DrawString("----------------------------------------", font, brush, leftMargin, yPos);
            yPos += 20;

            g.DrawString("TỔNG CỘNG:", boldFont, brush, leftMargin, yPos);
            g.DrawString($"{invoice.Total:N0} VNĐ", boldFont, brush, leftMargin + 200, yPos);
            yPos += 30;

            // Footer
            g.DrawString("Cảm ơn quý khách!", font, brush, leftMargin + 100, yPos);
            yPos += 20;
            g.DrawString("Hẹn gặp lại!", font, brush, leftMargin + 120, yPos);
        }

        private void PrintReportPage<T>(PrintPageEventArgs e, List<T> data, string reportTitle)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Arial", 8);
            Font boldFont = new Font("Arial", 8, FontStyle.Bold);
            Font titleFont = new Font("Arial", 12, FontStyle.Bold);
            Brush brush = Brushes.Black;
            float yPos = 20;
            float leftMargin = e.MarginBounds.Left;
            float columnWidth = (e.MarginBounds.Width - 20) / 4; // Assuming 4 columns

            // Title
            g.DrawString(reportTitle, titleFont, brush, leftMargin + 100, yPos);
            yPos += 30;

            // Header
            if (data.Count > 0)
            {
                var properties = data[0].GetType().GetProperties();
                int col = 0;
                foreach (var prop in properties.Take(4)) // Limit to 4 columns
                {
                    g.DrawString(prop.Name, boldFont, brush, leftMargin + col * columnWidth, yPos);
                    col++;
                }
                yPos += 20;

                // Line separator
                g.DrawString("----------------------------------------", font, brush, leftMargin, yPos);
                yPos += 15;

                // Data rows
                foreach (var item in data)
                {
                    col = 0;
                    foreach (var prop in properties.Take(4))
                    {
                        var value = prop.GetValue(item)?.ToString() ?? "";
                        g.DrawString(value, font, brush, leftMargin + col * columnWidth, yPos);
                        col++;
                    }
                    yPos += 15;

                    // Check if we need a new page
                    if (yPos > e.MarginBounds.Bottom - 50)
                    {
                        e.HasMorePages = true;
                        return;
                    }
                }
            }
        }
    }
}