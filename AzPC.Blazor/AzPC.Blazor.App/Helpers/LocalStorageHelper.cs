using AzPC.Shared.Helpers;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzPC.Blazor.App.Helpers;

public class LocalStorageEntryWithExpiry<T>
{
	[JsonPropertyName("expiry")]
	public DateTime Expiry { get; set; }

	[JsonPropertyName("value")]
	public string? StoredValue
	{
		get => _storedValue;
		set => SetStoredValue(value);
	}

	private void SetStoredValue(string? value)
	{
		_storedValue = value;
		_decompressedValue = Decompress(_storedValue);
		try
		{
			_value = _decompressedValue == null ? default : JsonHelper.FromJson<T>(_decompressedValue);
		}
		catch (JsonException)
		{
			_value = default;
		}
	}

	[JsonIgnore]
	private string? _storedValue;

	[JsonIgnore]
	private string? _decompressedValue;

	[JsonIgnore]
	private T? _value;

	private static string? Compress(string? value)
	{
		if (value == null) return null;
		try
		{
			using var storeStream = new MemoryStream();
			using var cstream = new DeflateStream(storeStream, CompressionLevel.SmallestSize);
			cstream.Write(Encoding.UTF8.GetBytes(value));
			cstream.Close();
			return Convert.ToBase64String(storeStream.ToArray());
		}
		catch (SystemException)
		{
			return null;
		}
	}

	private static string? Decompress(string? value)
	{
		if (value == null) return null;
		try
		{
			using var decompressedStream = new MemoryStream(Convert.FromBase64String(value));
			using var dstream = new DeflateStream(decompressedStream, CompressionMode.Decompress);
			using var reader = new StreamReader(dstream);
			return reader.ReadToEnd();
		}
		catch (SystemException)
		{
			return null;
		}
	}

	[JsonIgnore]
	public T? Value
	{
		get => _value;
		set => SetValue(value);
	}

	private void SetValue(T? value)
	{
		_value = value;
		_decompressedValue = JsonHelper.ToJson(value);
		_storedValue = Compress(_decompressedValue);
	}

	[JsonIgnore]
	public bool IsExpired => DateTime.Now > Expiry;
}

public class LocalStorageHelper(ILocalStorageService localStorageService, ILogger<LocalStorageHelper> logger)
{
	private static readonly ConcurrentDictionary<string, object?> CachedEntries = new();

	private static bool IsBrowser => OperatingSystem.IsBrowser();

	private static T? GetFromCache<T>(string key)
	{
		var value = CachedEntries.GetValueOrDefault(key);
		return value is T tValue ? tValue : default;
	}

	private static void PutToCache<T>(string key, T data)
	{
		CachedEntries.AddOrUpdate(key, data, (_, _) => data);
	}

	private static void RemoveFromCache(string key)
	{
		CachedEntries.TryRemove(key, out _);
	}

	private static void RemoveFromCache(IEnumerable<string> keys)
	{
		foreach (var key in keys)
		{
			RemoveFromCache(key);
		}
	}

	public async ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
	{
		try
		{
			if (IsBrowser)
			{
				var cachedValue = GetFromCache<T>(key);
				if (cachedValue != null)
				{
					return cachedValue;
				}
			}
			var localStorageStr = await localStorageService.GetItemAsStringAsync(key, cancellationToken);
			var localStorageEntry = !string.IsNullOrEmpty(localStorageStr)
				? await JsonHelper.FromJsonAsync<T>(localStorageStr, cancellationToken)
				: default;
			if (IsBrowser && localStorageEntry != null)
			{
				PutToCache(key, localStorageEntry);
			}
			return localStorageEntry;
		}
		catch (Exception e) when (e is JsonException || e is InvalidOperationException)
		{
			return default;
		}
	}

	public async ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = default)
	{
		if (IsBrowser)
		{
			RemoveFromCache(key);
		}
		try
		{
			var json = await JsonHelper.ToJsonAsync(data, cancellationToken);
			await localStorageService.SetItemAsStringAsync(key, json, cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			logger.LogWarning(ex, "Failed to set item in local storage, key: {key}", key);
		}
		if (IsBrowser)
		{
			PutToCache(key, data);
		}
	}

	public async ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default)
	{
		if (IsBrowser)
		{
			RemoveFromCache(key);
		}
		try
		{
			await localStorageService.RemoveItemAsync(key, cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			logger.LogWarning(ex, "Failed to remove item from local storage, key: {key}", key);
		}
	}

	public async ValueTask RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
	{
		if (IsBrowser)
		{
			RemoveFromCache(keys);
		}
		try
		{
			await localStorageService.RemoveItemsAsync(keys, cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			logger.LogWarning(ex, "Failed to remove items in local storage, key: {keys}", keys);
		}
	}
}
