using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CafePOSManagement.Web.Models.Entities;

/// <summary>
/// Thực thể Phiên làm việc AI (Cụm bảng AI State Store - Lời dặn giảng viên Lab 2)
/// </summary>
public sealed record AiSession(
    [property: Required, StringLength(40), Display(Name = "Mã phiên AI")] string Id,
    [property: Display(Name = "Mã nhân sự")] string? UserId,
    [property: Required, StringLength(200), Display(Name = "Tiêu đề phiên")] string Title)
{
    [StringLength(1000), Display(Name = "Ngữ cảnh / System Prompt")]
    public string? Context { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public User? User { get; init; }
    [JsonIgnore]
    public ICollection<AiMessage> Messages { get; init; } = new List<AiMessage>();
    [JsonIgnore]
    public ICollection<AiStateStore> States { get; init; } = new List<AiStateStore>();
}
