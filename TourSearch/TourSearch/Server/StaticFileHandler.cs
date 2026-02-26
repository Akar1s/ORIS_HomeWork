using System.Net;
using System.Text;
using TourSearch.Infrastructure;

namespace TourSearch.Server;

public class StaticFileHandler : IRouteHandler
{
    private readonly string _rootDirectory;
        
    private static readonly Dictionary<string, string> _mimeTypes = new()
    {
        [".html"] = "text/html; charset=utf-8",
        [".htm"] = "text/html; charset=utf-8",
        [".css"] = "text/css; charset=utf-8",
        [".js"] = "application/javascript; charset=utf-8",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".svg"] = "image/svg+xml",
        [".ico"] = "image/x-icon",
        [".woff"] = "font/woff",
        [".woff2"] = "font/woff2",
        [".ttf"] = "font/ttf",
        [".json"] = "application/json"
    };

    public StaticFileHandler(string rootDirectory)
    {
        _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        try
        {
            var path = request.Url?.AbsolutePath ?? "/";
            return path.StartsWith("/static/", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Logger.Warning($"Error in StaticFileHandler.CanHandle: {ex.Message}");
            return false;
        }
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        var response = context.Response;

        try
        {
            var request = context.Request;
            var path = request.Url?.AbsolutePath ?? "/static/";
            
            if (path.Length <= "/static/".Length)
            {
                await Write404Async(response);
                return;
            }

            var relativePath = path.Substring("/static/".Length);

                        if (string.IsNullOrWhiteSpace(relativePath))
            {
                await Write404Async(response);
                return;
            }

            var filePath = Path.Combine(_rootDirectory, relativePath);

                        string fullPath;
            try
            {
                fullPath = Path.GetFullPath(filePath);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Invalid file path: {filePath}, error: {ex.Message}");
                await Write404Async(response);
                return;
            }

            var rootFullPath = Path.GetFullPath(_rootDirectory);
            if (!fullPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Warning($"Path traversal attempt blocked: {path}");
                await Write404Async(response);
                return;
            }

            if (!File.Exists(fullPath))
            {
                await Write404Async(response);
                return;
            }

            var extension = Path.GetExtension(fullPath).ToLowerInvariant();
            if (!_mimeTypes.TryGetValue(extension, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            try
            {
                await using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                response.StatusCode = 200;
                response.ContentType = contentType;
                response.ContentLength64 = fs.Length;

                await fs.CopyToAsync(response.OutputStream);
            }
            catch (FileNotFoundException)
            {
                await Write404Async(response);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Warning($"Access denied to file: {fullPath}, error: {ex.Message}");
                await Write403Async(response);
                return;
            }
            catch (IOException ex)
            {
                Logger.Warning($"IO error reading file: {fullPath}, error: {ex.Message}");
                await Write500Async(response);
                return;
            }
        }
        catch (ObjectDisposedException)
        {
                    }
        catch (HttpListenerException)
        {
                    }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected error in StaticFileHandler");
            try
            {
                await Write500Async(response);
            }
            catch
            {
                            }
        }
        finally
        {
            try
            {
                response.Close();
            }
            catch
            {
                            }
        }
    }

    private static async Task Write404Async(HttpListenerResponse response)
    {
        try
        {
            const string message = "Static file not found";
            var buffer = Encoding.UTF8.GetBytes(message);

            response.StatusCode = 404;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch
        {
                    }
    }

    private static async Task Write403Async(HttpListenerResponse response)
    {
        try
        {
            const string message = "Access denied";
            var buffer = Encoding.UTF8.GetBytes(message);

            response.StatusCode = 403;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch
        {
                    }
    }

    private static async Task Write500Async(HttpListenerResponse response)
    {
        try
        {
            const string message = "Error while reading static file";
            var buffer = Encoding.UTF8.GetBytes(message);

            response.StatusCode = 500;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch
        {
                    }
    }
}
