using CafePOSManagement.Web.Models.ViewModels;

namespace CafePOSManagement.Web.Services.Interfaces;

public interface IPosService
{
    PosIndexViewModel GetOverview();
}
