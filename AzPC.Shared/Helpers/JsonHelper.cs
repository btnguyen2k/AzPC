using System.Text;
using System.Text.Json;

namespace AzPC.Shared.Helpers;

public class JsonHelper
{
	public static T? FromJson<T>(string json)
	{
		return JsonSerializer.Deserialize<T>(json);
	}

	public static async Task<T?> FromJsonAsync<T>(string json, CancellationToken cancellationToken = default)
	{
		return await Task.Run(async () =>
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
			return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
		});
	}

	public static string ToJson<T>(T data)
	{
		return JsonSerializer.Serialize(data);
	}

	public static async Task<string> ToJsonAsync<T>(T data, CancellationToken cancellationToken = default)
	{
		return await Task.Run(async () =>
		{
			using var valueStream = new MemoryStream();
			await JsonSerializer.SerializeAsync(valueStream, data, cancellationToken: cancellationToken);
			valueStream.Position = 0;
			using var reader = new StreamReader(valueStream, Encoding.UTF8);
			return await reader.ReadToEndAsync(cancellationToken);
		});
	}
}