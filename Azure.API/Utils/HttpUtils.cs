using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace Azure.API.Utils;

public static class HttpUtils
{
    /// <summary>
    /// Handles the error response.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <exception cref="HttpRequestException">Thrown when the response is not successful.</exception>
    public static async Task HandleErrorResponse(HttpResponseMessage response)
    {
        HttpStatusCode status = response.StatusCode;
        string? reason = response.ReasonPhrase;
        string content = await response.Content.ReadAsStringAsync();
        string errorMessage = $"API Error: {status} - {reason}";

        try
        {
            using (JsonDocument doc = JsonDocument.Parse(content))
            {
                if (doc.RootElement.TryGetProperty("message", out var msgElement))
                {
                    errorMessage = msgElement.GetString() ?? errorMessage;
                }
                else if (doc.RootElement.TryGetProperty("value", out var valElement) && valElement.ValueKind == JsonValueKind.Object && valElement.TryGetProperty("message", out var valMsg))
                {
                    errorMessage = valMsg.GetString() ?? errorMessage;
                }
            }
        }
        catch
        {
            if (!string.IsNullOrEmpty(content) && content.Length < 500 && !content.TrimStart().StartsWith('<'))
            {
                errorMessage = $"API Error: {status} - {content}";
            }
        }

        throw new HttpRequestException(errorMessage);
    }
}