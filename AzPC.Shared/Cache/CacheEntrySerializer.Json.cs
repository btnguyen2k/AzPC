﻿using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace AzPC.Shared.Cache;

/// <summary>
/// Options for the <see cref="JsonCacheEntrySerializer"/>.
/// </summary>
public class JsonCacheEntrySerializerOptions
{
	public static JsonCacheEntrySerializerOptions DEFAULT { get; } = new JsonCacheEntrySerializerOptions();

	public JsonSerializerOptions? SerializerOptions { get; set; } = default;
}

/// <summary>
/// A built-in cache entry serializer that uses JSON serialization.
/// </summary>
public class JsonCacheEntrySerializer : ICacheEntrySerializer
{
	public static JsonCacheEntrySerializer DEFAULT { get; } = new JsonCacheEntrySerializer(JsonCacheEntrySerializerOptions.DEFAULT);

	private readonly JsonCacheEntrySerializerOptions Options = default!;

	public JsonCacheEntrySerializer() : this(JsonCacheEntrySerializerOptions.DEFAULT) { }

	public JsonCacheEntrySerializer(JsonCacheEntrySerializerOptions options)
	{
		this.Options = options ?? JsonCacheEntrySerializerOptions.DEFAULT;
	}

	/// <inheritdoc/>
	public byte[] Serialize<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, Options.SerializerOptions);

	/// <inheritdoc/>
	public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken token = default) => await Task.FromResult(JsonSerializer.SerializeToUtf8Bytes(value, Options.SerializerOptions));

	/// <inheritdoc/>
	public T? Deserialize<T>(byte[] bytes)
	{
		try
		{
			return JsonSerializer.Deserialize<T>(bytes, Options.SerializerOptions);
		}
		catch (Exception e) when (e is JsonException || e is IOException)
		{
			return default;
		}
	}

	/// <inheritdoc/>
	public async Task<T?> DeserializeAsync<T>(byte[] bytes, CancellationToken token = default)
	{
		try
		{
			using (var ms = new MemoryStream(bytes))
			{
				return await JsonSerializer.DeserializeAsync<T>(ms, Options.SerializerOptions, token);
			}
		}
		catch (Exception e) when (e is JsonException || e is IOException)
		{
			return default;
		}
	}
}
