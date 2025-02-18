namespace AzPC.Shared.Azure;

public class AzureGlobals
{
	public const string AzureLocationsFilename = "config/azure-locations.json";

	public const string AzurePricesEndpoint = "https://prices.azure.com/api/retail/prices?api-version=2023-01-01-preview";

	public static List<AzureRegion> AzureRegions { get; set; } = default!;
}
