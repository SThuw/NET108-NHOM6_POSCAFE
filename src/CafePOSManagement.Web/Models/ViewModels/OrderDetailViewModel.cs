using CafePOSManagement.Web.Models.Entities;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed record OrderDetailViewModel(Order Order, string? CreatedByEmployeeName)
{
    public string EmployeeName => !string.IsNullOrWhiteSpace(CreatedByEmployeeName)
        ? CreatedByEmployeeName
        : (Order.Note?.Contains("Kiosk", StringComparison.OrdinalIgnoreCase) == true
            ? "Kiosk tự phục vụ"
            : "Nguyễn Kim Song Thư");
}
