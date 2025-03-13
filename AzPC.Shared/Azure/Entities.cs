using System.Text.Json.Serialization;

namespace AzPC.Shared.Azure;

public class AzureRegion
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	[JsonPropertyName("display_name")]
	public string DisplayName { get; set; } = default!;

	[JsonPropertyName("region_type")]
	public string RegionType { get; set; } = default!;

	[JsonPropertyName("geography")]
	public string Geography { get; set; } = default!;

	[JsonPropertyName("geography_group")]
	public string GeographyGroup { get; set; } = default!;

	[JsonPropertyName("physical_location")]
	public string PhysicalLocation { get; set; } = default!;

	[JsonPropertyName("latitude")]
	public string Latitude { get; set; } = default!;

	[JsonPropertyName("longitude")]
	public string Longitude { get; set; } = default!;

	[JsonPropertyName("paired_region")]
	public List<string> PairedRegion { get; set; } = [];

	public static AzureRegion UNKNOWN => new()
	{
		Name = "unknown",
		DisplayName = "Unknown",
		RegionType = "unknown",
		Geography = "unknown",
		GeographyGroup = "unknown",
		PhysicalLocation = "unknown",
		Latitude = "0",
		Longitude = "0",
		PairedRegion = [],
	};

	public AzureRegion Clone()
	{
		return new AzureRegion
		{
			Name = Name,
			DisplayName = DisplayName,
			RegionType = RegionType,
			Geography = Geography,
			GeographyGroup = GeographyGroup,
			PhysicalLocation = PhysicalLocation,
			Latitude = Latitude,
			Longitude = Longitude,
			PairedRegion = [.. PairedRegion],
		};
	}
}

public class AzureProduct
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = default!;

	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	// [JsonPropertyName("sku_id")]
	// public string SkuId { get; set; } = default!;

	[JsonPropertyName("sku_name")]
	public string SkuName { get; set; } = default!;

	// [JsonPropertyName("meter_id")]
	// public string MetterId { get; set; } = default!;

	[JsonPropertyName("meter_name")]
	public string MeterName { get; set; } = default!;
}

public class AzureService
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = default!;

	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	[JsonPropertyName("products")]
	public IDictionary<string, AzureProduct> Products { get; set; } = new Dictionary<string, AzureProduct>();

	// public AzureService AddProduct(AzureProduct product)
	// {
	// 	// Products[product.Id] = product;
	// 	Products[$"{product.Id}/{product.MeterName}"] = product;
	// 	return this;
	// }
}

public class AzureServiceFamily
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	[JsonPropertyName("services")]
	public IDictionary<string, AzureService> Services { get; set; } = new Dictionary<string, AzureService>();

	public AzureServiceFamily AddService(AzureService service)
	{
		Services[service.Id] = service;
		return this;
	}
}

// public class PriceItem
// {
// 	[JsonPropertyName("currencyCode")]
// 	public string CurrencyCode { get; set; } = default!;

// 	[JsonPropertyName("tierMinimumUnits")]
// 	public double TierMinimumUnits { get; set; }

// 	[JsonPropertyName("retailPrice")]
// 	public double RetailPrice { get; set; }

// 	[JsonPropertyName("unitPrice")]
// 	public double UnitPrice { get; set; }

// 	[JsonPropertyName("armRegionName")]
// 	public string ArmRegionName { get; set; } = default!;

// 	[JsonPropertyName("location")]
// 	public string Location { get; set; } = default!;

// 	[JsonPropertyName("effectiveStartDate")]
// 	public DateTimeOffset EffectiveStartDate { get; set; }

// 	[JsonPropertyName("meterId")]
// 	public string MeterId { get; set; } = default!;

// 	[JsonPropertyName("meterName")]
// 	public string MeterName { get; set; } = default!;

// 	[JsonPropertyName("productId")]
// 	public string ProductId { get; set; } = default!;

// 	[JsonPropertyName("skuId")]
// 	public string SkuId { get; set; } = default!;

// 	[JsonPropertyName("productName")]
// 	public string ProductName { get; set; } = default!;

// 	[JsonPropertyName("skuName")]
// 	public string SkuName { get; set; } = default!;

// 	[JsonPropertyName("serviceName")]
// 	public string ServiceName { get; set; } = default!;

// 	[JsonPropertyName("serviceId")]
// 	public string ServiceId { get; set; } = default!;

// 	[JsonPropertyName("serviceFamily")]
// 	public string ServiceFamily { get; set; } = default!;

// 	[JsonPropertyName("unitOfMeasure")]
// 	public string UnitOfMeasure { get; set; } = default!;

// 	[JsonPropertyName("type")]
// 	public string Type { get; set; } = default!;

// 	[JsonPropertyName("isPrimaryMeterRegion")]
// 	public bool IsPrimaryMeterRegion { get; set; }

// 	[JsonPropertyName("armSkuName")]
// 	public string ArmSkuName { get; set; } = default!;
// }

// public struct RetailPriceResp
// {
// 	[JsonPropertyName("BillingCurrency")]
// 	public string BillingCurrency { get; set; }

// 	[JsonPropertyName("CustomerEntityId")]
// 	public string CustomerEntityId { get; set; }

// 	[JsonPropertyName("CustomerEntityType")]
// 	public string CustomerEntityType { get; set; }

// 	[JsonPropertyName("NextPageLink")]
// 	public string NextPageLink { get; set; }

// 	[JsonPropertyName("Count")]
// 	public int Count { get; set; }

// 	[JsonPropertyName("Items")]
// 	public List<PriceItem> Items { get; set; }
// }
