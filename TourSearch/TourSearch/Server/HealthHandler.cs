using System.Net;
using System.Text;

namespace TourSearch.Server;

public class HealthHandler : IRouteHandler
{
    public bool CanHandle(HttpListenerRequest request)
    {
        return request.HttpMethod == "GET"
               && request.Url?.AbsolutePath == "/health";
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        const string json = """{"status":"ok"}""";

        var buffer = Encoding.UTF8.GetBytes(json);
        var response = context.Response;

        response.StatusCode = 200;
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = buffer.Length;

        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }
}
