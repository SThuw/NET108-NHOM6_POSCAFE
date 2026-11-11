using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.Entities;

public sealed record DiscountCode(
    [property: Required, StringLength(30)] string Code,
    [property: Required, StringLength(100)] string Name,
    [property: Range(0, 100), Display(Name = "Giảm giá (%)")] decimal Percent);
