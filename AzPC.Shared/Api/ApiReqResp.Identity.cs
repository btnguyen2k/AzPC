﻿using System.Text.Json.Serialization;
using AzPC.Shared.Identity;

namespace AzPC.Shared.Api;

/// <summary>
/// Request info for the <see cref="IApiClient.ChangeMyPasswordAsync(ChangePasswordReq, string, string?, HttpClient?, CancellationToken)"/> API call.
/// </summary>
public struct ChangePasswordReq
{
	[JsonPropertyName("current_password")]
	public string CurrentPassword { get; set; }

	[JsonPropertyName("new_password")]
	public string NewPassword { get; set; }
}

/// <summary>
/// Response info for the <see cref="IApiClient.ChangeMyPasswordAsync(ChangePasswordReq, string, string?, HttpClient?, CancellationToken)"/> API call.
/// </summary>
public struct ChangePasswordResp
{
	[JsonPropertyName("message")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Message { get; set; }

	[JsonPropertyName("token")]
	public string Token { get; set; }
}

/*----------------------------------------------------------------------*/

/// <summary>
/// Request info for the <see cref="IApiClient.UpdateMyProfileAsync(UpdateUserProfileReq, string, string?, HttpClient?, CancellationToken)"/> API call.
/// </summary>
public struct UpdateUserProfileReq
{
	[JsonPropertyName("given_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? GivenName { get; set; }

	[JsonPropertyName("family_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? FamilyName { get; set; }

	[JsonPropertyName("email")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Email { get; set; }
}

/// <summary>
/// Request to create a new user or update an exixting one.
/// </summary>
public struct CreateOrUpdateUserReq
{
	[JsonPropertyName("username")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Username { get; set; }

	[JsonPropertyName("email")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Email { get; set; }

	[JsonPropertyName("password")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Password { get; set; }

	[JsonPropertyName("given_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? GivenName { get; set; }

	[JsonPropertyName("family_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? FamilyName { get; set; }

	[JsonPropertyName("roles")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<string>? Roles { get; set; }

	[JsonPropertyName("claims")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<IdentityClaim>? Claims { get; set; }
}

/// <summary>
/// Response structure for APIs that return user information.
/// </summary>
public struct UserResp
{
	public static UserResp BuildFromUser(AzPCUser user)
	{
		return new UserResp
		{
			Id = user.Id,
			Username = user.UserName!,
			Email = user.Email!,
			GivenName = user.GivenName,
			FamilyName = user.FamilyName,
			Roles = user.Roles?.Select(r => RoleResp.BuildFromRole(r)),
			Claims = user.Claims?.Select(c => new ClaimResp { ClaimType = c.ClaimType!, ClaimValue = c.ClaimValue! }),
		};
	}

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("username")]
	public string Username { get; set; }

	[JsonPropertyName("email")]
	public string Email { get; set; }

	[JsonPropertyName("password")]
	public string? Password { get; set; }

	[JsonPropertyName("given_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? GivenName { get; set; }

	[JsonPropertyName("family_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? FamilyName { get; set; }

	[JsonPropertyName("roles")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<RoleResp>? Roles { get; set; }

	[JsonPropertyName("claims")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<ClaimResp>? Claims { get; set; }
}

/*----------------------------------------------------------------------*/

public struct ClaimResp
{
	[JsonPropertyName("type")]
	public string ClaimType { get; set; }

	[JsonPropertyName("value")]
	public string ClaimValue { get; set; }
}

/*----------------------------------------------------------------------*/

/// <summary>
/// Request to create a new role or update an exixting one.
/// </summary>
public struct CreateOrUpdateRoleReq
{
	[JsonPropertyName("name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Name { get; set; }

	[JsonPropertyName("description")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Description { get; set; }

	[JsonPropertyName("claims")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<IdentityClaim>? Claims { get; set; }
}

public struct RoleResp
{
	public static RoleResp BuildFromRole(AzPCRole role)
	{
		return new RoleResp
		{
			Id = role.Id,
			Name = role.Name ?? string.Empty,
			Description = role.Description ?? string.Empty,
			Claims = role.Claims?.Select(c => new ClaimResp { ClaimType = c.ClaimType!, ClaimValue = c.ClaimValue! }),
		};
	}

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("claims")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<ClaimResp>? Claims { get; set; }
}
