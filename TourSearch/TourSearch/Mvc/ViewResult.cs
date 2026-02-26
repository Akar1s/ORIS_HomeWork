namespace TourSearch.Mvc;

public class ViewResult : ControllerResult
{
    public string ViewPath { get; }
    public IDictionary<string, object?> Model { get; }

    public ViewResult(string viewPath, IDictionary<string, object?> model, int statusCode = 200)
        : base(statusCode)
    {
        ViewPath = viewPath;
        Model = model;
    }
}
