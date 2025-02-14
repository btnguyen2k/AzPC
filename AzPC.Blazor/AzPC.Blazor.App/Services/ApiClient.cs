﻿using AzPC.Shared.Api;
using System.Net.Http.Json;
using System.Text.Json;

namespace AzPC.Blazor.App.Services;

public class ApiClient : IApiClient
{
	private readonly HttpClient defaultHttpClient;

	public ApiClient(HttpClient httpClient)
	{
		defaultHttpClient = httpClient;
	}

	private void UsingBaseUrlAndHttpClient(string? baseUrl, HttpClient? requestHttpClient, out string usingBaseUrl, out HttpClient usingHttpClient)
	{
		usingBaseUrl = string.IsNullOrEmpty(baseUrl) ? (Globals.ApiBaseUrl ?? string.Empty) : baseUrl;
		usingHttpClient = requestHttpClient ?? defaultHttpClient;
	}

	private static HttpRequestMessage BuildRequest(HttpMethod method, Uri endpoint, string? authToken, object? requestData)
	{
		var req = new HttpRequestMessage(method, endpoint)
		{
			Headers = {
				{ "Accept", "application/json" }
			}
		};
		if (!string.IsNullOrEmpty(authToken))
		{
			req.Headers.Add("Authorization", $"Bearer {authToken}");
		}
		if (requestData != null)
		{
			req.Content = JsonContent.Create(requestData);
		}
		return req;
	}

	private static readonly string NoAuth = string.Empty;
	private static readonly object? NoData = null;

	private async Task<HttpResponseMessage> BuildAndSendRequestAsync(HttpClient? requestHttpClient, HttpMethod method, string? baseUrl, string apiEndpoint, string? authToken, object? requestData, CancellationToken cancellationToken)
	{
		UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), apiEndpoint);
		// Console.WriteLine($"[DEBUG] calling {method.ToString()}:{apiUri.ToString()}");
		var httpReq = BuildRequest(method, apiUri, authToken, requestData);
		return await usingHttpClient.SendAsync(httpReq, cancellationToken);
	}

	private static async Task<ApiResp<T>> ReadResponseAsync<T>(HttpResponseMessage httpResult, CancellationToken cancellationToken)
	{
		try
		{
			var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<T>>(cancellationToken);
			if (result == null)
			{
				// var statusCodes = (int)httpResult.StatusCode;
				// var content = await httpResult.Content.ReadAsStringAsync(cancellationToken);
				// Console.WriteLine($"[ERROR] Invalid response from server. Status: {statusCodes}, Type: {typeof(T).Name},\nContent: {content}");
				return new ApiResp<T> { Status = 500, Message = "Invalid response from server." };
			}
			return result;
		}
		catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException || ex is OperationCanceledException)
		{
			// Console.WriteLine($"[ERROR] Error reading response: {ex.Message}");
			// var statusCodes = (int)httpResult.StatusCode;
			// var content = await httpResult.Content.ReadAsStringAsync(cancellationToken);
			// Console.WriteLine($"[ERROR] Status: {statusCodes}, Type: {typeof(T).Name},\nContent: {content}");
			return new ApiResp<T> { Status = 500, Message = ex.Message };
		}
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<InfoResp>> InfoAsync(string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_INFO,
			NoAuth,
			NoData,
			cancellationToken
		);
		return await ReadResponseAsync<InfoResp>(httpResult, cancellationToken);
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<AuthResp>> LoginAsync(AuthReq req, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_AUTH_SIGNIN,
			NoAuth,
			req,
			cancellationToken
		);
		return await ReadResponseAsync<AuthResp>(httpResult, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<ApiResp<AuthResp>> RefreshAsync(string authToken, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_AUTH_REFRESH,
			authToken,
			NoData,
			cancellationToken
		);
		return await ReadResponseAsync<AuthResp>(httpResult, cancellationToken);
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<UserResp>> GetMyInfoAsync(string authToken, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_USERS_ME,
			authToken,
			NoData,
			cancellationToken
		);
		return await ReadResponseAsync<UserResp>(httpResult, cancellationToken);
	}

	// /// <inheritdoc/>
	// public async Task<ApiResp<UserResp>> UpdateMyProfileAsync(UpdateUserProfileReq req, string authToken, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_USERS_ME_PROFILE,
	// 		authToken,
	// 		req,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<UserResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<ChangePasswordResp>> ChangeMyPasswordAsync(ChangePasswordReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_USERS_ME_PASSWORD,
	// 		authToken,
	// 		req,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<ChangePasswordResp>(httpResult, cancellationToken);
	// }

	/*----------------------------------------------------------------------*/

	// /// <inheritdoc/>
	// public async Task<ApiResp<IEnumerable<UserResp>>> GetAllUsersAsync(string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_USERS,
	// 		authToken,
	// 		NoData,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<IEnumerable<UserResp>>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<UserResp>> CreateUserAsync(CreateOrUpdateUserReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_USERS,
	// 		authToken,
	// 		req,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<UserResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<UserResp>> GetUserAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_USERS_ID.Replace("{id}", id),
	// 		authToken,
	// 		NoData,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<UserResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<UserResp>> DeleteUserAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Delete, baseUrl, IApiClient.API_ENDPOINT_USERS_ID.Replace("{id}", id),
	// 		authToken,
	// 		NoData,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<UserResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<UserResp>> UpdateUserAsync(string id, CreateOrUpdateUserReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Put, baseUrl, IApiClient.API_ENDPOINT_USERS_ID.Replace("{id}", id),
	// 		authToken,
	// 		req,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<UserResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<IEnumerable<ClaimResp>>> GetAllClaimsAsync(string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_CLAIMS,
	// 		authToken,
	// 		NoData,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<IEnumerable<ClaimResp>>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<IEnumerable<RoleResp>>> GetAllRolesAsync(string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_ROLES,
	// 		authToken,
	// 		NoData,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<IEnumerable<RoleResp>>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<RoleResp>> CreateRoleAsync(CreateOrUpdateRoleReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_ROLES,
	// 		authToken,
	// 		req,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<RoleResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<RoleResp>> GetRoleAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_ROLES_ID.Replace("{id}", id),
	// 		authToken,
	// 		NoData,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<RoleResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<RoleResp>> DeleteRoleAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Delete, baseUrl, IApiClient.API_ENDPOINT_ROLES_ID.Replace("{id}", id),
	// 		authToken,
	// 		NoData,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<RoleResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<RoleResp>> UpdateRoleAsync(string id, CreateOrUpdateRoleReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Put, baseUrl, IApiClient.API_ENDPOINT_ROLES_ID.Replace("{id}", id),
	// 		authToken,
	// 		req,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<RoleResp>(httpResult, cancellationToken);
	// }

	/*----------------------------------------------------------------------*/

	// /// <inheritdoc/>
	// public async Task<ApiResp<IEnumerable<AppResp>>> GetAllAppsAsync(string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_APPS,
	// 		authToken,
	// 		NoData,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<IEnumerable<AppResp>>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<AppResp>> CreateAppAsync(CreateOrUpdateAppReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_APPS,
	// 		authToken,
	// 		req,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<AppResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<AppResp>> GetAppAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_APPS_ID.Replace("{id}", id),
	// 		authToken,
	// 		NoData,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<AppResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<AppResp>> DeleteAppAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Delete, baseUrl, IApiClient.API_ENDPOINT_APPS_ID.Replace("{id}", id),
	// 		authToken,
	// 		NoData,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<AppResp>(httpResult, cancellationToken);
	// }

	// /// <inheritdoc/>
	// public async Task<ApiResp<AppResp>> UpdateAppAsync(string id, CreateOrUpdateAppReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	// {
	// 	var httpResult = await BuildAndSendRequestAsync(
	// 		requestHttpClient,
	// 		HttpMethod.Put, baseUrl, IApiClient.API_ENDPOINT_APPS_ID.Replace("{id}", id),
	// 		authToken,
	// 		req,
	// 		cancellationToken
	// 	);
	// 	return await ReadResponseAsync<AppResp>(httpResult, cancellationToken);
	// }

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<IEnumerable<string>>> GetExternalAuthProvidersAsync(string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			httpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_EXTERNAL_AUTH_PROVIDERS,
			NoAuth,
			NoData,
			cancellationToken
		);
		return await ReadResponseAsync<IEnumerable<string>>(httpResult, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<ApiResp<string>> GetExternalAuthUrlAsync(ExternalAuthUrlReq req, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			httpClient,
			HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_EXTERNAL_AUTH_URL,
			NoAuth,
			req,
			cancellationToken
		);
		return await ReadResponseAsync<string>(httpResult, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<ApiResp<ExternalAuthResp>> ExternalLoginAsync(ExternalAuthReq req, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			httpClient,
			HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_EXTERNAL_AUTH_LOGIN,
			NoAuth,
			req,
			cancellationToken
		);
		return await ReadResponseAsync<ExternalAuthResp>(httpResult, cancellationToken);
	}

	/*----------------------------------------------------------------------*/

	/* FOR DEMO PURPOSES ONLY! */
	public async Task<ApiResp<IEnumerable<UserResp>>> GetSeedUsersAsync(string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, "/api/demo/seed_users",
			NoAuth,
			NoData,
			cancellationToken
		);
		return await ReadResponseAsync<IEnumerable<UserResp>>(httpResult, cancellationToken);
	}
}
