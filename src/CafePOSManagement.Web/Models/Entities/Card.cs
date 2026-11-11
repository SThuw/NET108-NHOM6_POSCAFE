using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.Entities;

/// <summary>
/// Thực thể Thẻ định danh / Thẻ bàn / Thẻ rung (Phân hệ Thẻ định danh)
/// Phụ trách: Trần Thị Tuyết Kha (TB02214)
/// </summary>
public sealed record Card(
    [property: Required, StringLength(40), Display(Name = "Mã thẻ")] string Id,
    [property: Required, StringLength(50), Display(Name = "Số thẻ / Mã hiển thị")] string Code,
    [property: Required, StringLength(100), Display(Name = "Tên thẻ / Bàn")] string Name,
    [property: Display(Name = "Trạng thái sử dụng (Trống / Đang phục vụ)")] bool IsOccupied = false,
    [property: Display(Name = "Hoạt động")] bool IsActive = true)
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
