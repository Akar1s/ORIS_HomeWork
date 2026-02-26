using System.Net;
using TourSearch.Controllers;
using TourSearch.Infrastructure;
using TourSearch.Mvc;

namespace TourSearch.Server;

public class LandingHandler : IRouteHandler
{
    private readonly LandingController _controller;
    private readonly ViewRenderer _viewRenderer;

    public LandingHandler(LandingController controller, ViewRenderer viewRenderer)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        return request.HttpMethod == "GET"
               && request.Url?.AbsolutePath == "/";
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        try
        {
            var result = await _controller.HandleAsync(context);
            await _viewRenderer.RenderAsync(context.Response, result);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in LandingHandler");
            await ViewRenderer.WriteErrorAsync(context.Response, 500, "Internal Server Error");
        }
    }
}
