using Microsoft.AspNetCore.Identity;

namespace AzPC.Shared.Identity;

public sealed class AzPCUser : IdentityUser
{
	public IEnumerable<AzPCRole>? Roles { get; set; } = default!;
	public IEnumerable<IdentityUserClaim<string>>? Claims { get; set; } = default!;

	public string? GivenName { get; set; } = default!;
	public string? FamilyName { get; set; } = default!;

	/// <summary>
	/// Touches the entity, updating the <see cref="ConcurrencyStamp"/> property.
	/// </summary>
	public void Touch() => ConcurrencyStamp = Guid.NewGuid().ToString();

	public override bool Equals(object? obj) => obj is AzPCUser other
		&& (ReferenceEquals(this, other) || Id.Equals(other.Id, Globals.StringComparison));

	public override int GetHashCode() => Id.GetHashCode(Globals.StringComparison);
}
