using System.Net;
using System.Text;
using TourSearch.Infrastructure;
using TourSearch.Mvc;
using TourSearch.TemplateEngine;

namespace TourSearch.Server;

public class ViewRenderer
{
    private readonly ITemplateEngine _templateEngine;

    public ViewRenderer(ITemplateEngine templateEngine)
    {
        _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
    }

    public async Task RenderAsync(HttpListenerResponse response, ControllerResult result)
    {
                if (result is HtmlResult htmlResult)
        {
            try
            {
                var buffer = Encoding.UTF8.GetBytes(htmlResult.Html);
                response.StatusCode = htmlResult.StatusCode;
                response.ContentType = "text/html; charset=utf-8";
                response.ContentLength64 = buffer.Length;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while rendering HTML result");
                await WriteErrorAsync(response, 500, "Internal Server Error");
            }
            finally
            {
                response.Close();
            }
            return;
        }

                if (result is not ViewResult viewResult)
        {
            await WriteErrorAsync(response, 500, "Unsupported result type");
            return;
        }

        if (string.IsNullOrWhiteSpace(viewResult.ViewPath) || !File.Exists(viewResult.ViewPath))
        {
            await WriteErrorAsync(response, 500, $"View not found: {viewResult.ViewPath}");
            return;
        }

        try
        {
            var templateText = await File.ReadAllTextAsync(viewResult.ViewPath, Encoding.UTF8);
            var html = _templateEngine.Render(templateText, viewResult.Model);

            var buffer = Encoding.UTF8.GetBytes(html);
            response.StatusCode = viewResult.StatusCode;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error while rendering view");
            await WriteErrorAsync(response, 500, "Internal Server Error");
        }
        finally
        {
            response.Close();
        }
    }

    public static async Task WriteErrorAsync(HttpListenerResponse response, int statusCode, string message)
    {
        try
        {
            response.StatusCode = statusCode;
            response.ContentType = "text/plain; charset=utf-8";

            var buffer = Encoding.UTF8.GetBytes(message);
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error while writing error response");
        }
        finally
        {
            try { response.Close(); } catch {  }
        }
    }
}