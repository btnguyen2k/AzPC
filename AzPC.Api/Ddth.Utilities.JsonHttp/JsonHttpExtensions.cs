using System.Net;
using System.Text.Json;

namespace AzPC.Api.Ddth.Utilities.JsonHttp;

/// <summary>
/// Encapsulates a HTTP response where the response content is expected to be a JSON.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class JsonResponse<T>
{
	public HttpStatusCode StatusCode { get; set; }
	public T? Data { get; set; }
	public string? Message { get; set; }
}

/// <summary>
/// Utility class that provides extension methods for <see cref="HttpClient"/> and <see cref="HttpResponseMessage"/> to simplify JSON operations.
/// </summary>
public static class JsonHttpExtensions
{
	/// <summary>
	/// Convenience method to read data from a http response message as JSON.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="httpResponseMessage"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public static async Task<JsonResponse<T>> ReadFromJsonAsync<T>(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
	{
		return await httpResponseMessage.ReadFromJsonAsync<T>(JsonSerializerOptions.Default, cancellationToken);
	}

	/// <summary>
	/// Convenience method to read data from a http response message as JSON.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="httpResponseMessage"></param>
	/// <param name="jsonSerializerOptions"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public static async Task<JsonResponse<T>> ReadFromJsonAsync<T>(this HttpResponseMessage httpResponseMessage, JsonSerializerOptions? jsonSerializerOptions, CancellationToken cancellationToken = default)
	{
		try
		{
			var data = await httpResponseMessage.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancellationToken);
			if (data == null)
			{
				return new JsonResponse<T> { StatusCode = HttpStatusCode.InternalServerError, Message = "Null response from server." };
			}
			return new JsonResponse<T> { StatusCode = httpResponseMessage.StatusCode, Data = data };
		}
		catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException || ex is OperationCanceledException)
		{
			return new JsonResponse<T> { StatusCode = HttpStatusCode.InternalServerError, Message = ex.Message };
		}
	}
}
