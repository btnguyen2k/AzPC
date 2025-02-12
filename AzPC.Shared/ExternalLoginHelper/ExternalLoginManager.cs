namespace AzPC.Shared.ExternalLoginHelper;

/// <summary>
/// Generic exception for external login errors.
/// </summary>
public class ExternalLoginException : Exception
{
	protected ExternalLoginException(string message) : base(message) { }
}

public sealed class ProviderNotSupported : ExternalLoginException
{
	public ProviderNotSupported(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when no provider is found.
/// </summary>
public sealed class NoProviderException : ExternalLoginException
{
	public NoProviderException(string message) : base(message) { }
}

/// <summary>
/// The request for building the authentication URL.
/// </summary>
public sealed class BuildAuthUrlReq
{
	/// <summary>
	/// The redirect URL after authentication.
	/// </summary>
	/// <remarks>Do NOT escape the URL! The URL will be escaped by the <c cref="ExternalLoginManager"/>.</remarks>
	public string RedirectUrl { get; set; } = default!;

	/// <summary>
	/// The scopes to request.
	/// </summary>
	public ISet<string>? Scopes { get; set; } = default;

	/// <summary>
	/// A value included in the request that's also returned in the token response.
	/// It can be a string of any content that you wish.
	/// A randomly generated unique value is typically used for preventing cross-site request forgery attacks.
	/// This property is also used to encode information about the user's state in the app before the authentication request occurred,
	/// such as the page or view they were on.
	/// </summary>
	public string State { get; set; } = Guid.NewGuid().ToString("N");
}

public sealed partial class ExternalLoginManager
{
	private IDictionary<string, ExternalLoginProviderConfig> Providers { get; } = new Dictionary<string, ExternalLoginProviderConfig>();
	private HttpClient HttpClient { get; set; } = default!;

	internal ExternalLoginManager(IDictionary<string, ExternalLoginProviderConfig> providers) : this(providers, null) { }

	internal ExternalLoginManager(IDictionary<string, ExternalLoginProviderConfig> providers, HttpClient? httpClient)
	{
		HttpClient = httpClient ?? new HttpClient();
		foreach (var (providerName, providerConfig) in providers)
		{
			if (providerConfig != null)
			{
				providerConfig.ProviderName = providerName;
				Providers[providerName] = providerConfig;
			}
		}
	}

	/// <summary>
	/// Gets all available provider names.
	/// </summary>
	/// <returns></returns>
	public IEnumerable<string> GetProviderNames()
	{
		return Providers.Keys.OrderBy(k => k);
	}

	/*----------------------------------------------------------------------*/

	/// <summary>
	/// Builds the authentication URL for the specified provider.
	/// </summary>
	/// <param name="providerName">The provider name.</param>
	/// <param name="req">The request.</param>
	/// <returns>The authentication URL.</returns>
	/// <exception cref="NoProviderException">Thrown when the provider is not found.</exception>
	/// <exception cref="ProviderNotSupported">Thrown when the provider is not supported.</exception>
	/// <exception cref="ExternalLoginException">Thrown when an error occurs while building the authentication URL.</exception>
	public string BuildAuthenticationUrl(string providerName, BuildAuthUrlReq req)
	{
		if (!Providers.TryGetValue(providerName, out var providerConfig))
		{
			throw new NoProviderException($"Provider '{providerName}' not found.");
		}

		providerName = providerName.ToUpper();
		return providerName switch
		{
			"MICROSOFT" => BuildAuthenticationUrlMicrosoft(providerConfig, req),
			_ => throw new ProviderNotSupported($"Provider '{providerName}' is not supported."),
		};
	}

	/// <summary>
	/// Tries to authenticate the user using the external provider.
	/// </summary>
	/// <param name="providerName">The provider name.</param>
	/// <param name="authReq">The authentication request.</param>
	/// <returns>The external login result.</returns>
	/// <exception cref="NoProviderException">Thrown when the provider is not found.</exception>
	/// <exception cref="ProviderNotSupported">Thrown when the provider is not supported.</exception>
	/// <exception cref="ExternalLoginException">Thrown when an error occurs while getting the external login information.</exception>
	public async Task<ExternalLoginResult> AuthenticateAsync(string providerName, IDictionary<string, string> authReq)
	{
		if (!Providers.TryGetValue(providerName, out var providerConfig))
		{
			throw new NoProviderException($"Provider '{providerName}' not found.");
		}

		providerName = providerName.ToUpper();
		return providerName switch
		{
			"MICROSOFT" => await AuthenticateMicrosoftAsync(providerConfig, authReq),
			_ => throw new ProviderNotSupported($"Provider '{providerName}' is not supported."),
		};
	}

	/// <summary>
	/// Gets the user profile from the external provider.
	/// </summary>
	/// <param name="providerName">The provider name.</param>
	/// <param name="accessToken">The access token.</param>
	/// <returns>The external user profile.</returns>
	/// <exception cref="NoProviderException">Thrown when the provider is not found.</exception>
	/// <exception cref="ProviderNotSupported">Thrown when the provider is not supported.</exception>
	/// <exception cref="ExternalLoginException">Thrown when an error occurs while getting the user profile.</exception>
	public async Task<ExternalUserProfile> GetUserProfileAsync(string providerName, string accessToken)
	{
		// if (!Providers.TryGetValue(providerName, out var providerConfig))
		// {
		// 	throw new NoProviderException($"Provider '{providerName}' not found.");
		// }

		providerName = providerName.ToUpper();
		return providerName switch
		{
			"MICROSOFT" => await GetUserProfileMicrosoftAsync(accessToken),
			_ => throw new ProviderNotSupported($"Provider '{providerName}' is not supported."),
		};
	}
}
