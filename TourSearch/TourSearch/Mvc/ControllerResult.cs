namespace TourSearch.Mvc;

public abstract class ControllerResult
{
    public int StatusCode { get; }

    protected ControllerResult(int statusCode)
    {
        StatusCode = statusCode;
    }
}
