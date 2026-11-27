using CafePOSManagement.Web.Models.Entities;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed record OrderDetailViewModel(Order Order, string? CreatedByEmployeeName)
{
    public string EmployeeName => string.IsNullOrWhiteSpace(CreatedByEmployeeName)
        ? "Không ghi nhận"
        : CreatedByEmployeeName;
}
