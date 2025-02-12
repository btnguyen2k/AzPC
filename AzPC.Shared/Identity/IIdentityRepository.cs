using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AzPC.Shared.Identity;

public class UserFetchOptions
{
	public static readonly UserFetchOptions DEFAULT = new();

	/// <summary>
	/// Fetches the roles along with the user.
	/// </summary>
	/// <returns>New options instance with <see cref="IncludeRoles"/> set to true.</returns>
	public UserFetchOptions FetchRoles()
	{
		return IncludeRoles ? this : new UserFetchOptions
		{
			IncludeRoles = true,
			IncludeClaims = this.IncludeClaims,
			IncludeRoleClaims = this.IncludeRoleClaims,
		};
	}

	/// <summary>
	/// Fetches the claims along with the user.
	/// </summary>
	/// <returns>New options instance with <see cref="IncludeClaims"/> set to true.</returns>
	public UserFetchOptions FetchClaims()
	{
		return IncludeClaims ? this : new UserFetchOptions
		{
			IncludeRoles = this.IncludeRoles,
			IncludeClaims = true,
			IncludeRoleClaims = this.IncludeRoleClaims,
		};
	}

	/// <summary>
	/// Fetches the role claims along with the user.
	/// </summary>
	/// <returns>New options instance with <see cref="IncludeRoleClaims"/> set to true.</returns>
	public UserFetchOptions FetchRoleClaims()
	{
		return IncludeRoleClaims ? this : new UserFetchOptions
		{
			IncludeRoles = this.IncludeRoles,
			IncludeClaims = this.IncludeClaims,
			IncludeRoleClaims = true,
		};
	}

	public bool IncludeRoles { get; set; } = false;
	public bool IncludeClaims { get; set; } = false;
	public bool IncludeRoleClaims { get; set; } = false;
}

public class RoleFetchOptions
{
	public static readonly RoleFetchOptions DEFAULT = new();

	/// <summary>
	/// Fetches the claims along with the role.
	/// </summary>
	/// <returns>New options instance with <see cref="IncludeClaims"/> set to true.</returns>
	public RoleFetchOptions FetchClaims()
	{
		return IncludeClaims ? this : new RoleFetchOptions
		{
			IncludeClaims = true
		};
	}

	public bool IncludeClaims { get; set; } = false;
}

public interface IIdentityRepository
{
	/// <summary>
	/// General error when no changes are saved.
	/// </summary>
	public static readonly IdentityResult NoChangesSaved = IdentityResult.Failed(new IdentityError() { Code = "NoChangesSaved" });

	/// <summary>
	/// Convenience method to check if the result is succeeded or no changes saved.
	/// </summary>
	/// <param name="result"></param>
	/// <returns></returns>
	public static bool IsSucceededOrNoChangesSaved(IdentityResult result)
	{
		return result.Succeeded || result.Errors.Any(e => e.Code == "NoChangesSaved");
	}

	/// <summary>
	/// Creates a new user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> CreateAsync(AzPCUser user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new user if it does not exist.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> CreateIfNotExistsAsync(AzPCUser user, CancellationToken cancellationToken = default);

	ValueTask<AzPCUser?> GetUserByIDAsync(string userId, UserFetchOptions? options = default, CancellationToken cancellationToken = default);
	ValueTask<AzPCUser?> GetUserByEmailAsync(string email, UserFetchOptions? options = default, CancellationToken cancellationToken = default);
	ValueTask<AzPCUser?> GetUserByUserNameAsync(string userName, UserFetchOptions? options = default, CancellationToken cancellationToken = default);
	IAsyncEnumerable<AzPCUser> AllUsersAsync();

	/// <summary>
	/// Updates the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>The user with updated data.</returns>
	/// <remarks>
	///		null is returned if the update operated didnot succeed.
	///		user's concurrency stamp is automatically updated and reflected in the returned instance.
	///	</remarks>
	ValueTask<AzPCUser?> UpdateAsync(AzPCUser user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates the security stamp of the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>The user with new security stamp</returns>
	/// <remarks>
	///		null is returned if the update operated didnot succeed.
	///		user's concurrency stamp is automatically updated and reflected in the returned instance.
	///	</remarks>
	ValueTask<AzPCUser?> UpdateSecurityStampAsync(AzPCUser user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes an existing user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the user does not exist, the operation is considered successful.
	/// </remarks>
	ValueTask<IdentityResult> DeleteAsync(AzPCUser user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves the roles of the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="roleFetchOptions"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the user has no roles, an empty collection is returned.
	///		If the user does not exist, null is returned.
	/// </remarks>
	ValueTask<IEnumerable<AzPCRole>> GetRolesAsync(AzPCUser user, RoleFetchOptions? roleFetchOptions = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves the claims of the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the user has no claims, an empty collection is returned.
	///		If the user does not exist, null is returned.
	/// </remarks>
	ValueTask<IEnumerable<IdentityUserClaim<string>>> GetClaimsAsync(AzPCUser user, CancellationToken cancellationToken = default);

	ValueTask<IdentityResult> AddToRolesAsync(AzPCUser user, IEnumerable<AzPCRole> roles, CancellationToken cancellationToken = default);
	ValueTask<IdentityResult> AddToRolesAsync(AzPCUser user, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);
	ValueTask<IdentityResult> AddToRoleIfNotExistsAsync(AzPCUser user, AzPCRole role, CancellationToken cancellationToken = default);
	ValueTask<IdentityResult> AddToRoleIfNotExistsAsync(AzPCUser user, string roleName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes the user from the specified roles.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="roles"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the user is not in the specified roles, the operation is considered successful.
	/// </remarks>
	ValueTask<IdentityResult> RemoveFromRolesAsync(AzPCUser user, IEnumerable<AzPCRole> roles, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes the user from the specified roles.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="roleNames"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the user is not in the specified roles, the operation is considered successful.
	/// </remarks>
	ValueTask<IdentityResult> RemoveFromRolesAsync(AzPCUser user, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds claims to the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="claims"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> AddClaimsAsync(AzPCUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a claim to the user, if it does not exist.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="claim"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> AddClaimIfNotExistsAsync(AzPCUser user, Claim claim, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes claims from the user.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="claims"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> RemoveClaimsAsync(AzPCUser user, IEnumerable<IdentityUserClaim<string>> claims, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes claims from the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="claims"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> RemoveClaimsAsync(AzPCUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default);

	/*----------------------------------------------------------------------*/

	/// <summary>
	/// Creates a new role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> CreateAsync(AzPCRole role, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new role if it does not exist.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> CreateIfNotExistsAsync(AzPCRole role, CancellationToken cancellationToken = default);

	ValueTask<AzPCRole?> GetRoleByIDAsync(string roleId, RoleFetchOptions? options = default, CancellationToken cancellationToken = default);
	ValueTask<AzPCRole?> GetRoleByNameAsync(string roleName, RoleFetchOptions? options = default, CancellationToken cancellationToken = default);

	IAsyncEnumerable<AzPCRole> AllRolesAsync();

	/// <summary>
	/// Updates the role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>The role with updated data.</returns>
	/// <remarks>
	///		null is returned if the update operated didnot succeed.
	///		role's concurrency stamp is automatically updated and reflected in the returned instance.
	///	</remarks>
	ValueTask<AzPCRole?> UpdateAsync(AzPCRole role, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes an existing role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the role does not exist, the operation is considered successful.
	/// </remarks>
	ValueTask<IdentityResult> DeleteAsync(AzPCRole role, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves the claims of the role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the role has no claims, an empty collection is returned.
	///		If the role does not exist, null is returned.
	/// </remarks>
	ValueTask<IEnumerable<IdentityRoleClaim<string>>> GetClaimsAsync(AzPCRole role, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds claims to the role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="claims"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> AddClaimsAsync(AzPCRole role, IEnumerable<Claim> claims, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a claim to the role, if it does not exist.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="claim"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> AddClaimIfNotExistsAsync(AzPCRole role, Claim claim, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes claims from the role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="claims"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> RemoveClaimsAsync(AzPCRole role, IEnumerable<IdentityRoleClaim<string>> claims, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes claims from the role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="claims"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<IdentityResult> RemoveClaimsAsync(AzPCRole role, IEnumerable<Claim> claims, CancellationToken cancellationToken = default);
}
