using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CafePOSManagement.Web.Models.Entities;

public sealed class Discount
{
    public int Id { get; set; }
    [Required, StringLength(50), Display(Name = "Mã giảm giá")]
    public string Code { get; set; } = "";
    [Required, StringLength(100), Display(Name = "Tên chương trình")]
    public string Name { get; set; } = "";
    [Range(0, 100), Display(Name = "Giảm giá (%)")]
    public decimal DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
    [JsonIgnore]
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
