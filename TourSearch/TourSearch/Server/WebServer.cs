using System.Net;
using System.Text;
using TourSearch.Server;
using TourSearch.Infrastructure;

public class WebServer
{
    private readonly HttpListener _listener;
    private readonly SimpleRouter _router;
    private readonly string[] _prefixes;
    private volatile bool _shouldStop;

    public WebServer(string[] prefixes, SimpleRouter router)
    {
        if (prefixes == null || prefixes.Length == 0)
            throw new ArgumentException("At least one prefix is required.", nameof(prefixes));

        _router = router ?? throw new ArgumentNullException(nameof(router));
        _prefixes = prefixes;
        _listener = new HttpListener();
    }

    public void RequestStop()
    {
        _shouldStop = true;
        try
        {
            if (_listener.IsListening)
            {
                _listener.Stop();
            }
        }
        catch (ObjectDisposedException)
        {
                    }
        catch (Exception ex)
        {
            Logger.Warning($"Error stopping listener: {ex.Message}");
        }
    }

    public async Task RunAsync(CancellationToken token)
    {
                if (!TryStartListener())
        {
            throw new InvalidOperationException("Failed to start HTTP listener on any of the configured prefixes");
        }

        Logger.Info("Server started.");

        try
        {
            while (!_shouldStop && !token.IsCancellationRequested)
            {
                HttpListenerContext? context = null;

                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (HttpListenerException ex)
                {
                                                            if (_shouldStop || token.IsCancellationRequested)
                    {
                        Logger.Info($"HttpListener stopped gracefully: {ex.Message}");
                        break;
                    }

                    Logger.Error(ex, $"HttpListenerException in GetContextAsync (ErrorCode: {ex.ErrorCode})");
                    continue;
                }
                catch (ObjectDisposedException)
                {
                                        Logger.Info("HttpListener disposed, exiting loop");
                    break;
                }
                catch (InvalidOperationException ex)
                {
                                        if (_shouldStop || token.IsCancellationRequested)
                    {
                        break;
                    }
                    Logger.Error(ex, "InvalidOperationException in GetContextAsync");
                    continue;
                }

                if (context != null)
                {
                                        _ = ProcessRequestSafeAsync(context);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Fatal error in server main loop");
            throw;
        }
        finally
        {
            try
            {
                _listener.Close();
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error closing listener: {ex.Message}");
            }
            Logger.Info("Server stopped.");
        }
    }

    private bool TryStartListener()
    {
                foreach (var prefix in _prefixes)
        {
            try
            {
                _listener.Prefixes.Clear();
                _listener.Prefixes.Add(prefix);
                _listener.Start();
                Console.WriteLine($"[OK] Listening on {prefix}");
                return true;
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"[WARN] Cannot listen on {prefix}: {ex.Message}");
                Logger.Warning($"Cannot listen on {prefix}: {ex.Message}");
                
                try
                {
                    if (_listener.IsListening)
                        _listener.Stop();
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Unexpected error for {prefix}: {ex.Message}");
                Logger.Error(ex, $"Unexpected error starting listener on {prefix}");
            }
        }

                try
        {
            _listener.Prefixes.Clear();
            foreach (var prefix in _prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
            _listener.Start();
            Console.WriteLine($"[OK] Listening on {string.Join(", ", _prefixes)}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to start listener: {ex.Message}");
            Logger.Error(ex, "Failed to start HttpListener");
            return false;
        }
    }

    private async Task ProcessRequestSafeAsync(HttpListenerContext context)
    {
        try
        {
            await HandleRequestAsync(context);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unhandled error processing request");
            
            try
            {
                await WriteErrorResponseAsync(context.Response, 500, "Internal Server Error");
            }
            catch (Exception innerEx)
            {
                Logger.Warning($"Failed to send error response: {innerEx.Message}");
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var url = request.Url?.ToString() ?? "unknown";
        
        Logger.Info($"{request.HttpMethod} {url}");

        try
        {
            IRouteHandler? handler = null;
            
            try
            {
                handler = _router.Match(request);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in router.Match");
                await WriteErrorResponseAsync(context.Response, 500, "Internal Server Error");
                return;
            }

            if (handler != null)
            {
                try
                {
                    await handler.HandleAsync(context);
                }
                catch (HttpListenerException ex)
                {
                                        Logger.Warning($"Client disconnected: {ex.Message}");
                }
                catch (IOException ex)
                {
                                        Logger.Warning($"IO error (client disconnected?): {ex.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Error in handler {handler.GetType().Name}");
                    await WriteErrorResponseAsync(context.Response, 500, "Internal Server Error");
                }
            }
            else
            {
                try
                {
                    var notFound = new NotFoundHandler();
                    await notFound.HandleAsync(context);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error in NotFoundHandler");
                    await WriteErrorResponseAsync(context.Response, 404, "Not Found");
                }
            }
        }
        catch (ObjectDisposedException)
        {
                        Logger.Warning("Response already disposed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Fatal error in HandleRequestAsync");
            
            try
            {
                await WriteErrorResponseAsync(context.Response, 500, "Internal Server Error");
            }
            catch
            {
                            }
        }
    }

    private async Task WriteErrorResponseAsync(HttpListenerResponse response, int statusCode, string message)
    {
        try
        {
            if (response == null)
                return;

            response.StatusCode = statusCode;
            response.ContentType = "text/html; charset=utf-8";

            var html = $@"<!DOCTYPE html>
<html>
<head><title>Error {statusCode}</title></head>
<body style=""font-family: Arial, sans-serif; text-align: center; padding: 50px;"">
<h1>Error {statusCode}</h1>
<p>{System.Net.WebUtility.HtmlEncode(message)}</p>
<a href=""/"">Return to Home</a>
</body>
</html>";

            var buffer = Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }
        catch (ObjectDisposedException)
        {
                    }
        catch (HttpListenerException)
        {
                    }
        catch (Exception ex)
        {
            Logger.Warning($"Error writing error response: {ex.Message}");
        }
    }
}
