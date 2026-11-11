using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed class CheckoutOrderDto : IValidatableObject
{
    public Guid RequestId { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn loại đơn."), RegularExpression("^(pickup|delivery|dinein)$", ErrorMessage = "Loại đơn không hợp lệ."), Display(Name = "Loại đơn")]
    public string OrderType { get; set; } = "pickup";
    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán."), RegularExpression("^(cash|card|wallet)$", ErrorMessage = "Phương thức thanh toán không hợp lệ."), Display(Name = "Thanh toán")]
    public string PaymentMethod { get; set; } = "cash";
    [StringLength(30, ErrorMessage = "Mã giảm giá tối đa 30 ký tự.")]
    public string? DiscountCode { get; set; }
    [Range(0, 100, ErrorMessage = "VAT phải từ 0 đến 100%."), Display(Name = "VAT (%)")]
    public decimal TaxPercent { get; set; }
    [StringLength(200, ErrorMessage = "Ghi chú tối đa 200 ký tự."), Display(Name = "Ghi chú")]
    public string? Note { get; set; }
    [Required(ErrorMessage = "Giỏ hàng không được để trống."), MinLength(1, ErrorMessage = "Giỏ hàng không được để trống."), MaxLength(100, ErrorMessage = "Mỗi đơn tối đa 100 món khác nhau.")]
    public List<CheckoutOrderItemDto> Items { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RequestId == Guid.Empty)
            yield return new ValidationResult("Thiếu mã yêu cầu thanh toán.", [nameof(RequestId)]);
        if (Items is not null && Items.Any(item => item is null))
            yield return new ValidationResult("Chi tiết món không hợp lệ.", [nameof(Items)]);
    }
}

public sealed class CheckoutOrderItemDto
{
    [Required(ErrorMessage = "Thiếu mã món."), StringLength(40, ErrorMessage = "Mã món tối đa 40 ký tự.")]
    public string ProductId { get; set; } = "";
    [Range(1, 99, ErrorMessage = "Số lượng mỗi món phải từ 1 đến 99.")]
    public int Quantity { get; set; }
}
