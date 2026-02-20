using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace Azure.API.Utils;

public static partial class HtmlUtils
{
    [GeneratedRegex("<.*?>")]
    private static partial Regex HtmlTagRegex();

    /// <summary>
    /// Converts an HTML string to plain text by removing all HTML tags and decoding HTML entities.
    /// </summary>
    /// <param name="htmlString">The HTML string to convert.</param>
    /// <returns>The plain text representation of the HTML string.</returns>
    public static string GetPlainText(object? htmlString)
    {
        if (htmlString == null) return "";
        string html = htmlString.ToString() ?? "";

        html = html.Replace("</div>", " ").Replace("<br>", " ").Replace("</p>", " ");

        string plainText = HtmlTagRegex().Replace(html, String.Empty);

        plainText = WebUtility.HtmlDecode(plainText).Trim();

        return plainText;
    }
    
    /// <summary>
    /// Tries to extract a "displayName" property from a JSON string or object,
    /// returning it if found, otherwise returns the original string or "N/A" if null/empty.
    /// </summary>
    /// <param name="value">The JSON string or object to extract the displayName from.</param>
    /// <returns>The extracted displayName, the original string, or "N/A" if not found.</returns>
    public static string GetDisplayName(object? value)
    {
        if (value is null) return "N/A";

        try
        {
            if (value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("displayName", out var nameProp))
                {
                    return nameProp.GetString() ?? "N/A";
                }
                if (element.ValueKind == JsonValueKind.String)
                {
                    return element.GetString() ?? "N/A";
                }
            }

            var jsonString = value.ToString();
            if (string.IsNullOrWhiteSpace(jsonString)) return "N/A";
            
            if (!jsonString.Trim().StartsWith('{')) return jsonString;

            using var doc = JsonDocument.Parse(jsonString);
            if (doc.RootElement.TryGetProperty("displayName", out var name))
            {
                return name.GetString() ?? "N/A";
            }
        }
        catch
        {
            return value.ToString() ?? "N/A";
        }

        return "N/A";
    }
}
