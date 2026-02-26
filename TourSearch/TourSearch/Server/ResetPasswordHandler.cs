using System.Net;
using TourSearch.Controllers;
using TourSearch.Infrastructure;
using TourSearch.Mvc;

namespace TourSearch.Server;

public class ResetPasswordHandler : IRouteHandler
{
    private readonly AccountController _controller;
    private readonly ViewRenderer _viewRenderer;

    public ResetPasswordHandler(AccountController controller, ViewRenderer viewRenderer)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        var path = request.Url?.AbsolutePath ?? "";
        return path.Equals("/account/reset-password", StringComparison.OrdinalIgnoreCase);
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var query = request.Url?.Query ?? "";
            var token = ExtractToken(query);

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 400;
                await ViewRenderer.WriteErrorAsync(context.Response, 400, "Missing token");
                return;
            }

            if (request.HttpMethod == "GET")
            {
                await RenderView(context, token, null);
            }
            else if (request.HttpMethod == "POST")
            {
                await HandlePostAsync(context, token);
            }
            else
            {
                context.Response.StatusCode = 405;
                context.Response.Close();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in ResetPasswordHandler");
            await ViewRenderer.WriteErrorAsync(context.Response, 500, "Internal Server Error");
        }
    }

    private async Task HandlePostAsync(HttpListenerContext context, string token)
    {
        var request = context.Request;

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var body = await reader.ReadToEndAsync();
        var form = FormHelper.ParseForm(body);

        var newPassword = form.TryGetValue("password", out var p) ? p : "";
        var confirm = form.TryGetValue("confirm", out var c) ? c : "";

        var (ok, error) = await _controller.ResetPasswordAsync(token, newPassword, confirm);

        if (!ok)
        {
            await RenderView(context, token, error);
            return;
        }

                context.Response.StatusCode = 302;
        context.Response.Headers["Location"] = "/account/login?success=password-reset";
        context.Response.Close();
    }

    private async Task RenderView(HttpListenerContext context, string token, string? error)
    {
        var response = context.Response;
        var result = await _controller.ShowResetPasswordAsync(token, error);
        await _viewRenderer.RenderAsync(response, result);
    }

    private static string ExtractToken(string query)
    {
        if (string.IsNullOrEmpty(query) || !query.StartsWith("?"))
            return "";

        var pairs = query.Substring(1).Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && parts[0] == "token")
                return Uri.UnescapeDataString(parts[1]);
        }

        return "";
    }
}