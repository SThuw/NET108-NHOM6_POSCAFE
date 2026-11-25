using System.ComponentModel.DataAnnotations;
using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Controllers;

[AllowAnonymous]
public sealed class KioskController(CafePosDbContext db, IOrderService orders) : Controller
{
    private static readonly List<KioskHeroBanner> FeaturedDeals =
    [
        new()
        {
            Tag = "COMBO ƯU ĐÃI SÁNG",
            Badge = "TIẾT KIỆM 25%",
            Title = "Cà Phê Sữa Đá & Croissant Nướng",
            Subtitle = "Khởi đầu ngày mới tràn đầy năng lượng cùng bộ đôi cà phê truyền thống & bánh nướng bơ tỏi.",
            Price = 69000,
            OriginalPrice = 95000,
            ImageUrl = "/images/kiosk/banner1.png"
        },
        new()
        {
            Tag = "ĐỒ UỐNG MÙA LỄ HỘI",
            Badge = "THANH NHIỆT",
            Title = "Cold Brew Cam Sả & Trà Trái Cây",
            Subtitle = "Cà phê ủ lạnh 16 tiếng kết hợp tép cam mọng nước và sả thanh, sảng khoái từng ngụm.",
            Price = 45000,
            OriginalPrice = 55000,
            ImageUrl = "/images/kiosk/banner2.png"
        },
        new()
        {
            Tag = "THÀNH VIÊN POLYKAFÉ",
            Badge = "VIP MEMBER",
            Title = "Tích Điểm Nhận Ưu Đãi Lên Đến 15%",
            Subtitle = "Nhập số điện thoại tại quầy Kiosk để nhận ngay chiết khấu thành viên tự động khi thanh toán.",
            Price = 39000,
            OriginalPrice = 45000,
            ImageUrl = "/images/kiosk/banner3.png"
        }
    ];

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        var hotProducts = await db.Products.AsNoTracking()
            .Where(p => p.IsAvailable)
            .Take(6)
            .ToListAsync();

        return View(new KioskIndexViewModel
        {
            Banners = FeaturedDeals,
            Categories = categories,
            HotProducts = hotProducts
        });
    }

    [HttpGet]
    public IActionResult Order(string mode = "dinein", string? phone = null)
    {
        mode = mode is "pickup" ? "pickup" : "dinein";
        return RedirectToAction("Index", "Pos", new { kiosk = 1, mode, phone = phone?.Trim() });
    }

    [HttpGet]
    public IActionResult CheckMember(string? phone)
    {
        phone = phone?.Trim();
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 9)
        {
            return Json(new { success = false, message = "Số điện thoại không hợp lệ." });
        }

        var (tier, discount, code, name) = ResolveMembership(phone);
        return Json(new
        {
            success = true,
            phone,
            name,
            tier,
            discountPercent = discount,
            discountCode = code,
            message = discount > 0
                ? $"Chào mừng {name}! Bạn được áp dụng ưu đãi {tier} giảm {discount}%."
                : $"Chào mừng {name}! Tích điểm thành công cho đơn hàng này."
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult Checkout([FromBody] KioskCheckoutDto input)
    {
        if (input.Items == null || input.Items.Count == 0)
        {
            return BadRequest(new { success = false, message = "Giỏ hàng của bạn đang trống." });
        }

        var (tier, _, discountCode, _) = ResolveMembership(input.Phone);

        var noteText = string.IsNullOrWhiteSpace(input.Phone)
            ? (input.Note ?? "Đơn tự đặt tại Kiosk")
            : $"[Kiosk - SĐT: {input.Phone} - {tier}] {input.Note}".Trim();

        var checkoutDto = new CheckoutOrderDto
        {
            RequestId = Guid.NewGuid(),
            OrderType = input.OrderType is "pickup" ? "pickup" : "dinein",
            PaymentMethod = "cash",
            DiscountCode = string.IsNullOrEmpty(discountCode) ? null : discountCode,
            TaxPercent = 0,
            Note = noteText.Length > 200 ? noteText[..200] : noteText,
            Items = input.Items.Select(i => new CheckoutOrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = Math.Clamp(i.Quantity, 1, 99)
            }).ToList()
        };

        try
        {
            var createdOrder = orders.Checkout(checkoutDto);
            return Json(new KioskCheckoutResultDto
            {
                Success = true,
                OrderId = createdOrder.Id,
                OrderNumber = createdOrder.OrderNumber,
                TicketUrl = Url.Action(nameof(Ticket), new { id = createdOrder.Id })
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Không thể ghi nhận đơn Kiosk: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Ticket(long id)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var (tier, _, _, _) = ResolveMembership(order.Note);

        var vm = new KioskTicketViewModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CreatedAt = order.CreatedAt,
            OrderType = order.OrderType,
            Note = order.Note,
            MemberTier = tier,
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            DiscountPercent = order.DiscountPercent,
            TaxAmount = order.TaxAmount,
            Total = order.Total,
            Items = order.Items.Select(i => new KioskTicketItemViewModel
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal
            }).ToList()
        };

        return View(vm);
    }

    private static (string Tier, int Discount, string Code, string Name) ResolveMembership(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return ("Khách vãng lai", 0, "", "Quý khách");

        var clean = phone.Trim();
        if (clean.Length < 10)
            return ("Khách vãng lai", 0, "", "Quý khách");

        if (clean is "0901234567" or "0912345678" or "0987654321" or "0909090909" or "0988888888")
            return ("Hội viên VVIP", 15, "SALE15", "Thành viên VVIP");
        if (clean is "0911223344" or "0933445566" or "0977889900" or "0903456789")
            return ("Hội viên VIP", 10, "SALE10", "Thành viên VIP");

        return ("Thành viên mới", 5, "SALE5", "Khách hàng mới");
    }
}
