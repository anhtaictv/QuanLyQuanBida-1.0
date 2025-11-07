using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IRateService
    {
        Task<List<Rate>> GetAllRatesAsync();
        Task<Rate?> GetRateByIdAsync(int id);
        Task<Rate?> GetApplicableRateAsync(DateTime dateTime);
        Task<Rate> CreateRateAsync(Rate rate);
        Task<bool> UpdateRateAsync(Rate rate);
        Task<bool> DeleteRateAsync(int id);
    }
}
