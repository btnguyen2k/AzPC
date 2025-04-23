using AzPC.Blazor.App.Helpers;
using AzPC.Shared.Api;
using AzPC.Shared.Azure;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

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
	private bool ForceBtnGetPricingDisabled { get; set; } = false;
	private bool BtnGetPricingDisabled
	{
		get
		{
			return ForceBtnGetPricingDisabled || string.IsNullOrEmpty(SelectedProduct) || SelectedRegions.Length == 0;
		}
	}

	private struct AzureRegionGroup
	{
		public string Name { get; set; }
		public IEnumerable<AzureRegion> Regions { get; set; }
	}
	private IEnumerable<AzureRegionGroup> RegionGroups { get; set; } = [];

	private IEnumerable<AzureServiceFamily>? ServiceFamilyList { get; set; }
	private IEnumerable<AzureService>? ServiceList { get; set; }
	private IEnumerable<AzureProduct>? ProductList { get; set; }
	private Dictionary<string, IEnumerable<AzureService>> ServiceMap { get; set; } = [];
	private Dictionary<string, IEnumerable<AzureProduct>> ProductMap { get; set; } = [];
	private IEnumerable<AzureRegion>? RegionList { get; set; }
	private IEnumerable<AzurePricingPerRegion>? PricingList { get; set; }
	private string[] PricingRegions { get; set; } = [];

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
				RegionList = resultRegions.Data!.OrderBy(r => r.Name);
				RegionGroups = RegionList.GroupBy(r => $"{r.GeographyGroup}/{r.Geography}").Select(g => new AzureRegionGroup
				{
					Name = g.Key,
					Regions = g.OrderBy(r => r.Name),
				}).OrderBy(g => g.Name);
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
			var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var cachedProducts = await localStorage.GetItemAsync<LocalStorageEntryWithExpiry<List<AzureServiceFamily>>>("AzureProducts");
			var ok = true;
			if (cachedProducts != null && !cachedProducts.IsExpired && cachedProducts.Value != null)
			{
				ServiceFamilyList = cachedProducts.Value.OrderBy(sf => sf.Name);
			}
			else
			{
				var result = await ApiClient.GetAzureProductsAsync(await GetAuthTokenAsync(), ApiBaseUrl);
				if (result.Status == 200)
				{
					ServiceFamilyList = result.Data?.OrderBy(sf => sf.Name);
					await localStorage.SetItemAsync("AzureProducts", new LocalStorageEntryWithExpiry<List<AzureServiceFamily>>
					{
						Value = ServiceFamilyList?.ToList() ?? [],
						Expiry = DateTime.Now.AddDays(1)
					});
				}
				else
				{
					ShowAlert("danger", result.Message ?? "Unknown error");
					ok = false;
				}
			}
			if (ok)
			{
				HideUI = false;
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

			await LoadSelection();
			EventChangeServiceOrFamily(SelectedServiceFamily, SelectedService, false);
		}
	}

	private void OnRegionChanged(ChangeEventArgs e)
	{
		SelectedRegions = e.Value is IEnumerable<object> regionValues ? [.. regionValues.OfType<string>()] : [];
		if (SelectedRegions.Length > 5)
		{
			ShowAlert("warning", "Too many regions selected, please limit number of regions to 5.");
			SelectedRegions = SelectedRegions[..5];
			return;
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
		SelectedProduct = string.Empty;
		if (saveSelection) await SaveSelection();
		StateHasChanged();
	}

	private void OnProductChanged(ChangeEventArgs e)
	{
		SelectedProduct = e.Value?.ToString() ?? string.Empty;
	}

	private async void BtnGetPricingClicked()
	{
		if (string.IsNullOrEmpty(SelectedProduct) || SelectedRegions.Length == 0)
		{
			ShowAlert("danger", "Please select a product and at least one region.");
			return;
		}

		// HideUI = true;
		ForceBtnGetPricingDisabled = true;
		ShowAlert("info", "Loading Azure pricing...");

		var respPricing = await ApiClient.GetAzurePricingAsync(
			new AzurePricingReq
			{
				Products = [SelectedProduct],
				Regions = [.. SelectedRegions.Take(5)],
			},
			await GetAuthTokenAsync(),
			ApiBaseUrl
		);
		if (respPricing.Status != 200)
		{
			// HideUI = false;
			ForceBtnGetPricingDisabled = false;
			ShowAlert("danger", respPricing.Message!);
			return;
		}

		PricingList = respPricing.Data?.OrderBy(p => p.ProductName).ThenBy(p => p.SkuName).ThenBy(p => p.MeterName);
		PricingRegions = SelectedRegions;
		// HideUI = false;
		ForceBtnGetPricingDisabled = false;
		ShowAlert("success", $"Azure pricing loaded for regions {string.Join(", ", SelectedRegions)}.");
	}
}
