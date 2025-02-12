﻿using AzPC.Api.Services;
using AzPC.Shared.Api;
using AzPC.Shared.Bootstrap;
using AzPC.Shared.Identity;
using AzPC.Shared.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AzPC.Api.Bootstrap;

/// <summary>
/// Sample bootstrapper that initializes and register an <see cref="IAuthenticator"/> service.
/// </summary>
[Bootstrapper]
public class AuthBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder, IOptions<JwtOptions> jwtOptions)
	{
		// use JwtBearer authentication scheme
		appBuilder.Services.AddAuthentication()
			.AddJwtBearer(options =>
			{
				options.SaveToken = true;
				options.RequireHttpsMetadata = false;
				options.TokenValidationParameters = jwtOptions.Value.TokenValidationParameters;
			});

		// Customize the behavior of the authorization middleware.
		appBuilder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, SampleAuthorizationMiddlewareResultHandler>();

		// Configurate authorization policies
		appBuilder.Services.AddAuthorization(c =>
		{
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_USER_MANAGER);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_APPLICATION_MANAGER, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_APPLICATION_MANAGER);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_USER_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_CREATE_USER_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_USER_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_MODIFY_USER_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_USER_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_DELETE_USER_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_ROLE_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_CREATE_ROLE_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_ROLE_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_MODIFY_ROLE_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_ROLE_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_DELETE_ROLE_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_APP_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_CREATE_APP_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_APP_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_MODIFY_APP_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_APP_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_DELETE_APP_PERM);
		});

		// setup IAuthenticator/IAuthenticatorAsync services
		appBuilder.Services.AddSingleton<SampleJwtAuthenticator>()
			// we want the lookup of both IAuthenticator and IAuthenticatorAsync to return the same SampleJwtAuthenticator instance
			.AddSingleton<IAuthenticator>(x => x.GetRequiredService<SampleJwtAuthenticator>())
			.AddSingleton<IAuthenticatorAsync>(x => x.GetRequiredService<SampleJwtAuthenticator>());
	}

	public static void DecorateApp(WebApplication app)
	{
		// activate authentication/authorization middleware
		app.UseAuthentication();
		app.UseAuthorization();
	}
}

/// <summary>
/// We want unauthorized API calls to return <see cref="ApiResp"/> instead of the default behavior.
/// </summary>
public class SampleAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
	private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();
	private readonly byte[] unauthorizedResult = JsonSerializer.SerializeToUtf8Bytes(new ApiResp
	{
		Status = StatusCodes.Status401Unauthorized,
		Message = "Unauthorized"
	});

	public async Task HandleAsync(
		RequestDelegate next,
		HttpContext context,
		AuthorizationPolicy policy,
		PolicyAuthorizationResult authorizeResult)
	{
		if (!authorizeResult.Succeeded)
		{
			if (authorizeResult.Challenged) await context.ChallengeAsync();
			else if (authorizeResult.Forbidden) await context.ForbidAsync();

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			await context.Response.BodyWriter.WriteAsync(unauthorizedResult);
			return;
		}

		// Fall back to the default implementation.
		await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
	}
}
