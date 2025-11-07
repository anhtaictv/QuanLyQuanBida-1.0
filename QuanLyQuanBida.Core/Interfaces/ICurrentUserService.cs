using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces;

public interface ICurrentUserService
{
    User? CurrentUser { get; set; }
}