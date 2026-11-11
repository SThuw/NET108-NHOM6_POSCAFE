using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CafePOSManagement.Web.Models.Entities;

/// <summary>
/// Thực thể Lưu trữ trạng thái bộ nhớ AI (Cụm bảng AI State Store)
/// </summary>
public sealed record AiStateStore
{
    public long Id { get; init; }
    [Required, StringLength(40), Display(Name = "Mã phiên")]
    public string SessionId { get; init; } = "";
    [Required, StringLength(100), Display(Name = "Khóa trạng thái (Key)")]
    public string StateKey { get; init; } = "";
    [Required, Display(Name = "Giá trị trạng thái (Value / JSON)")]
    public string StateValue { get; init; } = "";
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public AiSession? Session { get; init; }
}
