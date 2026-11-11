using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CafePOSManagement.Web.Models.Entities;

/// <summary>
/// Thực thể Tin nhắn / Lịch sử Prompt AI (Cụm bảng AI State Store)
/// </summary>
public sealed record AiMessage
{
    public long Id { get; init; }
    [Required, StringLength(40), Display(Name = "Mã phiên")]
    public string SessionId { get; init; } = "";
    [Required, StringLength(50), Display(Name = "Vai trò (user / assistant / system)")]
    public string Role { get; init; } = "user";
    [Required, Display(Name = "Nội dung")]
    public string Content { get; init; } = "";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public AiSession? Session { get; init; }
}
