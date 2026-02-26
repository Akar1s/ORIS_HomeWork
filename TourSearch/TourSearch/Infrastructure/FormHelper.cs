using System.Web;

namespace TourSearch.Infrastructure;

public static class FormHelper
{
    public static Dictionary<string, string> ParseForm(string body)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(body))
            return dict;

        var pairs = body.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0] ?? "");
            var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
            dict[key] = value;
        }

        return dict;
    }
}
