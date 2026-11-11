using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CafePOSManagement.Web.Models.Entities;

// Giữ tên và giá tại thời điểm chốt đơn, không phụ thuộc giá thực đơn về sau.
public sealed record OrderItem(
    string ProductId,
    string ProductName,
    decimal UnitPrice,
    [property: Range(1, 99, ErrorMessage = "Số lượng mỗi món phải từ 1 đến 99.")] int Quantity)
{
    public long Id { get; init; }
    public long OrderId { get; init; }
    [JsonIgnore]
    public Order? Order { get; init; }
    [JsonIgnore]
    public Product? Product { get; init; }
    // SQL lưu computed column; constructor vẫn tính đúng cho Service in-memory hiện tại.
    public decimal LineTotal { get; private init; } = UnitPrice * Quantity;
}
