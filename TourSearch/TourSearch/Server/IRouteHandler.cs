using System.Net;

namespace TourSearch.Server;

public interface IRouteHandler
{
    bool CanHandle(HttpListenerRequest request);

    Task HandleAsync(HttpListenerContext context);
}
