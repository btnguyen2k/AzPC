using AzPC.Shared.Api;
using AzPC.Shared.Azure;
using Microsoft.AspNetCore.Mvc;

namespace AzPC.Api.Controllers;

public class AzureController : ApiBaseController
{
	/// <summary>
	/// Gets list of Azure regions
	/// </summary>
	[HttpGet("/api/azure/regions")]
	public ActionResult<ApiResp<IEnumerable<AzureRegion>>> GetExternalAuthProviders()
	{
		return ResponseOk(AzureGlobals.AzureRegions);
	}
}
