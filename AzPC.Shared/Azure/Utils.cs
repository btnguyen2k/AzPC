using System.Net.Http.Json;

namespace AzPC.Shared.Azure;

public static class AzureUtils
{
	public static async Task<IEnumerable<AzurePriceItem>> GetRetailPriceAsync(HttpClient httpClient, string apiUrl)
	{
		var result = new List<AzurePriceItem>();
		while (true)
		{
			var httpResp = await httpClient.GetAsync(apiUrl);
			var jsonResp = await httpResp.Content.ReadFromJsonAsync<RetailPriceResp>();
			result.AddRange(jsonResp.Items);
			if (string.IsNullOrEmpty(jsonResp.NextPageLink)) break;
			apiUrl = jsonResp.NextPageLink;
		}
		return result;
	}
}
