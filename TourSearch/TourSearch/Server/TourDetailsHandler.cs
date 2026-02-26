using System.Net;
using TourSearch.Controllers;
using TourSearch.Infrastructure;
using TourSearch.Mvc;

namespace TourSearch.Server;

public class TourDetailsHandler : IRouteHandler
{
    private readonly ToursController _controller;
    private readonly ViewRenderer _viewRenderer;

    public TourDetailsHandler(ToursController controller, ViewRenderer viewRenderer)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        if (request.HttpMethod != "GET")
            return false;

        var path = request.Url?.AbsolutePath ?? "";
        if (!path.StartsWith("/tours/", StringComparison.OrdinalIgnoreCase))
            return false;

        var idPart = path["/tours/".Length..];
        return int.TryParse(idPart, out var id) && id > 0;
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        var response = context.Response;
        var path = context.Request.Url!.AbsolutePath;
        var idPart = path["/tours/".Length..];

        if (!int.TryParse(idPart, out var id) || id <= 0)
        {
            response.StatusCode = 404;
            response.ContentType = "text/plain; charset=utf-8";
            await response.OutputStream.WriteAsync("Tour not found"u8.ToArray());
            response.Close();
            return;
        }

        try
        {
            var result = await _controller.DetailsAsync(id);

            if (result.StatusCode == 404)
            {
                response.StatusCode = 404;
                response.ContentType = "text/plain; charset=utf-8";
                await response.OutputStream.WriteAsync("Tour not found"u8.ToArray());
                response.Close();
                return;
            }

            await _viewRenderer.RenderAsync(response, result);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in TourDetailsHandler");
            await ViewRenderer.WriteErrorAsync(response, 500, "Internal Server Error");
        }
    }
}