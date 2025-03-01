using AzPC.Blazor.App.Shared;
using AzPC.Shared.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace AzPC.Blazor.App.Pages;

public partial class Login : BaseComponent
{
	private CModal ModalDialog { get; set; } = default!;

	private void ShowModalNotImplemented()
	{
		ModalDialog.Open();
	}

	private IEnumerable<string> ExternalAuthProviders { get; set; } = [];

	private string AlertType { get; set; } = "info";
	private string AlertMessage { get; set; } = string.Empty;
	private bool HideLoginForm { get; set; } = false;
	private bool DisableExternalLogin { get; set; } = false;

	private void CloseAlert()
	{
		AlertMessage = string.Empty;
		StateHasChanged();
	}

	private void ShowAlert(string type, string message)
	{
		AlertType = type;
		AlertMessage = message;
		StateHasChanged();
	}

	protected override async Task OnInitializedAsync()
	{
		ShowAlert("waiting", "Please wait...");
		await base.OnInitializedAsync();

		var providers = await ApiClient.GetExternalAuthProvidersAsync(ApiBaseUrl);
		ExternalAuthProviders = providers.Data ?? [];

		CloseAlert();
	}

	private async void BtnClickExternalLogin(string provider)
	{
		HideLoginForm = DisableExternalLogin = true;
		ShowAlert("waiting", "Redirecting to external login provider...");
		var returnUrl = QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query)
			.TryGetValue("returnUrl", out var returnUrlValue) ? returnUrlValue.FirstOrDefault("/") : "/";

		ApiResp<string> apiResult;
		switch (provider)
		{
			case "Microsoft":
				var uriBuilder = new UriBuilder(NavigationManager.BaseUri)
				{
					Path = UIGlobals.ROUTE_LOGIN_EXTERNAL_MICROSOFT,
					Query = QueryHelpers.AddQueryString(string.Empty, "returnUrl", returnUrl)
				};
				apiResult = await ApiClient.GetExternalAuthUrlAsync(new ExternalAuthUrlReq()
				{
					Provider = provider,
					RedirectUrl = uriBuilder.ToString()
				}, ApiBaseUrl);
				break;
			default:
				HideLoginForm = DisableExternalLogin = false;
				ShowModalNotImplemented();
				return;
		}

		if (apiResult.Status != 200)
		{
			HideLoginForm = DisableExternalLogin = false;
			ShowAlert("danger", $"{apiResult.Status}: {apiResult.Message ?? "Failed to get external auth URL."}");
			return;
		}

		await Task.Delay(100); // UI hack to have the alert displayed before redirecting
		NavigationManager.NavigateTo(apiResult.Data ?? UIGlobals.ROUTE_HOME);
	}
}
