using System.Threading.Tasks;
using CentralKitchen.Application.DTOs.Dashboard;

namespace CentralKitchen.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
}
