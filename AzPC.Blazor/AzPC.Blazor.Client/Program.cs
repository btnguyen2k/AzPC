using AzPC.Blazor.Client;
using AzPC.Shared.Helpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var assemblies = AppDomain.CurrentDomain.GetAssemblies()
	.Append(typeof(AzPC.Blazor.App.Globals).Assembly); // AzPC.Blazor.App is shared between Blazor Server and WebAssembly, add its assembly to the list
var wasmAppBuilder = WebAssemblyHostBuilder.CreateDefault(args);
AzPC.Blazor.App.Globals.ApiBaseUrl = string.IsNullOrEmpty(wasmAppBuilder.Configuration[AzPC.Blazor.App.Globals.CONF_KEY_API_BASE_URL])
	? wasmAppBuilder.HostEnvironment.BaseAddress
	: wasmAppBuilder.Configuration[AzPC.Blazor.App.Globals.CONF_KEY_API_BASE_URL];
var tasks = WasmAppBootstrapper.Bootstrap(out var app, wasmAppBuilder, assemblies);
await Task.Run(() =>
{
	var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	logger.LogInformation("Background bootstrapping completed.");
});

await app.RunAsync();
