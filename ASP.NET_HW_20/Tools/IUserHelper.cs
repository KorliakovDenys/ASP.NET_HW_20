using System.Security.Claims;

namespace ASP.NET_HW_20.Tools;

public interface IUserHelper {
    public Task IncreaseVisitCountAsync(ClaimsPrincipal user);
    public Task RefreshUserAsync(ClaimsPrincipal user);
}