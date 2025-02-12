﻿using AzPC.Blazor.App.Shared;
using AzPC.Shared.Api;
using AzPC.Shared.Identity;
using Microsoft.AspNetCore.Components;

namespace AzPC.Blazor.App.Pages;

public partial class RolesModify
{
	[Parameter]
	public string Id { get; set; } = string.Empty;

	private string AlertMessage { get; set; } = string.Empty;
	private string AlertType { get; set; } = "info";
	private bool HideUI { get; set; } = false;

	private string RoleName { get; set; } = string.Empty;
	private string RoleDescription { get; set; } = string.Empty;

	private RoleResp? SelectedRole { get; set; }
	private IEnumerable<ClaimResp>? ClaimList { get; set; }
	private IDictionary<string, bool> ClaimSelectedMap { get; set; } = new Dictionary<string, bool>();

	private async Task<RoleResp?> LoadRoleAsync(string id, string authToken)
	{
		HideUI = true;
		ShowAlert("info", "Loading role details. Please wait...");
		var result = await ApiClient.GetRoleAsync(id, authToken, ApiBaseUrl);
		if (result.Status == 200)
		{
			return result.Data;
		}
		ShowAlert("danger", result.Message!);
		return null;
	}

	private async Task<IEnumerable<ClaimResp>?> LoadAllClaimsAsync(string authToken)
	{
		HideUI = true;
		ShowAlert("info", "Loading claims. Please wait...");
		var result = await ApiClient.GetAllClaimsAsync(authToken, ApiBaseUrl);
		if (result.Status == 200)
		{
			return result.Data;
		}
		ShowAlert("danger", result.Message!);
		return null;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender)
		{
			HideUI = true;
			ShowAlert("info", "Loading role details. Please wait...");

			SelectedRole = await LoadRoleAsync(Id, await GetAuthTokenAsync());
			if (SelectedRole == null)
			{
				return;
			}
			RoleName = SelectedRole?.Name ?? string.Empty;
			RoleDescription = SelectedRole?.Description ?? string.Empty;

			ClaimList = await LoadAllClaimsAsync(await GetAuthTokenAsync());
			if (ClaimList == null)
			{
				return;
			}
			ClaimSelectedMap.Clear();
			if (SelectedRole?.Claims != null)
			{
				foreach (var claim in SelectedRole?.Claims!)
				{
					ClaimSelectedMap.Add($"{claim.ClaimType}:{claim.ClaimValue}", true);
				}
			}

			HideUI = false;
			CloseAlert();
		}
	}

	private void OnClaimChanged(string claimType, string claimValue)
	{
		var claim = $"{claimType}:{claimValue}";
		if (ClaimSelectedMap.ContainsKey(claim))
		{
			ClaimSelectedMap.Remove(claim);
		}
		else
		{
			ClaimSelectedMap.Add(claim, true);
		}
	}

	private void BtnClickCancel()
	{
		NavigationManager.NavigateTo(UIGlobals.ROUTE_IDENTITY_ROLES);
	}

	private void ShowAlert(string type, string message)
	{
		AlertType = type;
		AlertMessage = message;
		StateHasChanged();
	}

	private void CloseAlert()
	{
		AlertType = AlertMessage = string.Empty;
		StateHasChanged();
	}

	private async Task BtnClickSave()
	{
		ShowAlert("info", "Please wait...");
		if (string.IsNullOrWhiteSpace(RoleName))
		{
			ShowAlert("warning", "Role name is required.");
			return;
		}
		var req = new CreateOrUpdateRoleReq
		{
			Name = RoleName.Trim(),
			Description = RoleDescription.Trim(),
			Claims = ClaimSelectedMap.Keys.Select(k => new IdentityClaim { Type = k.Split(':')[0], Value = k.Split(':')[1], }),
		};
		var resp = await ApiClient.UpdateRoleAsync(Id, req, await GetAuthTokenAsync(), ApiBaseUrl);
		if (resp.Status != 200)
		{
			ShowAlert("danger", resp.Message!);
			return;
		}
		ShowAlert("success", "Role updated successfully. Navigating to roles list...");
		var passAlertMessage = $"Role '{req.Name}' updated successfully.";
		var passAlertType = "success";
		await Task.Delay(500);
		NavigationManager.NavigateTo($"{UIGlobals.ROUTE_IDENTITY_ROLES}?alertMessage={passAlertMessage}&alertType={passAlertType}");
	}
}
