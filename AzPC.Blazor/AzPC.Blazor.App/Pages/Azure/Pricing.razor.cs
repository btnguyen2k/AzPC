using AzPC.Shared.Azure;
using Microsoft.AspNetCore.Components;

namespace AzPC.Blazor.App.Pages.Azure;

public partial class Pricing
{
	private bool HideUI { get; set; } = false;

	private string AlertMessage { get; set; } = string.Empty;
	private string AlertType { get; set; } = "info";

	private string SelectedServiceFamily { get; set; } = string.Empty;
	private string SelectedService { get; set; } = string.Empty;
	private string SelectedProduct { get; set; } = string.Empty;
	private string[] SelectedRegions { get; set; } = [];

	private IEnumerable<AzureServiceFamily>? ServiceFamilyList { get; set; }
	private IEnumerable<AzureService>? ServiceList { get; set; }
	private IEnumerable<AzureProduct>? ProductList { get; set; }
	private Dictionary<string, IEnumerable<AzureService>> ServiceMap { get; set; } = [];
	private Dictionary<string, IEnumerable<AzureProduct>> ProductMap { get; set; } = [];
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
		if (firstRender)
		{
			HideUI = true;

			ShowAlert("info", "Loading Azure regions...");
			SelectedRegions = [];
			var resultRegions = await ApiClient.GetAzureRegionsAsync(await GetAuthTokenAsync(), ApiBaseUrl);
			if (resultRegions.Status == 200)
			{
				RegionList = resultRegions.Data?.OrderBy(r => r.Name);
			}
			else
			{
				ShowAlert("danger", resultRegions.Message ?? "Unknown error");
				return;
			}

			ShowAlert("info", "Loading Azure products...");
			SelectedServiceFamily = string.Empty;
			SelectedService = string.Empty;
			SelectedProduct = string.Empty;
			var resultProducts = await ApiClient.GetAzureProductsAsync(await GetAuthTokenAsync(), ApiBaseUrl);
			if (resultProducts.Status == 200)
			{
				HideUI = false;
				ServiceFamilyList = resultProducts.Data?.OrderBy(sf => sf.Name);
				ServiceMap = [];
				ProductMap = [];
				foreach (var serviceFamily in ServiceFamilyList!)
				{
					var serviceList = serviceFamily.Services.Values.OrderBy(s => s.Name);
					ServiceMap.Add(serviceFamily.Name, serviceList);
					foreach (var service in serviceList)
					{
						var productList = service.Products.Values.DistinctBy(p => p.Id).OrderBy(p => p.Name).ThenBy(p => p.SkuName).ThenBy(p => p.MeterName);
						ProductMap.Add(service.Id, productList);
					}
				}
				CloseAlert();
			}
			else
			{
				ShowAlert("danger", resultProducts.Message ?? "Unknown error");
			}
		}
	}

	private void OnRegionChanged(ChangeEventArgs e)
	{
		SelectedRegions = e.Value is IEnumerable<object> regionValues ? [.. regionValues.OfType<string>()] : [];
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

	private void EventChangeServiceOrFamily(string serviceFamily, string service)
	{
		ServiceList = ServiceMap.TryGetValue(serviceFamily, out var services) ? services : null;
		ProductList = ProductMap.TryGetValue(service, out var products) ? products : null;
		SelectedServiceFamily = serviceFamily;
		SelectedService = service;
		StateHasChanged();
	}

	private void OnProductChanged(ChangeEventArgs e)
	{
		SelectedProduct = e.Value?.ToString() ?? string.Empty;
	}
}
