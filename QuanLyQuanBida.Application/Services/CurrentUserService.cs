using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;

namespace QuanLyQuanBida.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    public User? CurrentUser { get; set; }
}