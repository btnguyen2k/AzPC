using AzPC.Shared.Helpers;

var assemblies = AppDomain.CurrentDomain.GetAssemblies()
	.Append(typeof(AzPC.Api.Globals).Assembly) // AzPC.Api is used in Blazor Server, add its assembly to the list
	.Append(typeof(AzPC.Blazor.App.Globals).Assembly); // AzPC.Blazor.App is shared between Blazor Server and WebAssembly, add its assembly to the list
var appBuilder = WebApplication.CreateBuilder(args);
AzPC.Blazor.App.Globals.ApiBaseUrl = string.IsNullOrEmpty(appBuilder.Configuration[AzPC.Blazor.App.Globals.CONF_KEY_API_BASE_URL])
	? null
	: appBuilder.Configuration[AzPC.Blazor.App.Globals.CONF_KEY_API_BASE_URL];
var tasks = AzPC.Api.AppBootstrapper.Bootstrap(out var app, appBuilder, assemblies);
await Task.Run(() =>
{
	var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	AzPC.Api.Globals.Ready = true; // server is ready to handle requests
	logger.LogInformation("Background bootstrapping completed.");
});
app.Run();
