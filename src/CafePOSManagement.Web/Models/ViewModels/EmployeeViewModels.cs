using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed record EmployeeListItem(
    string Id,
    string FullName,
    string Email,
    bool IsManager,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed class CreateEmployeeViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Họ tên phải có từ 2 đến 60 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có từ 6 đến 100 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";

    [Display(Name = "Vai trò")]
    public bool IsManager { get; set; }
}

public sealed class EditEmployeeViewModel
{
    [Required]
    public string Id { get; set; } = "";

    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Họ tên phải có từ 2 đến 60 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Display(Name = "Vai trò")]
    public bool IsManager { get; set; }

    [Display(Name = "Trạng thái")]
    public bool IsActive { get; set; }
}

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
    public string Email { get; set; } = "";
}
