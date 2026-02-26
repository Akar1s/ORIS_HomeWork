using System.Net;
using System.Text;
using TourSearch.Controllers;
using TourSearch.Infrastructure;
using TourSearch.Mvc;
using TourSearch.TemplateEngine;

namespace TourSearch.Server;

public class AccountRegisterHandler : IRouteHandler
{
    private readonly AccountController _controller;
    private readonly ViewRenderer _viewRenderer;

    public AccountRegisterHandler(AccountController controller, ViewRenderer viewRenderer)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        var path = request.Url?.AbsolutePath ?? "";
        return path.Equals("/account/register", StringComparison.OrdinalIgnoreCase);
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        if (context.Request.HttpMethod == "GET")
        {
            await RenderRegister(context, null, null);
        }
        else if (context.Request.HttpMethod == "POST")
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
        var confirm = form.TryGetValue("confirm", out var c) ? c : "";

        var (ok, error) = await _controller.RegisterAsync(email, password, confirm);

        if (!ok)
        {
            await RenderRegister(context, error, null);
        }
        else
        {
            await RenderRegister(context, null, "Регистрация прошла успешно. Теперь вы можете войти.");
        }
    }

    private async Task RenderRegister(HttpListenerContext context, string? error, string? success)
    {
        var result = await _controller.ShowRegisterAsync(error, success);
        await _viewRenderer.RenderAsync(context.Response, result);
    }
}
