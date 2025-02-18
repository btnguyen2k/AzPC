using AzPC.Shared.Bootstrap;

namespace AzPC.Api.Bootstrap;

/// <summary>
/// Bootstrapper that initializes Azure static data.
/// </summary>
[Bootstrapper]
public class AzureBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<AzureBootstrapper>();
		logger.LogInformation("Initializing Azure static data...");

		appBuilder.Services.AddHostedService<AzureRegionsInitializer>();
	}
}
