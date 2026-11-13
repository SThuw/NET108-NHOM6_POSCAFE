using CafePOSManagement.Web.Models.Entities;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed class ProductManagementViewModel
{
    public IReadOnlyList<Product> Products { get; init; } = [];
    public IReadOnlyList<Category> Categories { get; init; } = [];
    public string Search { get; init; } = "";
    public string CategoryId { get; init; } = "";
    public string Status { get; init; } = "all";
    public int Page { get; init; } = 1;
    public int PageCount { get; init; } = 1;
    public int Total { get; init; }
    public int Active { get; init; }
    public int Stopped { get; init; }
    public int ResultCount { get; init; }
}
