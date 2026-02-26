using System.Net;
using System.Text;

namespace TourSearch.Server;

public class NotFoundHandler : IRouteHandler
{
    public bool CanHandle(HttpListenerRequest request) => true;

    public async Task HandleAsync(HttpListenerContext context)
    {
        const string html = """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>Not found</title>
</head>
<body>
    <h1>404 — Not Found</h1>
</body>
</html>
""";

        var buffer = Encoding.UTF8.GetBytes(html);
        var response = context.Response;

        response.StatusCode = 404;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;

        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }
}
