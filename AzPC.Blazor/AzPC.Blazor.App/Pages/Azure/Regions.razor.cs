using AzPC.Shared.Azure;
using Microsoft.AspNetCore.WebUtilities;

namespace AzPC.Blazor.App.Pages.Azure;

public partial class Regions
{
	private bool HideUI { get; set; } = false;

	private string AlertMessage { get; set; } = string.Empty;
	private string AlertType { get; set; } = "info";

	private int RegionIndex = 0;
	private IEnumerable<AzureRegion>? RegionList { get; set; }

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

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		RegionIndex = 0;
		if (firstRender)
		{
			HideUI = true;
			ShowAlert("info", "Loading applications...");
			var result = await ApiClient.GetAzureRegionsAsync(await GetAuthTokenAsync(), ApiBaseUrl);
			if (result.Status == 200)
			{
				HideUI = false;
				RegionList = result.Data?.OrderBy(r => r.Name);
				var queryParameters = QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query);
				var alertMessage = queryParameters.TryGetValue("alertMessage", out var alertMessageValue) ? alertMessageValue.ToString() : string.Empty;
				var alertType = queryParameters.TryGetValue("alertType", out var alertTypeValue) ? alertTypeValue.ToString() : string.Empty;
				if (!string.IsNullOrEmpty(alertMessage) && !string.IsNullOrEmpty(alertType))
				{
					ShowAlert(alertType!, alertMessage!);
				}
				else
				{
					CloseAlert();
				}
			}
			else
			{
				ShowAlert("danger", result.Message ?? "Unknown error");
			}
		}
	}
}
