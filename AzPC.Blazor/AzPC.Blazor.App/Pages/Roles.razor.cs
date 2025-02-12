﻿using AzPC.Blazor.App.Shared;
using AzPC.Shared.Api;
using Microsoft.AspNetCore.WebUtilities;

namespace AzPC.Blazor.App.Pages;

public partial class Roles
{
	private CModal ModalDialogInfo { get; set; } = default!;
	private CModal ModalDialogDelete { get; set; } = default!;

	private bool HideUI { get; set; } = false;

	private string AlertMessage { get; set; } = string.Empty;
	private string AlertType { get; set; } = "info";

	private int RoleIndex = 0;
	private IEnumerable<RoleResp>? RoleList { get; set; }
	private IDictionary<string, RoleResp>? RoleMap { get; set; }
	private RoleResp? SelectedRole { get; set; }

	private void CloseAlert()
	{
		AlertMessage = string.Empty;
		StateHasChanged();
	}

	private void ShowAlert(string type, string message)
	{
		AlertType = type;
		AlertMessage = message;
		StateHasChanged();
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		RoleIndex = 0;
		if (firstRender)
		{
			HideUI = true;
			ShowAlert("info", "Loading roles...");
			var result = await ApiClient.GetAllRolesAsync(await GetAuthTokenAsync(), ApiBaseUrl);
			if (result.Status == 200)
			{
				HideUI = false;
				RoleList = result.Data?.OrderBy(r => r.Name);
				RoleMap = RoleList!.ToDictionary(role => role.Id);
				var queryParameters = QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query);
				var alertMessage = queryParameters.TryGetValue("alertMessage", out var alertMessageValue) ? alertMessageValue.ToString() : string.Empty;
				var alertType = queryParameters.TryGetValue("alertType", out var alertTypeValue) ? alertTypeValue.ToString() : string.Empty;
				if (!string.IsNullOrEmpty(alertMessage) && !string.IsNullOrEmpty(alertType))
				{
					ShowAlert(alertType!, alertMessage!);
				}
				else
				{
					CloseAlert();
				}
			}
			else
			{
				ShowAlert("danger", result.Message ?? "Unknown error");
			}
		}
	}

	private void BtnClickInfo(string roleId)
	{
		SelectedRole = RoleMap?[roleId];
		ModalDialogInfo.Open();
	}

	private void BtnClickModify(string roleId)
	{
		SelectedRole = RoleMap?[roleId];
		NavigationManager.NavigateTo(UIGlobals.ROUTE_IDENTITY_ROLES_MODIFY.Replace("{id}", roleId));
	}

	private void BtnClickDelete(string roleId)
	{
		SelectedRole = RoleMap?[roleId];
		ModalDialogDelete.Open();
	}

	private async void BtnClickDeleteConfirm()
	{
		ModalDialogDelete.Close();
		HideUI = true;
		ShowAlert("info", $"Deleting role '{SelectedRole?.Name}', please wait...");
		var result = await ApiClient.DeleteRoleAsync(SelectedRole?.Id ?? string.Empty, await GetAuthTokenAsync(), ApiBaseUrl);
		HideUI = false;
		if (result.Status == 200)
		{
			await OnAfterRenderAsync(true);
			ShowAlert("success", $"Role '{SelectedRole?.Name}' deleted successfully.");
		}
		else
		{
			ShowAlert("danger", result.Message ?? "Unknown error");
		}
	}

	private void BtnClickAdd()
	{
		NavigationManager.NavigateTo(UIGlobals.ROUTE_IDENTITY_ROLES_ADD);
	}
}
