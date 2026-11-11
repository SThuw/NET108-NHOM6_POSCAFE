using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;

namespace CafePOSManagement.Web.Services.Implementations;

public sealed class ProductService : IProductService
{
    private static readonly object Gate = new();
    private static readonly Category[] Categories =
    [
        new("traditional", "Cà phê truyền thống"), new("snack", "Đồ ăn nhẹ"),
        new("sandwich", "Bánh mì & Sandwich"), new("coldbrew", "Cold Brew đặc biệt"),
        new("espresso", "Cà phê Ý (Espresso)"), new("seasonal", "Đồ uống theo mùa"), new("tea", "Trà trái cây")
    ];
    // shortcut: Dữ liệu chỉ sống trong tiến trình; thay bằng DbContext ở giai đoạn 5.
    private static readonly List<Product> Products =
    [
        new("p1", "Cà phê sữa đá", "traditional", 55000, 45000, "/images/menu/latte.jpg"),
        new("p2", "Cappuccino", "espresso", 65000, 55000, "/images/menu/coffee.jpg"),
        new("p3", "Bánh Croissant bơ", "snack", 50000, 45000, "/images/menu/croissant.jpg"),
        new("p4", "Americano nóng", "espresso", 45000, 40000, "/images/menu/espresso.jpg"),
        new("p5", "Bạc xỉu đá", "traditional", null, 39000, "/images/menu/iced.jpg"),
        new("p6", "Sandwich nướng", "sandwich", 75000, 65000, "/images/menu/sandwich.jpg"),
        new("p7", "Cold Brew sữa tươi", "coldbrew", 60000, 55000, "/images/menu/latte.jpg"),
        new("p8", "Trà chanh thanh mát", "tea", null, 45000, "/images/menu/fruit-tea.jpg"),
        new("p9", "Latte kem sữa", "espresso", 65000, 59000, "/images/menu/coffee.jpg"),
        new("p10", "Cà phê sữa mùa đông", "seasonal", null, 49000, "/images/menu/tea.jpg"),
        new("p11", "Cold Brew đá", "coldbrew", 55000, 49000, "/images/menu/iced.jpg"),
        new("p12", "Bánh quy bơ", "snack", 35000, 29000, "/images/menu/tea.jpg")
    ];

    public IReadOnlyList<Category> GetCategories() => Array.AsReadOnly(Categories);

    public IReadOnlyList<Product> GetProducts(string? categoryId = null, string? search = null)
    {
        var query = Normalize(search ?? "");
        lock (Gate)
            return Products.Where(p => (string.IsNullOrEmpty(categoryId) || p.CategoryId == categoryId) && Normalize(p.Name).Contains(query)).ToArray();
    }

    public Product Create(CreateProductDto input)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        if (!Categories.Any(category => category.Id == input.CategoryId))
            throw new ValidationException("Danh mục không tồn tại.");
        lock (Gate)
        {
            if (Products.Any(product => product.CategoryId == input.CategoryId && Normalize(product.Name) == Normalize(input.Name)))
                throw new ValidationException("Món này đã tồn tại trong danh mục.");
            var product = new Product($"p{Products.Count + 1}", input.Name.Trim(), input.CategoryId,
                input.OriginalPrice, input.Price, string.IsNullOrWhiteSpace(input.ImageUrl) ? "/images/product-fallback.svg" : input.ImageUrl.Trim(), input.IsAvailable);
            Products.Add(product);
            return product;
        }
    }

    private static string Normalize(string text)
    {
        var decomposed = text.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        return string.Concat(decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
    }
}
