﻿using AzPC.Shared.Api;
using AzPC.Shared.Bootstrap;
using Microsoft.AspNetCore.Mvc;

namespace AzPC.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that adds and maps controllers.
/// </summary>
[Bootstrapper]
public class ControllersBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddControllers()
			.ConfigureApiBehaviorOptions(options =>
			{
				// configure custom response for invalid model state (usually input validation failed)
				options.InvalidModelStateResponseFactory = context =>
				{
					var errors = context.ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
					return new BadRequestObjectResult(new ApiResp
					{
						Status = 400,
						Message = $"Bad request: {string.Join(", ", errors)}",
						Extras = errors
					});
				};
			});
	}

	public static void DecorateApp(WebApplication app)
	{
		app.MapControllers();
	}
}
