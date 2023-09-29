using System.Security.Claims;
using ASP.NET_HW_20.Models;
using Microsoft.AspNetCore.Identity;

namespace ASP.NET_HW_20.Tools;

public class UserHelper : IUserHelper {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserHelper(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager) {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }

    public async Task IncreaseVisitCountAsync(ClaimsPrincipal claimsPrincipal) {
        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user is not null) {
            user.VisitCount++;
            await _userManager.UpdateAsync(user);

            switch (user.VisitCount) {
                case 5:
                    await SetRoleAsync(user, "Experienced");
                    break;
                case 10:
                    await RemoveRoleAsync(user, "Experienced");
                    await SetRoleAsync(user, "OldTimer");
                    break;
                default:
                    break;
            }
        }
    }

    public async Task RefreshUserAsync(ClaimsPrincipal claimsPrincipal) {
        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (_signInManager.IsSignedIn(claimsPrincipal)) {
            await _signInManager.RefreshSignInAsync(user!);
        }
    }

    private async Task SetRoleAsync(ApplicationUser user, string role) {
        if (await _roleManager.RoleExistsAsync(role)) {
            await _userManager.AddToRoleAsync(user, role);
        }
    }

    private async Task RemoveRoleAsync(ApplicationUser user, string role) {
        if (await _roleManager.RoleExistsAsync(role)) {
            await _userManager.RemoveFromRoleAsync(user, role);
        }
    }
}