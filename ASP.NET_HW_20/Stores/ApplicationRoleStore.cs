using System.Security.Claims;
using ASP.NET_HW_20.Data;
using Microsoft.AspNetCore.Identity;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace ASP.NET_HW_20.Stores;

public class ApplicationRoleStore : ApplicationStore, IRoleStore<IdentityRole> {
    private const string CreateRoleQuery = """
                                           
                                                   INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                                                   VALUES (@RoleId, @RoleName, @NormalizedRoleName, @ConcurrencyStamp);
                                               
                                           """;

    private const string UpdateRoleQuery = """
                                           
                                                   UPDATE AspNetRoles
                                                   SET Name = @RoleName,
                                                       NormalizedName = @NormalizedRoleName,
                                                       ConcurrencyStamp = @ConcurrencyStamp
                                                   WHERE Id = @RoleId;
                                               
                                           """;

    private const string DeleteRoleQuery = """
                                           
                                                   DELETE FROM AspNetRoles
                                                   WHERE Id = @RoleId;
                                               
                                           """;

    private const string FindRoleByIdQuery = """
                                             
                                                     SELECT *
                                                     FROM AspNetRoles
                                                     WHERE Id = @RoleId;
                                                 
                                             """;

    private const string FindRoleByNameQuery = """
                                                                                                                           
                                                       SELECT *
                                                       FROM AspNetRoles
                                                       WHERE NormalizedName = @NormalizedRoleName;
                                                   
                                               """;

    public ApplicationRoleStore(IDatabaseConnection databaseConnection) : base(databaseConnection) { }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }

    public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await ExecuteQueryAsync(CreateRoleQuery, new {
            RoleId = role.Id,
            RoleName = role.Name,
            NormalizedRoleName = role.NormalizedName,
            ConcurrencyStamp = role.ConcurrencyStamp
        });

        return result > 0 ? IdentityResult.Success : IdentityResult.Failed();
    }

    public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await ExecuteQueryAsync(UpdateRoleQuery, new {
            RoleId = role.Id,
            RoleName = role.Name,
            NormalizedRoleName = role.NormalizedName
        });

        return result > 0 ? IdentityResult.Success : IdentityResult.Failed();
    }

    public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await ExecuteQueryAsync(DeleteRoleQuery, new {
            RoleId = role.Id
        });

        return result > 0 ? IdentityResult.Success : IdentityResult.Failed();
    }

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(role.Id);
    }

    public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return await GetSingleOrDefaultAsync<IdentityRole>(FindRoleByIdQuery, new { RoleId = roleId });
    }

    public async Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return await GetSingleOrDefaultAsync<IdentityRole>(FindRoleByNameQuery,
            new { NormalizedRoleName = normalizedRoleName });
    }
}