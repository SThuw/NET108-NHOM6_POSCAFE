using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.Entities;

/// <summary>
/// Thực thể Người dùng / Nhân viên (Phân hệ Nhân sự & Tài khoản)
/// Phụ trách: Nguyễn Lâm Chí Tâm (TB02234)
/// </summary>
public sealed record User(
    [property: Required, StringLength(40), Display(Name = "Mã nhân viên")] string Id,
    [property: Required, EmailAddress, StringLength(100), Display(Name = "Email")] string Email,
    [property: Required, StringLength(255), Display(Name = "Mật khẩu")] string Password,
    [property: Required, StringLength(60), Display(Name = "Họ và tên")] string FullName,
    [property: Required, Phone, StringLength(15), Display(Name = "Số điện thoại")] string Phone,
    [property: Display(Name = "Vai trò (0: Nhân viên, 1: Quản lý)")] bool Role,
    [property: Display(Name = "Trạng thái hoạt động")] bool IsActive = true)
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
