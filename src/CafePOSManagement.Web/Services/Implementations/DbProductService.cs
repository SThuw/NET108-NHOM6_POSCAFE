using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Services.Implementations;

/// <summary>
/// Triển khai IProductService dùng EF Core / SQL Server (Giai đoạn 4+).
/// Mỗi request nhận một DbContext Scoped từ DI; không dùng static list.
/// </summary>
public sealed class DbProductService(CafePosDbContext db) : IProductService
{
    public IReadOnlyList<Category> GetCategories()
        => db.Categories
             .Where(c => c.IsActive)
             .OrderBy(c => c.DisplayOrder)
             .ThenBy(c => c.Name)
             .AsNoTracking()
             .ToArray();

    public IReadOnlyList<Product> GetProducts(string? categoryId = null, string? search = null)
    {
        var query = db.Products
                      .Where(p => p.IsAvailable)
                      .AsNoTracking()
                      .AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoryId))
            query = query.Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Tìm không dấu: chạy trên client sau khi lọc danh mục trên server.
            // Với dataset nhỏ (< vài nghìn món) vẫn hiệu quả.
            var normalized = Normalize(search);
            return query.ToArray()
                        .Where(p => Normalize(p.Name).Contains(normalized))
                        .ToArray();
        }

        return query
               .OrderBy(p => p.CategoryId)
               .ThenBy(p => p.Name)
               .ToArray();
    }

    public Product Create(CreateProductDto input)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);

        var categoryExists = db.Categories.Any(c => c.Id == input.CategoryId && c.IsActive);
        if (!categoryExists)
            throw new ValidationException("Danh mục không tồn tại.");

        // Kiểm tra trùng tên trong cùng danh mục (Latin1_General_100_CI_AI đã xử lý ở DB).
        // Kiểm tra thêm phía app để hiển thị thông báo lỗi tiếng Việt rõ ràng.
        var normalizedName = Normalize(input.Name);
        var nameExists = db.Products
            .Where(p => p.CategoryId == input.CategoryId)
            .AsNoTracking()
            .ToArray()
            .Any(p => Normalize(p.Name) == normalizedName);
        if (nameExists)
            throw new ValidationException("Món này đã tồn tại trong danh mục.");

        var newId = $"p{Guid.NewGuid():N}";

        var product = new Product(
            newId,
            input.Name.Trim(),
            input.CategoryId,
            input.OriginalPrice,
            input.Price,
            string.IsNullOrWhiteSpace(input.ImageUrl)
                ? "/images/product-fallback.svg"
                : input.ImageUrl.Trim(),
            input.IsAvailable)
        {
            Description = input.Description?.Trim()
        };

        db.Products.Add(product);
        db.SaveChanges();
        return product;
    }

    private static string Normalize(string text)
    {
        var decomposed = text.Trim().ToLowerInvariant()
                             .Replace('đ', 'd')
                             .Normalize(NormalizationForm.FormD);
        return string.Concat(decomposed.Where(
            c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
    }
}
