using System.Collections.Generic;
using System.Threading.Tasks;
using TableEntity = QuanLyQuanBida.Core.Entities.Table;

namespace QuanLyQuanBida.Core.Interfaces;

public interface ITableService
{
    Task<List<TableEntity>> GetAllTablesAsync();
}