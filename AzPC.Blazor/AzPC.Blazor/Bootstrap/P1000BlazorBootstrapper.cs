﻿using AzPC.Shared.Bootstrap;

namespace AzPC.Blazor.Bootstrap;

[Bootstrapper]
public class BlazorBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddRazorComponents()
			.AddInteractiveServerComponents()
			.AddInteractiveWebAssemblyComponents();
	}

	public static void DecorateApp(WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			app.UseWebAssemblyDebugging();
		}
		else
		{
			app.UseExceptionHandler("/Error", createScopeForErrors: true);
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseStaticFiles();
		//app.UseRouting();
		app.UseAntiforgery();

		app.MapRazorComponents<Components.App>()
			.AddInteractiveServerRenderMode()
			.AddInteractiveWebAssemblyRenderMode()
			.AddAdditionalAssemblies(typeof(Client._Imports).Assembly, typeof(App._Imports).Assembly);
	}
}
