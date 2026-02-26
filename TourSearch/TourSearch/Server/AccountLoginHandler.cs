using System.Net;
using System.Text;
using TourSearch.Controllers;
using TourSearch.Data;
using TourSearch.Infrastructure;
using TourSearch.Mvc;
using TourSearch.TemplateEngine;

namespace TourSearch.Server;

public class AccountLoginHandler : IRouteHandler
{
    private readonly AccountController _controller;
    private readonly ViewRenderer _viewRenderer;
    private readonly UserRepository _userRepo;

    public AccountLoginHandler(AccountController controller, ViewRenderer viewRenderer, UserRepository userRepo)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
        _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        var path = request.Url?.AbsolutePath ?? "";
        return path.Equals("/account/login", StringComparison.OrdinalIgnoreCase)
               || path.Equals("/account/logout", StringComparison.OrdinalIgnoreCase);
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var path = request.Url!.AbsolutePath;

        if (path.Equals("/account/logout", StringComparison.OrdinalIgnoreCase))
        {
            await HandleLogoutAsync(context);
            return;
        }

                var existingUser = await UserAuthHelper.GetCurrentUserAsync(request, _userRepo);
        if (existingUser != null)
        {
                        context.Response.StatusCode = 302;
            context.Response.Headers["Location"] = "/account/profile";
            context.Response.Close();
            return;
        }

        if (request.HttpMethod == "GET")
        {
            await RenderLogin(context, null);
        }
        else if (request.HttpMethod == "POST")
        {
            await HandlePostAsync(context);
        }
        else
        {
            context.Response.StatusCode = 405;
            context.Response.Close();
        }
    }

    private async Task HandlePostAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var body = await reader.ReadToEndAsync();
        var form = FormHelper.ParseForm(body);

        var email = form.TryGetValue("email", out var e) ? e : "";
        var password = form.TryGetValue("password", out var p) ? p : "";

        var ipAddress = request.RemoteEndPoint?.Address?.ToString() ?? "unknown";

        var (ok, error, user) = await _controller.LoginAsync(email, password, ipAddress);

        if (!ok || user == null)
        {
            await RenderLogin(context, error ?? "Login error.");
            return;
        }

        var token = UserAuthHelper.GenerateToken(user.Id);
        var cookie = new Cookie(UserAuthConfig.AuthCookieName, token)
        {
            HttpOnly = true,
            Path = "/"
        };
        response.SetCookie(cookie);

                response.StatusCode = 302;
        response.Headers["Location"] = "/";
        response.Close();
    }

    private async Task RenderLogin(HttpListenerContext context, string? error)
    {
        var result = await _controller.ShowLoginAsync(error);
        await _viewRenderer.RenderAsync(context.Response, result);
    }

    private static async Task HandleLogoutAsync(HttpListenerContext context)
    {
        var response = context.Response;

        var cookie = new Cookie(UserAuthConfig.AuthCookieName, "")
        {
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = "/"
        };
        response.SetCookie(cookie);

        response.StatusCode = 302;
        response.Headers["Location"] = "/";
        response.Close();
    }
}
