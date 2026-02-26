using System.Net;
using System.Text;
using TourSearch.Controllers;
using TourSearch.Infrastructure;
using TourSearch.Mvc;

namespace TourSearch.Server;

public class PlaceholderHandler : IRouteHandler
{
    private readonly PlaceholderController _controller;
    private readonly ViewRenderer _viewRenderer;

    public PlaceholderHandler(PlaceholderController controller, ViewRenderer viewRenderer)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        return request.HttpMethod == "GET"
               && request.Url?.AbsolutePath == "/placeholder";
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        try
        {
            var result = await _controller.HandleAsync(context);
            
            if (result is HtmlResult htmlResult)
            {
                var buffer = Encoding.UTF8.GetBytes(htmlResult.Html);
                context.Response.StatusCode = htmlResult.StatusCode;
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.Close();
            }
            else
            {
                await _viewRenderer.RenderAsync(context.Response, result);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in PlaceholderHandler");
            await ViewRenderer.WriteErrorAsync(context.Response, 500, "Internal Server Error");
        }
    }
}
