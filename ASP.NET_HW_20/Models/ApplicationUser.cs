using Microsoft.AspNetCore.Identity;

namespace ASP.NET_HW_20.Models;

public class ApplicationUser : IdentityUser {
    public int VisitCount { get; set; }
}