using System.Net;

namespace TourSearch.Server;

public class SimpleRouter
{
    private readonly List<IRouteHandler> _handlers = new();

    public void RegisterHandler(IRouteHandler handler)
    {
        _handlers.Add(handler);
    }

    public IRouteHandler? Match(HttpListenerRequest request)
    {
        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(request))
                return handler;
        }

        return null;
    }
}
