using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ShotCall.API.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role)!;
}