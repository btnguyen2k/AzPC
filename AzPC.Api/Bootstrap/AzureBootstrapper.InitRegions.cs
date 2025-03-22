using System.Text.Json;
using AzPC.Shared.Azure;

namespace AzPC.Api.Bootstrap;

sealed class AzureRegionsInitializer(ILogger<AzureRegionsInitializer> logger) : IHostedService
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
			})
			.ToList() ?? [];
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		var azureLocations = await LoadAzureLocationsData(cancellationToken);
		logger.LogInformation("Azure locations in file: {count}", azureLocations.Count);

		AzureGlobals.AzureRegions = [
			.. azureLocations.Where(x => x.RegionType.Equals("Physical", StringComparison.OrdinalIgnoreCase))
			.OrderBy(x => x.GeographyGroup).ThenBy(x => x.Geography).ThenBy(x => x.RegionType).ThenBy(x => x.Name)
		];
		logger.LogInformation("Physical Azure regions: {regions}", AzureGlobals.AzureRegions.Count);
	}
}
