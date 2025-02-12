using AzPC.Blazor.App.Helpers;
using AzPC.Blazor.App.Services;
using AzPC.Blazor.App.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AzPC.Blazor.App.Pages;

public partial class Logout : BaseComponent
{
	private string Message { get; set; } = "Logging out, please wait...";

	[Inject]
	private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
		await localStorage.RemoveItemAsync(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
		((JwtAuthenticationStateProvider)AuthenticationStateProvider).NotifyStageChanged();

		NavigationManager.NavigateTo(UIGlobals.ROUTE_LANDING, forceLoad: true);
	}
}
