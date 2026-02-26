using System.Net;
using System.Text;
using System.Text.Json;
using TourSearch.Data;
using TourSearch.ViewModels;

namespace TourSearch.Server;

public class ToursApiHandler : IRouteHandler
{
    private readonly TourRepository _tourRepo;
    private readonly DestinationRepository _destinationRepo;
    private readonly TravelStyleRepository _styleRepo;

    public ToursApiHandler(
        TourRepository tourRepo,
        DestinationRepository destinationRepo,
        TravelStyleRepository styleRepo)
    {
        _tourRepo = tourRepo;
        _destinationRepo = destinationRepo;
        _styleRepo = styleRepo;
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        var path = request.Url?.AbsolutePath ?? "";
        return path.Equals("/api/tours", StringComparison.OrdinalIgnoreCase);
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (request.HttpMethod != "GET")
        {
            response.StatusCode = 405;
            response.Close();
            return;
        }

        try
        {
            var query = request.Url!.Query;             var styleId = GetIntQueryParam(request, "styleId");
            var destId = GetIntQueryParam(request, "destinationId");

            var tours = await _tourRepo.GetAllAsync();

            if (styleId.HasValue)
                tours = tours.Where(t => t.TravelStyleId == styleId.Value).ToList();

            if (destId.HasValue)
                tours = tours.Where(t => t.DestinationId == destId.Value).ToList();

            var destinationCache = new Dictionary<int, string>();
            var styleCache = new Dictionary<int, string>();

            var models = new List<TourApiModel>();

            foreach (var t in tours)
            {
                if (!destinationCache.TryGetValue(t.DestinationId, out var destName))
                {
                    var dest = await _destinationRepo.GetByIdAsync(t.DestinationId);
                    destName = dest?.Name ?? "";
                    destinationCache[t.DestinationId] = destName;
                }

                if (!styleCache.TryGetValue(t.TravelStyleId, out var styleName))
                {
                    var style = await _styleRepo.GetByIdAsync(t.TravelStyleId);
                    styleName = style?.Name ?? "";
                    styleCache[t.TravelStyleId] = styleName;
                }

                models.Add(new TourApiModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Days = t.DurationDays,
                    Price = t.BasePrice,
                    StartDate = t.StartDate?.ToString("yyyy-MM-dd"),
                    Destination = destName,
                    TravelStyle = styleName
                });
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(models, options); 
            var buffer = Encoding.UTF8.GetBytes(json);
            response.StatusCode = 200;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }
        catch
        {
            response.StatusCode = 500;
            response.ContentType = "application/json; charset=utf-8";
            var errorJson = "{\"error\":\"Server error\"}";
            var buffer = Encoding.UTF8.GetBytes(errorJson);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }
    }

    private static int? GetIntQueryParam(HttpListenerRequest request, string name)
    {
        var value = request.QueryString[name];
        if (int.TryParse(value, out var parsed) && parsed > 0)
            return parsed;
        return null;
    }
}
