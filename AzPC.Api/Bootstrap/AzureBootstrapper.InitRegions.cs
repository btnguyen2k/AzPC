using System.Text.Json;
using AzPC.Shared.Azure;
using AzPC.Api.Ddth.Utilities.JsonHttp;

namespace AzPC.Api.Bootstrap;

sealed class AzureRegionsInitializer(
	IServiceProvider serviceProvider,
	ILogger<AzureRegionsInitializer> logger) : IHostedService
{
	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	private static readonly JsonSerializerOptions jsonSerializerOptions = new() { ReadCommentHandling = JsonCommentHandling.Skip };

	private async Task<List<AzureRegion>> LoadAzureLocationsData(CancellationToken cancellationToken)
	{
		var filename = "config/azure-locations.json";
		logger.LogInformation("Loading Azure locations data from {filename}...", filename);
		var jsonData = await File.ReadAllTextAsync(filename, cancellationToken);
		var jsonDoc = JsonSerializer.Deserialize<JsonDocument>(jsonData, jsonSerializerOptions);
		return jsonDoc?.RootElement.EnumerateArray()
				.Select(x =>
				{
					var r = new AzureRegion
					{
						Name = x.GetProperty("name").GetString() ?? "<null>",
						DisplayName = x.GetProperty("displayName").GetString() ?? "<null>",
						RegionType = x.GetProperty("metadata").GetProperty("regionType").GetString() ?? "<null>",
						Geography = x.GetProperty("metadata").GetProperty("geography").GetString() ?? "<null>",
						GeographyGroup = x.GetProperty("metadata").GetProperty("geographyGroup").GetString() ?? "<null>",
						PhysicalLocation = x.GetProperty("metadata").GetProperty("physicalLocation").GetString() ?? "<null>",
						Latitude = x.GetProperty("metadata").GetProperty("latitude").GetString() ?? "<null>",
						Longitude = x.GetProperty("metadata").GetProperty("longitude").GetString() ?? "<null>",
					};
					var pairedRegionNode = x.GetProperty("metadata").GetProperty("pairedRegion");
					if (pairedRegionNode.ValueKind == JsonValueKind.Array)
					{
						r.PairedRegion = [.. pairedRegionNode.EnumerateArray().Select(y => y.GetString() ?? "<null>")];
					}
					return r;
				}).ToList() ?? [];
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		var azureLocations = await LoadAzureLocationsData(cancellationToken);
		logger.LogInformation("Azure Locations: {locations}", azureLocations.Count);

		using (var scope = serviceProvider.CreateScope())
		{
			var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
			var endpoint = AzureGlobals.AzurePricesEndpoint + "&$filter=serviceName eq 'Storage' and meterName eq 'E1 LRS Disk'";
			logger.LogInformation("Fetching Azure Regions data...{endpoint}", endpoint);
			var req = new HttpRequestMessage(HttpMethod.Get, endpoint)
			{
				Headers = { { "Accept", "application/json" } }
			};
			var resp = await httpClient.SendAsync(req, cancellationToken);
			var jsonResp = await resp.ReadFromJsonAsync<JsonDocument>(cancellationToken);

			AzureGlobals.AzureRegions = [];
			jsonResp.Data?.RootElement.GetProperty("Items").EnumerateArray().Select(x => x.GetProperty("armRegionName").GetString()!)
				.Distinct().ToList().ForEach(x =>
				{
					var region = azureLocations.FirstOrDefault(y => y.Name == x);
					if (region is null)
					{
						region = AzureRegion.UNKNOWN.Clone();
						region.Name = x;
						region.DisplayName = x;
					}
					AzureGlobals.AzureRegions.Add(region);
				});
			AzureGlobals.AzureRegions = [.. AzureGlobals.AzureRegions.OrderBy(x => x.GeographyGroup).ThenBy(x => x.Geography).ThenBy(x => x.RegionType).ThenBy(x => x.Name)];
			logger.LogInformation("Azure Regions: {regions}", AzureGlobals.AzureRegions.Count);
		}
	}
}
