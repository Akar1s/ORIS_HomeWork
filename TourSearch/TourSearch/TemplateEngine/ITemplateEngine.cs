namespace TourSearch.TemplateEngine;

public interface ITemplateEngine
{
    string Render(string templateText, IDictionary<string, object?> model);
}
