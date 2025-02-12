﻿using Microsoft.AspNetCore.Identity;

namespace AzPC.Shared.Identity;

public sealed class AzPCRole : IdentityRole
{
	public IEnumerable<IdentityRoleClaim<string>>? Claims { get; set; } = default!;

	public string? Description { get; set; }

	/// <summary>
	/// Touches the entity, updating the <see cref="ConcurrencyStamp"/> property.
	/// </summary>
	public void Touch() => ConcurrencyStamp = Guid.NewGuid().ToString();

	public override bool Equals(object? obj) => obj is AzPCRole other
		&& (ReferenceEquals(this, other) || Id.Equals(other.Id, Globals.StringComparison));

	public override int GetHashCode() => Id.GetHashCode(Globals.StringComparison);
}
