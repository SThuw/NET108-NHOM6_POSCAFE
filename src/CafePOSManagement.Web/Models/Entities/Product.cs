using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CafePOSManagement.Web.Models.Entities;

public sealed record Product(
    string Id,
    [property: Required, StringLength(150), Display(Name = "Tên món")] string Name,
    [property: Required, Display(Name = "Danh mục")] string CategoryId,
    [property: Range(1, 10000000), Display(Name = "Giá gốc")] decimal? OriginalPrice,
    [property: Range(1, 10000000), Display(Name = "Giá bán")] decimal Price,
    [property: StringLength(500), Display(Name = "Hình ảnh")] string ImageUrl,
    [property: Display(Name = "Đang bán")] bool IsAvailable = true)
{
    [StringLength(1000), Display(Name = "Mô tả")]
    public string? Description { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    [JsonIgnore]
    public Category? Category { get; init; }
    [JsonIgnore]
    public ICollection<OrderItem> OrderItems { get; init; } = new List<OrderItem>();
}
