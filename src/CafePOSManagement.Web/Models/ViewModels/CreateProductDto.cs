using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed class CreateProductDto : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng nhập tên món."), StringLength(100, ErrorMessage = "Tên món tối đa 100 ký tự."), Display(Name = "Tên món")]
    public string Name { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng chọn danh mục."), StringLength(40, ErrorMessage = "Mã danh mục tối đa 40 ký tự."), Display(Name = "Danh mục")]
    public string CategoryId { get; set; } = "";
    [Range(1, 10000000, ErrorMessage = "Giá bán phải từ 1 đến 10.000.000 đồng."), Display(Name = "Giá bán")]
    public decimal Price { get; set; }
    [Range(1, 10000000, ErrorMessage = "Giá gốc phải từ 1 đến 10.000.000 đồng."), Display(Name = "Giá gốc")]
    public decimal? OriginalPrice { get; set; }
    [StringLength(500, ErrorMessage = "Đường dẫn ảnh tối đa 500 ký tự."), Display(Name = "Đường dẫn ảnh")]
    public string? ImageUrl { get; set; }
    [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự."), Display(Name = "Mô tả")]
    public string? Description { get; set; }
    public bool IsAvailable { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Price != decimal.Truncate(Price) || (OriginalPrice.HasValue && OriginalPrice != decimal.Truncate(OriginalPrice.Value)))
            yield return new ValidationResult("Giá tiền phải là số nguyên đồng.", [nameof(Price), nameof(OriginalPrice)]);
        if (OriginalPrice.HasValue && OriginalPrice < Price)
            yield return new ValidationResult("Giá gốc không được nhỏ hơn giá bán.", [nameof(OriginalPrice)]);
        if (!string.IsNullOrWhiteSpace(ImageUrl) &&
            (!Uri.TryCreate(ImageUrl.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo)))
            yield return new ValidationResult("Ảnh phải dùng đường dẫn HTTPS hợp lệ hoặc để trống.", [nameof(ImageUrl)]);
    }
}
