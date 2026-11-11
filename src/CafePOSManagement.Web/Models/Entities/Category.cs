using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CafePOSManagement.Web.Models.Entities;

public sealed record Category(
    [property: Required, StringLength(40), Display(Name = "Mã danh mục")] string Id,
    [property: Required, StringLength(100), Display(Name = "Tên danh mục")] string Name)
{
    [Required, StringLength(40), Display(Name = "Mã danh mục")]
    public string Code { get; init; } = Id;
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    [JsonIgnore]
    public ICollection<Product> Products { get; init; } = new List<Product>();
}
