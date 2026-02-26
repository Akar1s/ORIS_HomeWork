
using System.Net;
using TourSearch.Infrastructure;

namespace TourSearch.Server;

public class LegacyRedirectHandler : IRouteHandler
{
    private static readonly Dictionary<string, string> Redirects = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/Login.html"] = "/account/login",
        ["/Register.html"] = "/account/register",
        ["/Index.html"] = "/",
        ["/Home.html"] = "/"
    };

    public bool CanHandle(HttpListenerRequest request)
    {
        var path = request.Url?.AbsolutePath ?? "";
        return Redirects.ContainsKey(path);
    }

    public Task HandleAsync(HttpListenerContext context)
    {
        try
        {
            var path = context.Request.Url!.AbsolutePath;
            var newLocation = Redirects[path];

            context.Response.StatusCode = 301;             context.Response.Headers["Location"] = newLocation;
            context.Response.Close();

            Logger.Info($"Redirected {path} → {newLocation}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in LegacyRedirectHandler");
            context.Response.StatusCode = 500;
            context.Response.Close();
        }

        return Task.CompletedTask;
    }
}
