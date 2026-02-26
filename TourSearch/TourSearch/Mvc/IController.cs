using System.Net;

namespace TourSearch.Mvc;

public interface IController
{
    Task<ControllerResult> HandleAsync(HttpListenerContext context);
}
