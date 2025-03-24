using System.Text.Json;
using AzPC.Shared.Azure;

namespace AzPC.Api.Bootstrap;

sealed class AzureProductsInitializer(ILogger<AzureProductsInitializer> logger) : IHostedService
{
	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	private static readonly JsonSerializerOptions jsonSerializerOptions = new() { ReadCommentHandling = JsonCommentHandling.Skip };

	private async Task<List<AzureServiceFamily>> LoadAzureProductsData(CancellationToken cancellationToken)
	{
		var filename = "config/azure-products.json";
		logger.LogInformation("Loading Azure products data from {filename}...", filename);
		var jsonData = await File.ReadAllTextAsync(filename, cancellationToken);
		var serviceFamilyList = JsonSerializer.Deserialize<List<AzureServiceFamily>>(jsonData, jsonSerializerOptions);
		return serviceFamilyList ?? [];
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		var azureServiceFamylies = await LoadAzureProductsData(cancellationToken);
		logger.LogInformation("Azure Service families: {count}", azureServiceFamylies.Count);
		AzureGlobals.AzureServiceFamilies = azureServiceFamylies;
	}
}
