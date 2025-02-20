using AzPC.Shared.Azure;

namespace AzPC.Shared.Api;

public interface IApiClient
{
	public const string API_ENDPOINT_HEALTH = "/health";
	public const string API_ENDPOINT_HEALTHZ = "/healthz";
	public const string API_ENDPOINT_READY = "/ready";
	public const string API_ENDPOINT_INFO = "/info";

	public const string API_ENDPOINT_AUTH_SIGNIN = "/auth/signin";
	public const string API_ENDPOINT_AUTH_LOGIN = "/auth/login";
	public const string API_ENDPOINT_AUTH_REFRESH = "/auth/refresh";

	public const string API_ENDPOINT_USERS_ME = "/api/users/-me";

	public const string API_ENDPOINT_EXTERNAL_AUTH_PROVIDERS = "/api/external-auth/providers";
	public const string API_ENDPOINT_EXTERNAL_AUTH_LOGIN = "/api/external-auth/login";
	public const string API_ENDPOINT_EXTERNAL_AUTH_URL = "/api/external-auth/url";

	public const string API_ENDPOINT_AZURE_REGIONS = "/api/azure/regions";

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_INFO"/> to get the backend information.
	/// </summary>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<InfoResp>> InfoAsync(string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_AUTH_LOGIN"/> to sign in a user.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AuthResp>> LoginAsync(AuthReq req, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_AUTH_REFRESH"/> to refresh current user's authentication token.
	/// </summary>
	/// <param name="authToken">The authentication token to authenticate current user.</param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AuthResp>> RefreshAsync(string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_USERS_ME"/> to get current user's information.
	/// </summary>
	/// <param name="authToken">The authentication token to authenticate current user.</param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<UserResp>> GetMyInfoAsync(string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_EXTERNAL_AUTH_PROVIDERS"/> to get the list of external authentication providers.
	/// </summary>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<IEnumerable<string>>> GetExternalAuthProvidersAsync(string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_EXTERNAL_AUTH_URL"/> to get the authentication URL for the external provider.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<string>> GetExternalAuthUrlAsync(ExternalAuthUrlReq req, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);


	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_EXTERNAL_AUTH_LOGIN"/> to sign in an external user.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<ExternalAuthResp>> ExternalLoginAsync(ExternalAuthReq req, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/*----------------------------------------------------------------------*/

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_AZURE_REGIONS"/> to get the list of Azure regions.
	/// </summary>
	/// <param name="authToken">The authentication token to authenticate current user.</param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<List<AzureRegion>>> GetAzureRegionsAsync(string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);
}
