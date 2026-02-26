using System.Net;
using TourSearch.Controllers;
using TourSearch.Data;
using TourSearch.Infrastructure;
using TourSearch.Mvc;

namespace TourSearch.Server;

public class AdminToursHandler : IRouteHandler
{
    private readonly AdminToursController _controller;
    private readonly ViewRenderer _viewRenderer;
    private readonly UserRepository _userRepo;

    public AdminToursHandler(
        AdminToursController controller,
        ViewRenderer viewRenderer,
        UserRepository userRepo)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
        _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        var path = request.Url?.AbsolutePath ?? "";
        return path.Equals("/admin/tours", StringComparison.OrdinalIgnoreCase)
               || path.Equals("/admin/tours/create", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/admin/tours/edit/", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("/admin/tours/delete/", StringComparison.OrdinalIgnoreCase);
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        try
        {
                        var user = await UserAuthHelper.GetCurrentUserAsync(context.Request, _userRepo);
            if (user == null || !string.Equals(user.Role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 302;
                context.Response.Headers["Location"] = "/account/login";
                context.Response.Close();
                return;
            }

            var request = context.Request;
            var path = request.Url?.AbsolutePath ?? "";
            var method = request.HttpMethod.ToUpperInvariant();

                        if (path.Equals("/admin/tours", StringComparison.OrdinalIgnoreCase))
            {
                if (method == "GET")
                    await HandleListAsync(context);
                else
                    MethodNotAllowed(context.Response);

                return;
            }

                        if (path.Equals("/admin/tours/create", StringComparison.OrdinalIgnoreCase))
            {
                if (method == "GET")
                    await HandleCreateGetAsync(context);
                else if (method == "POST")
                    await HandleCreatePostAsync(context);
                else
                    MethodNotAllowed(context.Response);

                return;
            }

                        if (path.StartsWith("/admin/tours/edit/", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(path.Split('/').Last(), out var id) || id <= 0)
                {
                    await ViewRenderer.WriteErrorAsync(context.Response, 400, "Некорректный ID тура.");
                    return;
                }

                if (method == "GET")
                    await HandleEditGetAsync(context, id);
                else if (method == "POST")
                    await HandleEditPostAsync(context, id);
                else
                    MethodNotAllowed(context.Response);

                return;
            }

                        if (path.StartsWith("/admin/tours/delete/", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(path.Split('/').Last(), out var id) || id <= 0)
                {
                    await ViewRenderer.WriteErrorAsync(context.Response, 400, "Некорректный ID тура.");
                    return;
                }

                if (method == "POST")
                {
                    await _controller.DeleteAsync(id);
                    context.Response.StatusCode = 302;
                    context.Response.Headers["Location"] = "/admin/tours";
                    context.Response.Close();
                }
                else
                {
                    MethodNotAllowed(context.Response);
                }

                return;
            }

                        await ViewRenderer.WriteErrorAsync(context.Response, 404, "Not Found");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in AdminToursHandler");
            await ViewRenderer.WriteErrorAsync(context.Response, 500, "Internal Server Error");
        }
    }

    private async Task HandleCreateGetAsync(HttpListenerContext context)
    {
        var response = context.Response;
        var result = await _controller.ShowCreateAsync();
        await _viewRenderer.RenderAsync(response, result);
    }

    private async Task HandleCreatePostAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var body = await reader.ReadToEndAsync();
        var form = FormHelper.ParseForm(body);

        var name = form.GetValueOrDefault("name", "");
        var description = form.GetValueOrDefault("description", "");
        var days = form.GetValueOrDefault("days", "");
        var price = form.GetValueOrDefault("price", "");
        var start = form.GetValueOrDefault("start_date", "");
        var dest = form.GetValueOrDefault("destination_id", "");
        var style = form.GetValueOrDefault("travel_style_id", "");
        var itinerary = form.GetValueOrDefault("itinerary", "");
        var whatsIncluded = form.GetValueOrDefault("whats_included", "");
        var newDestName = form.GetValueOrDefault("new_dest_name", "");
        var newDestCountry = form.GetValueOrDefault("new_dest_country", "");
        var newStyleName = form.GetValueOrDefault("new_style_name", "");
        var imageUrl = form.GetValueOrDefault("image_url", "");

        var (ok, error) = await _controller.SaveCreateAsync(
            name, description, days, price, start, dest, style, itinerary, whatsIncluded,
            newDestName, newDestCountry, newStyleName, imageUrl);

        if (!ok)
        {
            var result = await _controller.ShowCreateAsync(error);
            await _viewRenderer.RenderAsync(response, result);
            return;
        }

        response.StatusCode = 302;
        response.Headers["Location"] = "/admin/tours";
        response.Close();
    }

    private async Task HandleEditGetAsync(HttpListenerContext context, int id)
    {
        var response = context.Response;
        var result = await _controller.ShowEditAsync(id);
        if (result.StatusCode == 404)
        {
            await ViewRenderer.WriteErrorAsync(response, 404, "Тур не найден.");
            return;
        }
        await _viewRenderer.RenderAsync(response, result);
    }

    private async Task HandleEditPostAsync(HttpListenerContext context, int id)
    {
        var request = context.Request;
        var response = context.Response;

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var body = await reader.ReadToEndAsync();
        var form = FormHelper.ParseForm(body);

        var name = form.GetValueOrDefault("name", "");
        var description = form.GetValueOrDefault("description", "");
        var days = form.GetValueOrDefault("days", "");
        var price = form.GetValueOrDefault("price", "");
        var start = form.GetValueOrDefault("start_date", "");
        var dest = form.GetValueOrDefault("destination_id", "");
        var style = form.GetValueOrDefault("travel_style_id", "");
        var itinerary = form.GetValueOrDefault("itinerary", "");
        var whatsIncluded = form.GetValueOrDefault("whats_included", "");
        var newDestName = form.GetValueOrDefault("new_dest_name", "");
        var newDestCountry = form.GetValueOrDefault("new_dest_country", "");
        var newStyleName = form.GetValueOrDefault("new_style_name", "");
        var imageUrl = form.GetValueOrDefault("image_url", "");

        var (ok, error) = await _controller.SaveEditAsync(
            id, name, description, days, price, start, dest, style, itinerary, whatsIncluded,
            newDestName, newDestCountry, newStyleName, imageUrl);

        if (!ok)
        {
            var result = await _controller.ShowEditAsync(id, error);
            await _viewRenderer.RenderAsync(response, result);
            return;
        }

        response.StatusCode = 302;
        response.Headers["Location"] = "/admin/tours";
        response.Close();
    }

    private static void MethodNotAllowed(HttpListenerResponse response)
    {
        response.StatusCode = 405;
        response.Close();
    }

    private async Task HandleListAsync(HttpListenerContext context)
    {
        var response = context.Response;
        var result = await _controller.ListAsync();
        await _viewRenderer.RenderAsync(response, result);
    }
}
