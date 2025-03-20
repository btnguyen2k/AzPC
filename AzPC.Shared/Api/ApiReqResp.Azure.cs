using System.Text.Json.Serialization;
using AzPC.Shared.Azure;

namespace AzPC.Shared.Api;

/// <summary>
/// Request to ask for pricing of Azure products.
/// </summary>
public struct AzurePricingReq
{
	[JsonPropertyName("products")]
	public List<string> Products { get; set; }

	[JsonPropertyName("regions")]
	public List<string> Regions { get; set; }
}

/// <summary>
/// Since once product has many SKUs, each SKU has many meters, pricing is grouped by {product/sku/meter}.
/// </summary>
public struct AzurePricingPerRegion
{
	[JsonPropertyName("product_id")]
	public string ProductId { get; set; }

	[JsonPropertyName("product_name")]
	public string ProductName { get; set; }

	[JsonPropertyName("sku_id")]
	public string SkuId { get; set; }

	[JsonPropertyName("sku_name")]
	public string SkuName { get; set; }

	[JsonPropertyName("meter_id")]
	public string MeterId { get; set; }

	[JsonPropertyName("meter_name")]
	public string MeterName { get; set; }

	/// <summary>
	/// Pricing for the given {product/sku/meter} in regions {region->price}.
	/// </summary>
	[JsonPropertyName("pricing_per_region")]
	public IDictionary<string, AzurePriceItem> PricingPerRegion { get; set; }
}

// /// <summary>
// /// Response structure for APIs that return pricing of Azure products.
// /// </summary>
// public struct AzurePricingResp
// {
// 	[JsonPropertyName("pricing")]
// 	public List<AzurePricingPerRegion> Pricing { get; set; }
// }
