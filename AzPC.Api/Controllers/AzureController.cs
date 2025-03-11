using AzPC.Shared.Api;
using AzPC.Shared.Azure;
using Microsoft.AspNetCore.Mvc;

namespace AzPC.Api.Controllers;

public class AzureController : ApiBaseController
{
	/// <summary>
	/// Gets list of Azure regions
	/// </summary>
	[HttpGet(IApiClient.API_ENDPOINT_AZURE_REGIONS)]
	public ActionResult<ApiResp<IEnumerable<AzureRegion>>> GetAzureRegions()
	{
		return ResponseOk(AzureGlobals.AzureRegions);
	}

	/// <summary>
	/// Gets list of Azure products
	/// </summary>
	[HttpGet(IApiClient.API_ENDPOINT_AZURE_PRODUCTS)]
	public ActionResult<ApiResp<IEnumerable<AzureServiceFamily>>> GetAzureProducts()
	{
		return ResponseOk(AzureGlobals.AzureServiceFamilies);
	}
}
