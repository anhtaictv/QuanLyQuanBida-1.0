using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLyQuanBida.Core.DTOs; // SỬA: Thêm DTO
using TableEntity = QuanLyQuanBida.Core.Entities.Table;
namespace QuanLyQuanBida.Core.Interfaces;
public interface ITableService
{
    Task<List<TableEntity>> GetAllTablesAsync();

    Task<TableEntity?> GetTableByIdAsync(int id);
    Task<TableEntity> CreateTableAsync(TableDto tableDto);
    Task<bool> UpdateTableAsync(TableDto tableDto);
    Task<bool> DeleteTableAsync(int id);
    Task<bool> CreateTablesAsync(List<TableDto> tables);
}