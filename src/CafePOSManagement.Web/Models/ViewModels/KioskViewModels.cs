using CafePOSManagement.Web.Models.Entities;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed class KioskHeroBanner
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string Tag { get; set; } = "";
    public string Badge { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public decimal? OriginalPrice { get; set; }
    public decimal Price { get; set; }
}

public sealed class KioskIndexViewModel
{
    public List<KioskHeroBanner> Banners { get; set; } = [];
    public List<Product> HotProducts { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
}

public sealed class KioskOrderViewModel
{
    public string OrderType { get; set; } = "dinein"; // dinein | pickup
    public string? Phone { get; set; }
    public string? MemberName { get; set; }
    public string MemberTier { get; set; } = "Standard";
    public int DiscountPercent { get; set; } = 0;
    public string DiscountCode { get; set; } = "";
    public List<Category> Categories { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}

public sealed class KioskItemDto
{
    public string ProductId { get; set; } = "";
    public int Quantity { get; set; } = 1;
}

public sealed class KioskCheckoutDto
{
    public string OrderType { get; set; } = "dinein";
    public string? Phone { get; set; }
    public string? Note { get; set; }
    public List<KioskItemDto> Items { get; set; } = [];
}

public sealed class KioskCheckoutResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public long OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public string? TicketUrl { get; set; }
}

public sealed class KioskTicketItemViewModel
{
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class KioskTicketViewModel
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public string OrderType { get; set; } = "dinein";
    public string? Phone { get; set; }
    public string MemberTier { get; set; } = "Thành viên";
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? Note { get; set; }
    public List<KioskTicketItemViewModel> Items { get; set; } = [];
}
