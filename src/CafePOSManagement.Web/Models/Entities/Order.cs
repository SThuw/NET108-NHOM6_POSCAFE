using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CafePOSManagement.Web.Models.Entities;

public sealed record Order
{
    public long Id { get; init; }
    public Guid RequestId { get; init; }
    [Required, StringLength(20)]
    public string OrderNumber { get; init; } = "";
    [Required, StringLength(50), Display(Name = "Loại đơn hàng")]
    public string OrderType { get; init; } = "pickup";
    public int? DiscountId { get; init; }
    [JsonIgnore]
    public Discount? Discount { get; init; }
    [Range(0, 100), Display(Name = "Giảm giá (%)")]
    public decimal DiscountPercent { get; init; }
    [Range(0, 100), Display(Name = "Thuế VAT (%)")]
    public decimal TaxPercent { get; init; }
    [StringLength(500), Display(Name = "Ghi chú")]
    public string Note { get; init; } = "";
    [Required, StringLength(50), Display(Name = "Phương thức thanh toán")]
    public string PaymentMethod { get; init; } = "cash";
    [Required, StringLength(30), Display(Name = "Trạng thái")]
    public string Status { get; init; } = "Hoàn thành";
    public DateTimeOffset CreatedAt { get; init; }
    // List hỗ trợ EF thêm navigation khi materialize; Service cũ vẫn trả danh sách chỉ đọc.
    public IReadOnlyList<OrderItem> Items { get; init; } = new List<OrderItem>();
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal Total { get; init; }
}
