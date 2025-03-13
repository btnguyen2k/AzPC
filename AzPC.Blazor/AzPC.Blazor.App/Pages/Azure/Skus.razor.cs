using AzPC.Blazor.App.Helpers;
using AzPC.Shared.Azure;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace AzPC.Blazor.App.Pages.Azure;

public partial class Skus
{
	private bool HideUI { get; set; } = false;

	private string AlertMessage { get; set; } = string.Empty;
	private string AlertType { get; set; } = "info";

	private string SelectedServiceFamily { get; set; } = string.Empty;
	private string SelectedService { get; set; } = string.Empty;
	private IEnumerable<AzureServiceFamily>? ServiceFamilyList { get; set; }
	private IEnumerable<AzureService>? ServiceList { get; set; }
	private IEnumerable<AzureProduct>? ProductList { get; set; }
	private Dictionary<string, IEnumerable<AzureService>> ServiceMap { get; set; } = [];
	private Dictionary<string, IEnumerable<AzureProduct>> ProductMap { get; set; } = [];

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
		if (firstRender)
		{
			HideUI = true;
			ShowAlert("info", "Loading Azure products...");
			SelectedServiceFamily = string.Empty;
			SelectedService = string.Empty;
			// SelectedProduct = string.Empty;
			var result = await ApiClient.GetAzureProductsAsync(await GetAuthTokenAsync(), ApiBaseUrl);
			if (result.Status == 200)
			{
				HideUI = false;
				ServiceFamilyList = result.Data?.OrderBy(sf => sf.Name);
				ServiceMap = [];
				ProductMap = [];
				foreach (var serviceFamily in ServiceFamilyList!)
				{
					var serviceList = serviceFamily.Services.Values.OrderBy(s => s.Name);
					ServiceMap.Add(serviceFamily.Name, serviceList);
					foreach (var service in serviceList)
					{
						var productList = service.Products.Values.OrderBy(p => p.Name);
						ProductMap.Add(service.Id, productList);
					}
				}
				CloseAlert();
			}
			else
			{
				ShowAlert("danger", result.Message ?? "Unknown error");
			}

			await LoadSelection();
			EventChangeServiceOrFamily(SelectedServiceFamily, SelectedService, false);
		}
	}

	private async Task SaveSelection()
	{
		var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
		await localStorage.SetItemAsync("SelectedServiceFamily", SelectedServiceFamily);
		await localStorage.SetItemAsync("SelectedService", SelectedService);
	}
	private async Task LoadSelection()
	{
		var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
		SelectedServiceFamily = await localStorage.GetItemAsync<string>("SelectedServiceFamily") ?? string.Empty;
		SelectedService = await localStorage.GetItemAsync<string>("SelectedService") ?? string.Empty;
	}

	private void OnServiceFamilyChanged(ChangeEventArgs e)
	{
		var serviceFamily = e.Value?.ToString() ?? string.Empty;
		if (!SelectedServiceFamily.Equals(serviceFamily, StringComparison.InvariantCulture))
		{
			EventChangeServiceOrFamily(serviceFamily, string.Empty);
		}
	}
	private void OnServiceChanged(ChangeEventArgs e)
	{
		var service = e.Value?.ToString() ?? string.Empty;
		if (!SelectedService.Equals(service, StringComparison.InvariantCulture))
		{
			EventChangeServiceOrFamily(SelectedServiceFamily, service);
		}
	}

	private async void EventChangeServiceOrFamily(string serviceFamily, string service, bool saveSelection = true)
	{
		ServiceList = ServiceMap.TryGetValue(serviceFamily, out var services) ? services : null;
		ProductList = ProductMap.TryGetValue(service, out var products) ? products : null;
		SelectedServiceFamily = serviceFamily;
		SelectedService = service;
		if (saveSelection) await SaveSelection();
		StateHasChanged();
	}
}
