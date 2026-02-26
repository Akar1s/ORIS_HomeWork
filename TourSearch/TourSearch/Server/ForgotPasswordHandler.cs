using System.Net;
using System.Text;
using TourSearch.Controllers;
using TourSearch.Infrastructure;
using TourSearch.Mvc;

namespace TourSearch.Server;

public class ForgotPasswordHandler : IRouteHandler
{
    private readonly AccountController _controller;
    private readonly ViewRenderer _viewRenderer;

    public ForgotPasswordHandler(AccountController controller, ViewRenderer viewRenderer)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        var path = request.Url?.AbsolutePath ?? "";
        return path.Equals("/account/forgot-password", StringComparison.OrdinalIgnoreCase);
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;

            if (request.HttpMethod == "GET")
            {
                await RenderView(context, null, null);
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
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in ForgotPasswordHandler");
            Console.WriteLine($"❌ ERROR in ForgotPasswordHandler: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            await ViewRenderer.WriteErrorAsync(context.Response, 500, $"Internal Server Error: {ex.Message}");
        }
    }

    private async Task HandlePostAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;

            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var body = await reader.ReadToEndAsync();
            var form = FormHelper.ParseForm(body);

            var email = form.TryGetValue("email", out var e) ? e : "";

            Console.WriteLine($"📧 Password reset request for: {email}");

            var (ok, error) = await _controller.RequestPasswordResetAsync(email);

            if (!ok)
            {
                Console.WriteLine($"❌ Password reset failed: {error}");
                await RenderView(context, error, null);
                return;
            }

            Console.WriteLine($"✅ Password reset request processed");
            await RenderView(context, null, "Письмо с инструкциями отправлено на указанный email.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in HandlePostAsync");
            Console.WriteLine($"❌ ERROR in HandlePostAsync: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }

    private async Task RenderView(HttpListenerContext context, string? error, string? success)
    {
        try
        {
            var response = context.Response;

            var result = await _controller.ShowForgotPasswordAsync(error, success);
            await _viewRenderer.RenderAsync(response, result);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in RenderView");
            Console.WriteLine($"❌ ERROR in RenderView: {ex.Message}");
            throw;
        }
    }
}