using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CentralKitchen.Application.DTOs.Common;
using CentralKitchen.Application.DTOs.Dashboard;
using CentralKitchen.Application.Interfaces;

namespace CentralKitchen.API.Controllers;

/// <summary>
/// Managers reporting statistics.
/// </summary>
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Gets today's sales order count summary and low-stock warning indicators. Restricted to Managers.
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), 200)]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboardService.GetDashboardSummaryAsync();
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary));
    }
}
