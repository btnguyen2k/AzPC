using AzPC.Blazor.App.Helpers;
using AzPC.Blazor.App.Services;
using AzPC.Libs.Opurator;
using AzPC.Shared.Api;
using AzPC.Shared.Bootstrap;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace AzPC.Blazor.App.Bootstrap;

/// <summary>
/// Bootstrapper that registers services used by Blazor application.
/// </summary>
/// <remarks>
///		Bootstrappers in Blazor.App project are shared between for Blazor Server and Blazor WASM.
/// </remarks>
[Bootstrapper]
public class BlazorServicesBootstrapper
{
	public static void ConfigureServices(IServiceCollection services)
	{
		services.AddHttpClient();
		services.AddSingleton<IApiClient, ApiClient>();
		services.AddBlazoredLocalStorage();
		services.AddScoped<LocalStorageHelper>();
		services.AddTaskOperator();
		services.AddSingleton<StateContainer>();

		// https://stackoverflow.com/questions/52889827/remove-http-client-logging-handler-in-asp-net-core
		services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
	}
}
