using System.Drawing.Printing;
using AzPC.Shared.Api;
using AzPC.Shared.Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzPC.Api.Controllers;

public class AzureController(HttpClient httpClient) : ApiBaseController
{
	/// <summary>
	/// Gets list of Azure regions
	/// </summary>
	[HttpGet(IApiClient.API_ENDPOINT_AZURE_REGIONS)]
	public ActionResult<ApiResp<IEnumerable<AzureRegion>>> GetAzureRegions()
	{
		return ResponseOk(AzureGlobals.AzureRegions);
	}

	/// <summary>
	/// Gets list of Azure products
	/// </summary>
	[HttpGet(IApiClient.API_ENDPOINT_AZURE_PRODUCTS)]
	public ActionResult<ApiResp<IEnumerable<AzureServiceFamily>>> GetAzureProducts()
	{
		return ResponseOk(AzureGlobals.AzureServiceFamilies);
	}

	private readonly struct PSM
	{
		public string ProductId { get; }
		public string ProductName { get; }
		public string SkuId { get; }
		public string SkuName { get; }
		public string MeterId { get; }
		public string MeterName { get; }

		public PSM(AzurePriceItem item)
		{
			ProductId = item.ProductId;
			ProductName = item.ProductName;
			SkuId = item.SkuId;
			SkuName = item.SkuName;
			MeterId = item.MeterId;
			MeterName = item.MeterName;
		}

		public override bool Equals(object? obj)
		{
			if (obj is PSM other)
			{
				return ProductId == other.ProductId && SkuId == other.SkuId && MeterId == other.MeterId;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ProductId, SkuId, MeterId);
		}
	}

	/// <summary>
	/// Gets Azure pricing for the given products and regions.
	/// </summary>
	/// <param name="req"></param>
	/// <returns></returns>
	[HttpPost(IApiClient.API_ENDPOINT_AZURE_PRICING)]
	[Authorize]
	public ActionResult<ApiResp<IEnumerable<AzurePricingPerRegion>>> GetAzurePricing([FromBody] AzurePricingReq req)
	{
		if (req.Products == null || req.Products.Count == 0 || req.Regions == null || req.Regions.Count == 0)
		{
			return ResponseNoData(400, "Expected list of products and list of regions to check pricing for.");
		}

		// {product/sku/meter -> region-name -> pricing}
		var pricingPerRegionMap = new Dictionary<PSM, IDictionary<string, AzurePriceItem>>();
		var baseApiUrl = "https://prices.azure.com/api/retail/prices?$filter=productId eq '{product-id}'";
		foreach (var product in req.Products)
		{
			var apiUrl = baseApiUrl.Replace("{product-id}", product, StringComparison.Ordinal);
			var priceItems = AzureUtils.GetRetailPriceAsync(httpClient, apiUrl).Result;
			foreach (var item in priceItems)
			{
				if (!req.Regions.Contains(item.ArmRegionName)) continue;
				var key = new PSM(item);
				if (!pricingPerRegionMap.TryGetValue(key, out var regionPricing))
				{
					regionPricing = new Dictionary<string, AzurePriceItem>();
					pricingPerRegionMap[key] = regionPricing;
				}
				regionPricing[item.ArmRegionName] = item;
			}
		}

		var result = new List<AzurePricingPerRegion>();
		foreach (var (key, value) in pricingPerRegionMap)
		{
			var pricingPerRegion = new AzurePricingPerRegion
			{
				ProductId = key.ProductId,
				ProductName = key.ProductName,
				SkuId = key.SkuId,
				SkuName = key.SkuName,
				MeterId = key.MeterId,
				MeterName = key.MeterName,
				PricingPerRegion = new Dictionary<string, AzurePriceItem>()
			};
			foreach (var region in value.Keys)
			{
				pricingPerRegion.PricingPerRegion[region] = value[region];
			}
			result.Add(pricingPerRegion);
		}

		return ResponseOk(result);
	}
}
