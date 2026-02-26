
using System.Text.RegularExpressions;
using TourSearch.Infrastructure;

namespace TourSearch.TemplateEngine;

public class SimpleTemplateEngine : ITemplateEngine
{
    private static readonly Regex PlaceholderRegex =
        new(@"\{\{([A-Za-z0-9_\.]+)\}\}", RegexOptions.Compiled);

        private static readonly Regex EachBlockRegex =
        new(@"\{\{#each\s+([A-Za-z0-9_]+)\s*\}\}(.*?)\{\{\/each\}\}",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex IfBlockRegex =
        new(@"\{\{#if\s+([A-Za-z0-9_]+)\s*\}\}(.*?)\{\{\/if\}\}",
            RegexOptions.Compiled | RegexOptions.Singleline);

    public string Render(string templateText, IDictionary<string, object?> model)
    {
        if (templateText == null)
            throw new ArgumentNullException(nameof(templateText));
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        try
        {
                        var withConditions = ProcessIfBlocks(templateText, model);

                        var withLoops = ProcessEachBlocks(withConditions, model);

                        var final = PlaceholderRegex.Replace(withLoops, m =>
            {
                try
                {
                    var key = m.Groups[1].Value;
                    return ResolveValue(key, model);
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Error resolving placeholder '{m.Value}': {ex.Message}");
                    return string.Empty;
                }
            });

            return final;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in template rendering");
            throw new TemplateRenderException("Failed to render template", ex);
        }
    }

    private string ProcessIfBlocks(string templateText, IDictionary<string, object?> model)
    {
        try
        {
            return IfBlockRegex.Replace(templateText, match =>
            {
                try
                {
                    var key = match.Groups[1].Value;
                    var content = match.Groups[2].Value;

                    if (model.TryGetValue(key, out var value))
                    {
                                                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                        {
                            return content;
                        }
                    }

                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Error processing if block: {ex.Message}");
                    return string.Empty;
                }
            });
        }
        catch (Exception ex)
        {
            Logger.Warning($"Error processing if blocks: {ex.Message}");
            return templateText;
        }
    }

    private string ProcessEachBlocks(string templateText, IDictionary<string, object?> model)
    {
        try
        {
            return EachBlockRegex.Replace(templateText, match =>
            {
                try
                {
                    var collectionKey = match.Groups[1].Value;
                    var innerTemplate = match.Groups[2].Value;

                    if (!model.TryGetValue(collectionKey, out var collectionObj) || collectionObj is null)
                        return string.Empty;

                    if (collectionObj is not System.Collections.IEnumerable enumerable)
                        return string.Empty;

                    var result = new System.Text.StringBuilder();

                    foreach (var item in enumerable)
                    {
                        try
                        {
                                                        var itemDict = ToDictionary(item);

                                                        var merged = new Dictionary<string, object?>(model);
                            foreach (var kv in itemDict)
                                merged[kv.Key] = kv.Value;

                            var rendered = PlaceholderRegex.Replace(innerTemplate, m =>
                            {
                                try
                                {
                                    var key = m.Groups[1].Value;
                                    return ResolveValue(key, merged);
                                }
                                catch
                                {
                                    return string.Empty;
                                }
                            });

                            result.Append(rendered);
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Error rendering item in each block: {ex.Message}");
                        }
                    }

                    return result.ToString();
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Error processing each block: {ex.Message}");
                    return string.Empty;
                }
            });
        }
        catch (Exception ex)
        {
            Logger.Warning($"Error processing each blocks: {ex.Message}");
            return templateText;
        }
    }

    private static string ResolveValue(string key, IDictionary<string, object?> model)
    {
        try
        {
                        if (key.Contains('.'))
            {
                var parts = key.Split('.');
                if (!model.TryGetValue(parts[0], out var obj) || obj is null)
                    return string.Empty;

                object? current = obj;
                for (int i = 1; i < parts.Length && current != null; i++)
                {
                    var type = current.GetType();
                    var prop = type.GetProperty(parts[i]);
                    if (prop == null) return string.Empty;
                    current = prop.GetValue(current);
                }

                return current?.ToString() ?? string.Empty;
            }

            if (!model.TryGetValue(key, out var value) || value is null)
                return string.Empty;

            return value.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Warning($"Error resolving value for key '{key}': {ex.Message}");
            return string.Empty;
        }
    }

    private static Dictionary<string, object?> ToDictionary(object item)
    {
        var dict = new Dictionary<string, object?>();
        
        if (item == null)
            return dict;

        try
        {
            var type = item.GetType();

            foreach (var prop in type.GetProperties())
            {
                try
                {
                    dict[prop.Name] = prop.GetValue(item);
                }
                catch
                {
                    dict[prop.Name] = null;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Error converting object to dictionary: {ex.Message}");
        }

        return dict;
    }
}

public class TemplateRenderException : Exception
{
    public TemplateRenderException(string message) : base(message) { }
    public TemplateRenderException(string message, Exception innerException) : base(message, innerException) { }
}
