using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed class CategoryForm
{
    [Required, RegularExpression("^[a-z0-9-]{1,40}$", ErrorMessage = "Mã chỉ gồm chữ thường, số và dấu gạch ngang (tối đa 40 ký tự).")]
    public string Id { get; set; } = "";
    [Required, StringLength(100)]
    public string Name { get; set; } = "";
    [Required, RegularExpression("^[a-z0-9-]{1,40}$", ErrorMessage = "Mã hiển thị chỉ gồm chữ thường, số và dấu gạch ngang.")]
    public string Code { get; set; } = "";
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }
}
