using System.Security.Claims;
using ASP.NET_HW_20.Data;
using ASP.NET_HW_20.Models;
using Microsoft.AspNetCore.Identity;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace ASP.NET_HW_20.Stores;

public class ApplicationUserStore : ApplicationStore, IUserEmailStore<ApplicationUser>,
    IUserPasswordStore<ApplicationUser>, IUserRoleStore<ApplicationUser>, IUserLoginStore<ApplicationUser>,
    IUserLockoutStore<ApplicationUser>, IUserAuthenticationTokenStore<ApplicationUser>,
    IUserSecurityStampStore<ApplicationUser>, IUserTwoFactorStore<ApplicationUser>, IUserClaimStore<ApplicationUser> {
    private const string CreateQuery = """

                                       INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, VisitCount)
                                       VALUES (@UserId, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed, @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled, @LockoutEnd, @LockoutEnabled, @AccessFailedCount, 0);

                                       """;

    private const string UpdateQuery = """

                                       UPDATE AspNetUsers
                                       SET UserName = @UserName,
                                           NormalizedUserName = @NormalizedUserName,
                                           Email = @Email,
                                           NormalizedEmail = @NormalizedEmail,
                                           EmailConfirmed = @EmailConfirmed,
                                           PasswordHash = @PasswordHash,
                                           SecurityStamp = @SecurityStamp,
                                           ConcurrencyStamp = @ConcurrencyStamp,
                                           PhoneNumber = @PhoneNumber,
                                           PhoneNumberConfirmed = @PhoneNumberConfirmed,
                                           TwoFactorEnabled = @TwoFactorEnabled,
                                           LockoutEnd = @LockoutEnd,
                                           LockoutEnabled = @LockoutEnabled,
                                           AccessFailedCount = @AccessFailedCount,
                                           VisitCount = @VisitCount
                                       WHERE Id = @UserId;

                                       """;

    private const string DeleteQuery = """

                                       DELETE FROM AspNetUsers
                                       WHERE Id = @UserId;

                                       """;

    private const string FindByIdQuery = """

                                         SELECT *
                                         FROM AspNetUsers
                                         WHERE Id = @UserId;

                                         """;

    private const string FindByNameQuery = """

                                           SELECT *
                                           FROM AspNetUsers
                                           WHERE NormalizedUserName = @NormalizedUserName;

                                           """;

    private const string HasPasswordQuery = """

                                            SELECT CASE
                                                WHEN PasswordHash IS NOT NULL THEN 1
                                                ELSE 0
                                            END AS HasPassword
                                            FROM AspNetUsers
                                            WHERE Id = @UserId;

                                            """;

    private const string AddToRoleQuery = """

                                          INSERT INTO AspNetUserRoles (UserId, RoleId)
                                          VALUES (@UserId, (SELECT Id FROM AspNetRoles WHERE NormalizedName = @RoleName));

                                          """;

    private const string RemoveFromRoleQuery = """

                                               DELETE FROM AspNetUserRoles
                                               WHERE UserId = @UserId
                                                 AND RoleId = (SELECT Id FROM AspNetRoles WHERE NormalizedName = @RoleName);

                                               """;

    private const string GetRolesQuery = """

                                         SELECT r.Name
                                         FROM AspNetUserRoles ur
                                         INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                                         WHERE ur.UserId = @UserId;
                                             
                                         """;

    private const string IsInRoleQuery = """

                                         SELECT CASE
                                             WHEN EXISTS (
                                                 SELECT 1
                                                 FROM AspNetUserRoles ur
                                                 INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                                                 WHERE ur.UserId = @UserId AND r.NormalizedName = @RoleName
                                             ) THEN 1
                                             ELSE 0
                                         END AS IsInRole;
                                             
                                         """;

    private const string GetUsersInRoleQuery = """
                                                                                                                                                  
                                               SELECT u.*
                                               FROM AspNetUsers u
                                               INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                                               INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                                               WHERE r.NormalizedName = @RoleName;
                                                   
                                               """;

    private const string FindUserByEmailQuery = """

                                                SELECT *
                                                FROM AspNetUsers
                                                WHERE NormalizedEmail = @NormalizedEmail;
                                                    
                                                """;

    private const string AddLoginQuery = """

                                         INSERT INTO AspNetUserLogins (LoginProvider, ProviderKey, ProviderDisplayName, UserId)
                                         VALUES (@LoginProvider, @ProviderKey, @ProviderDisplayName, @UserId);
                                             
                                         """;

    private const string RemoveLoginQuery = """

                                            DELETE FROM AspNetUserLogins
                                            WHERE UserId = @UserId
                                              AND LoginProvider = @LoginProvider
                                              AND ProviderKey = @ProviderKey;
                                                
                                            """;

    private const string GetLoginsQuery = """

                                          SELECT *
                                          FROM AspNetUserLogins
                                          WHERE UserId = @UserId;
                                              
                                          """;

    private const string FindByLoginQuery = """

                                            SELECT u.*
                                            FROM AspNetUsers AS u
                                            INNER JOIN AspNetUserLogins AS ul ON u.Id = ul.UserId
                                            WHERE ul.LoginProvider = @LoginProvider
                                              AND ul.ProviderKey = @ProviderKey;
                                              
                                            """;

    private const string SetTokenQuery = """

                                         INSERT INTO AspNetUserTokens (UserId, LoginProvider, Name, Value)
                                         VALUES (@UserId, @LoginProvider, @Name, @Value);
                                             
                                         """;

    private const string RemoveTokenQuery = """

                                            DELETE FROM AspNetUserTokens
                                            WHERE UserId = @UserId
                                            AND LoginProvider = @LoginProvider
                                            AND Name = @Name;

                                            """;

    private const string GetTokenQuery = """

                                         SELECT * FROM AspNetUserTokens
                                         WHERE UserId = @UserId
                                         AND LoginProvider = @LoginProvider
                                         AND Name = @Name;

                                         """;

    private const string GetClaimsQuery = """

                                          SELECT ClaimType, ClaimValue FROM AspNetUserClaims
                                          WHERE UserId = @UserId

                                          """;

    private const string AddClaimsQuery = """

                                          INSERT INTO AspNetUserClaims (ClaimType, ClaimValue, UserId)
                                          VALUES (@ClaimType, @ClaimValue, @UserId);
                                              
                                          """;

    private const string ReplaceClaimQuery = """

                                             UPDATE AspNetUserClaims
                                             SET ClaimType = @NewClaimType,
                                                 ClaimValue = @NewClaimValue,
                                                 UserId = @UserId
                                             WHERE ClaimType = @ClaimType
                                              AND ClaimValue = @ClaimValue
                                              AND UserId = @UserId
                                             """;

    private const string RemoveClaimsQuery = """

                                             DELETE FROM AspNetUserClaims
                                                    WHERE ClaimType = @ClaimType
                                                    AND ClaimValue = @ClaimValue
                                                    AND UserId = @UserId
                                                 
                                             """;

    private const string GetUsersForClaimsQuery = """

                                                  SELECT * FROM AspNetUserClaims
                                                  WHERE ClaimType = @ClaimType
                                                  AND ClaimValue = @ClaimValue

                                                  """;

    private readonly ILogger<ApplicationUserStore> _logger;

    public ApplicationUserStore(IDatabaseConnection databaseConnection, ILogger<ApplicationUserStore> logger) : base(
        databaseConnection) {
        _logger = logger;
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.UserName);
    }

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await ExecuteQueryAsync(CreateQuery,
            new {
                UserId = user.Id,
                UserName = user.UserName,
                NormalizedUserName = user.NormalizedUserName,
                Email = user.Email,
                NormalizedEmail = string.IsNullOrEmpty(user.Email) ? null : user.Email.ToUpper(),
                EmailConfirmed = user.EmailConfirmed,
                PasswordHash = user.PasswordHash,
                SecurityStamp = user.SecurityStamp,
                ConcurrencyStamp = user.ConcurrencyStamp,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnd = user.LockoutEnd,
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount,
                VisitCount = user.VisitCount,
            });

        return result > 0 ? IdentityResult.Success : IdentityResult.Failed();
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await ExecuteQueryAsync(UpdateQuery,
            new {
                UserId = user.Id,
                UserName = user.UserName,
                NormalizedUserName = user.NormalizedUserName,
                Email = user.Email,
                NormalizedEmail = string.IsNullOrEmpty(user.Email) ? null : user.Email.ToUpper(),
                EmailConfirmed = user.EmailConfirmed,
                PasswordHash = user.PasswordHash,
                SecurityStamp = user.SecurityStamp,
                ConcurrencyStamp = user.ConcurrencyStamp,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnd = user.LockoutEnd,
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount,
                VisitCount = user.VisitCount,
            });

        return result > 0 ? IdentityResult.Success : IdentityResult.Failed();
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await ExecuteQueryAsync(DeleteQuery,
            new {
                UserId = user.Id
            });

        return result > 0 ? IdentityResult.Success : IdentityResult.Failed();
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return await GetSingleOrDefaultAsync<ApplicationUser?>(FindByIdQuery,
            new {
                UserId = userId
            });
    }

    public async Task<ApplicationUser?>
        FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return await GetSingleOrDefaultAsync<ApplicationUser?>(FindByNameQuery,
            new {
                NormalizedUserName = normalizedUserName
            });
    }

    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.PasswordHash);
    }

    public async Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return await GetSingleOrDefaultAsync<bool>(HasPasswordQuery,
            new {
                UserId = user.Id
            });
    }

    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        await ExecuteQueryAsync(AddToRoleQuery,
            new {
                UserId = user.Id,
                RoleName = roleName
            });
    }

    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        await ExecuteQueryAsync(RemoveFromRoleQuery,
            new {
                UserId = user.Id,
                RoleName = roleName
            });
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await GetIEnumerable<string>(GetRolesQuery,
            new {
                UserId = user.Id
            });

        return result.ToList();
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return await GetSingleOrDefaultAsync<bool>(IsInRoleQuery,
            new {
                UserId = user.Id,
                RoleName = roleName
            });
    }

    public async Task<IList<ApplicationUser>>
        GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await GetIEnumerable<ApplicationUser>(GetUsersInRoleQuery,
            new {
                RoleName = roleName
            });

        return result.ToList();
    }

    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return await GetSingleOrDefaultAsync<ApplicationUser?>(FindUserByEmailQuery,
            new {
                NormalizedEmail = normalizedEmail
            });
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public async Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        await ExecuteQueryAsync(AddLoginQuery, new {
            LoginProvider = login.LoginProvider,
            ProviderKey = login.ProviderKey,
            ProviderDisplayName = login.ProviderDisplayName,
            UserId = user.Id
        });
    }

    public async Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        await ExecuteQueryAsync(RemoveLoginQuery, new {
            LoginProvider = loginProvider,
            ProviderKey = providerKey,
            UserId = user.Id
        });
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await GetIEnumerable<UserLoginInfo>(GetLoginsQuery, new {
            UserId = user.Id
        });
        return result.ToList();
    }

    public async Task<ApplicationUser?> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return await GetSingleOrDefaultAsync<ApplicationUser?>(FindByLoginQuery, new {
            LoginProvider = loginProvider,
            ProviderKey = providerKey
        });
    }

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.LockoutEnd);
    }

    public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? lockoutEnd,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.AccessFailedCount);
    }

    public Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.LockoutEnabled);
    }

    public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    public async Task SetTokenAsync(ApplicationUser user, string loginProvider, string name, string? value,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        await ExecuteQueryAsync(SetTokenQuery, new {
            UserId = user.Id,
            LoginProvider = loginProvider,
            Name = name,
            Value = value
        });
    }

    public async Task RemoveTokenAsync(ApplicationUser user, string loginProvider, string name,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        await ExecuteQueryAsync(RemoveTokenQuery, new {
            UserId = user.Id,
            LoginProvider = loginProvider,
            Name = name
        });
    }

    public async Task<string?> GetTokenAsync(ApplicationUser user, string loginProvider, string name,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return await GetSingleOrDefaultAsync<string?>(GetTokenQuery, new {
            UserId = user.Id,
            LoginProvider = loginProvider,
            Name = name
        });
    }

    public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.SecurityStamp);
    }

    public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.TwoFactorEnabled);
    }

    
    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        
        var result = await GetIEnumerable<ClaimModel>(GetClaimsQuery, new {
            UserId = user.Id
        });
        return result.Select(cc => new Claim(cc.ClaimType!, cc.ClaimValue!)).ToList();
    }

    public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var claim in claims) {
            await ExecuteQueryAsync(AddClaimsQuery, new {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                UserId = user.Id
            });
        }
    }

    public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        await ExecuteQueryAsync(ReplaceClaimQuery, new {
            UserId = user.Id,
            NewClaimType = newClaim.Type,
            NewClaimValue = newClaim.Value,
            ClaimType = claim.Type,
            ClaimValue = claim.Value
        });
    }

    public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var claim in claims) {
            await ExecuteQueryAsync(RemoveClaimsQuery, new {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                UserId = user.Id
            });
        }
    }

    public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await GetIEnumerable<ApplicationUser>(GetUsersForClaimsQuery, new {
            ClaimType = claim.Type,
            ClaimValue = claim.Value,
        });
        return result.ToList();
    }
}