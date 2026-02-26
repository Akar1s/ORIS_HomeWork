using System.Net;
using TourSearch.Data;
using TourSearch.Mvc;
using TourSearch.TemplateEngine;
using TourSearch.ViewModels;
using TourSearch.Infrastructure;

namespace TourSearch.Controllers;

public class HomeController : IController
{
    private readonly string _projectRoot;
    private readonly ITemplateEngine _templateEngine;
    private readonly TourRepository _tourRepo;
    private readonly DestinationRepository? _destRepo;
    private readonly TravelStyleRepository? _styleRepo;

    public HomeController(string projectRoot, ITemplateEngine templateEngine, TourRepository tourRepo,
        DestinationRepository? destRepo = null, TravelStyleRepository? styleRepo = null)
    {
        _projectRoot = projectRoot;
        _templateEngine = templateEngine;
        _tourRepo = tourRepo;
        _destRepo = destRepo;
        _styleRepo = styleRepo;
    }

    public async Task<ControllerResult> HandleAsync(HttpListenerContext context)
    {
        return await GetIndexAsync(false);
    }

    public async Task<ControllerResult> GetIndexAsync(bool isAdmin)
    {
        try
        {
            var viewPath = Path.Combine(_projectRoot, "Views", "Home", "Index.html");

            var toursFromDb = await _tourRepo.GetAllWithDetailsAsync();

            var tours = toursFromDb.Select(t => new TourViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Days = t.DurationDays,
                Price = t.BasePrice,
                Destination = t.DestinationName ?? "Unknown",
                TravelStyle = t.TravelStyleName ?? "Classic",
                StartDate = t.StartDate?.ToString("MMM d, yyyy") ?? "TBA"
            }).ToList();

                        var destinations = new List<object>();
            var travelStyles = new List<object>();

            if (_destRepo != null)
            {
                try
                {
                    var destList = await _destRepo.GetAllAsync();
                    destinations = destList.Select(d => new { Id = d.Id, Name = d.Name }).ToList<object>();
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Error loading destinations: {ex.Message}");
                }
            }

            if (_styleRepo != null)
            {
                try
                {
                    var styleList = await _styleRepo.GetAllAsync();
                    travelStyles = styleList.Select(s => new { Id = s.Id, Name = s.Name }).ToList<object>();
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Error loading travel styles: {ex.Message}");
                }
            }

            var model = new Dictionary<string, object?>
            {
                ["Title"] = "Search Tours",
                ["Tours"] = tours,
                ["TourCount"] = tours.Count.ToString(),
                ["Destinations"] = destinations,
                ["TravelStyles"] = travelStyles,
                ["IsAdmin"] = isAdmin ? "yes" : ""
            };

            return new ViewResult(viewPath, model, 200);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in HomeController.GetIndexAsync");
            return new HtmlResult("<h1>Error loading tours</h1><p>Please try again later.</p>", 500);
        }
    }
}
