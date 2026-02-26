using System.Net;
using TourSearch.Controllers;
using TourSearch.Infrastructure;
using TourSearch.Mvc;
using TourSearch.Data;

namespace TourSearch.Server;

public class SearchHandler : IRouteHandler
{
    private readonly HomeController _controller;
    private readonly ViewRenderer _viewRenderer;
    private readonly UserRepository? _userRepo;

    public SearchHandler(HomeController controller, ViewRenderer viewRenderer, UserRepository? userRepo = null)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
        _userRepo = userRepo;
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        return request.HttpMethod == "GET"
               && request.Url?.AbsolutePath == "/search";
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        try
        {
                        bool isAdmin = false;
            
            if (_userRepo != null)
            {
                try
                {
                    var authCookie = context.Request.Cookies[UserAuthConfig.AuthCookieName];
                    if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
                    {
                        if (UserAuthHelper.TryParseToken(authCookie.Value, out var userId))
                        {
                            var user = await _userRepo.GetByIdAsync(userId);
                            isAdmin = user?.Role == "admin";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Error checking admin status: {ex.Message}");
                }
            }

            var result = await _controller.GetIndexAsync(isAdmin);
            await _viewRenderer.RenderAsync(context.Response, result);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in SearchHandler");
            await ViewRenderer.WriteErrorAsync(context.Response, 500, "Internal Server Error");
        }
    }
}
