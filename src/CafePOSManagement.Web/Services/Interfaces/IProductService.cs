using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;

namespace CafePOSManagement.Web.Services.Interfaces;

public interface IProductService
{
    IReadOnlyList<Category> GetCategories();
    IReadOnlyList<Product> GetProducts(string? categoryId = null, string? search = null);
    Product Create(CreateProductDto input);
}
