namespace AzPC.Blazor.App.Services;

/// <summary>
/// Service that provides a simple state container for Blazor components.
/// </summary>
public sealed class StateContainer
{
	public event Action? OnChange;

	public void NotifyStateChanged() => OnChange?.Invoke();
}
