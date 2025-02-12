﻿using AzPC.Blazor.App.Helpers;
using AzPC.Blazor.App.Services;
using AzPC.Libs.Opurator;
using AzPC.Shared.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzPC.Blazor.App.Layout;

/// <summary>
/// Base layout class that provides common properties and utility methods.
/// </summary>
public abstract class BaseLayout : LayoutComponentBase
{
	[Inject]
	protected IServiceProvider ServiceProvider { get; init; } = default!;

	[Inject]
	protected NavigationManager NavigationManager { get; init; } = default!;

	[Inject]
	protected StateContainer StateContainer { get; init; } = default!;

	/// <summary>
	/// Check if the component is rendered in WASM mode.
	/// </summary>
	public bool IsBrowser { get => OperatingSystem.IsBrowser(); }

	public AppInfo? AppInfo { get => Globals.AppInfo; }

	public IApiClient ApiClient { get => ServiceProvider.GetRequiredService<IApiClient>(); }

	/// <summary>
	/// Convenience property to obtain the API's base URL.
	/// </summary>
	public string ApiBaseUrl { get => Globals.ApiBaseUrl ?? NavigationManager.BaseUri; }

	/// <summary>
	/// Convenience property to obtain the host environment (Development, Staging, Production).
	/// </summary>
	public string HostEnvironment { get; protected set; } = default!;

	/// <summary>
	/// Convenience method to obtain the authentication token from local storage.
	/// </summary>
	/// <returns>The authentication token, or an empty string if not found.</returns>
	protected virtual async Task<string> GetAuthTokenAsync()
	{
		using (var scope = ServiceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			return await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN) ?? string.Empty;
		}
	}

	public void Dispose()
	{
		StateContainer.OnChange -= StateHasChanged;
	}

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		StateContainer.OnChange += StateHasChanged;
		if (IsBrowser)
		{
			var env = ServiceProvider.GetService<IWebAssemblyHostEnvironment>();
			HostEnvironment = env?.Environment ?? "Unknown";
		}
		else
		{
			var env = ServiceProvider.GetService<IHostingEnvironment>();
			HostEnvironment = env?.EnvironmentName ?? "Unknown";
		}

		if (!IsBrowser)
		{
			var taskExecutor = ServiceProvider.GetRequiredService<ITaskOperator>();
			await InvokeAsync(async () =>
			{
				await taskExecutor.ExecuteOnlyOnceAsync("FetchAppInfo", () =>
				{
					// Get the app info from the configuration, once, if in Blazor Server mode
					// In WASM mode, the app info is automatically fetched from the server and stored in <see cref="Globals.AppInfo"/>
					var conf = ServiceProvider.GetRequiredService<IConfiguration>();
					Globals.AppInfo = conf.GetSection("App").Get<AppInfo>();
				});
				StateHasChanged();
				// StateContainer.NotifyStateChanged();
			});
		}

		// Add your logic here
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		// Add your logic here
	}
}
