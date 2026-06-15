using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace CentralKitchen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var subClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (subClaim != null && Guid.TryParse(subClaim.Value, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }

    protected string CurrentUserRole
    {
        get
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value.ToLower() ?? "";
        }
    }

    protected int? CurrentUserStoreId
    {
        get
        {
            var storeIdClaim = User.FindFirst("store_id");
            if (storeIdClaim != null && int.TryParse(storeIdClaim.Value, out var storeId))
            {
                return storeId;
            }
            return null;
        }
    }
}
