namespace TourSearch.Mvc;

public class HtmlResult : ControllerResult
{
    public string Html { get; }

    public HtmlResult(string html, int statusCode = 200) : base(statusCode)
    {
        Html = html;
    }
}
