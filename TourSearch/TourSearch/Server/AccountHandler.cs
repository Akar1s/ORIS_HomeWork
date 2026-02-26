using System.Net;
using TourSearch.Data;
using TourSearch.Infrastructure;
using TourSearch.Mvc;

namespace TourSearch.Server;

public class AccountHandler : IRouteHandler
{
    private readonly UserRepository _userRepo;
    private readonly ViewRenderer _viewRenderer;
    private readonly string _projectRoot;

    public AccountHandler(UserRepository userRepo, ViewRenderer viewRenderer, string projectRoot)
    {
        _userRepo = userRepo;
        _viewRenderer = viewRenderer;
        _projectRoot = projectRoot;
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        var path = request.Url?.AbsolutePath ?? "";
        return path.Equals("/account", StringComparison.OrdinalIgnoreCase)
               || path.Equals("/account/profile", StringComparison.OrdinalIgnoreCase);
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        var user = await UserAuthHelper.GetCurrentUserAsync(context.Request, _userRepo);
        
        if (user == null)
        {
                        context.Response.StatusCode = 302;
            context.Response.Headers["Location"] = "/account/login";
            context.Response.Close();
            return;
        }

                var viewPath = Path.Combine(_projectRoot, "Views", "Account", "Profile.html");
        var initial = !string.IsNullOrEmpty(user.Email) ? user.Email[0].ToString().ToUpper() : "U";
        var isAdmin = string.Equals(user.Role, "admin", StringComparison.OrdinalIgnoreCase);
        
        var model = new Dictionary<string, object?>
        {
            ["User"] = new
            {
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt.ToString("dd.MM.yyyy"),
                Initial = initial
            },
            ["IsAdmin"] = isAdmin,
            ["IsUser"] = !isAdmin
        };

        var result = new ViewResult(viewPath, model, 200);
        await _viewRenderer.RenderAsync(context.Response, result);
    }
}
