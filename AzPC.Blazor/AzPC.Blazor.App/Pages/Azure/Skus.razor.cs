using AzPC.Shared.Azure;
using Microsoft.AspNetCore.Components;

namespace AzPC.Blazor.App.Pages.Azure;

public partial class Skus
{
	private bool HideUI { get; set; } = false;

	private string AlertMessage { get; set; } = string.Empty;
	private string AlertType { get; set; } = "info";

	private string SelectedServiceFamily { get; set; } = string.Empty;
	private string SelectedService { get; set; } = string.Empty;
	private string SelectedProduct { get; set; } = string.Empty;
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
		SelectedServiceFamily = string.Empty;
		SelectedService = string.Empty;
		SelectedProduct = string.Empty;
		if (firstRender)
		{
			HideUI = true;
			ShowAlert("info", "Loading Azure products...");
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
		}
	}

	private void OnServiceFamilyChanged(ChangeEventArgs e)
	{
		var serviceFamily = e.Value?.ToString() ?? string.Empty;
		if (!SelectedServiceFamily.Equals(serviceFamily, StringComparison.InvariantCulture))
		{
			EventServiceFamilyChanged(serviceFamily);
		}
	}
	private void EventServiceFamilyChanged(string serviceFamily)
	{
		Console.WriteLine($"ServiceFamilyChanged: {serviceFamily}");
		SelectedServiceFamily = serviceFamily;
		SelectedService = string.Empty;
		SelectedProduct = string.Empty;
		ProductList = null;
		if (ServiceMap.TryGetValue(serviceFamily, out var value))
		{
			ServiceList = value;
		}
		else
		{
			ServiceList = null;
		}
		StateHasChanged();
	}

	private void OnServiceChanged(ChangeEventArgs e)
	{
		var service = e.Value?.ToString() ?? string.Empty;
		if (!SelectedService.Equals(service, StringComparison.InvariantCulture))
		{
			EventServiceChanged(service);
		}
	}
	private void EventServiceChanged(string service)
	{
		Console.WriteLine($"ServiceChanged: {service}");
		SelectedService = service;
		SelectedProduct = string.Empty;
		if (ProductMap.TryGetValue(service, out var value))
		{
			ProductList = value;
		}
		else
		{
			ProductList = null;
		}
	}
}
