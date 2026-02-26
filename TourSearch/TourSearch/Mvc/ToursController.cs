using System.Net;
using TourSearch.Data;
using TourSearch.Domain.Entities;
using TourSearch.Mvc;
using TourSearch.ViewModels;

namespace TourSearch.Controllers;

public class ToursController
{
    private readonly string _projectRoot;
    private readonly TourRepository _tourRepo;
    private readonly DestinationRepository _destinationRepo;
    private readonly TravelStyleRepository _styleRepo;

    public ToursController(
        string projectRoot,
        TourRepository tourRepo,
        DestinationRepository destinationRepo,
        TravelStyleRepository styleRepo)
    {
        _projectRoot = projectRoot;
        _tourRepo = tourRepo;
        _destinationRepo = destinationRepo;
        _styleRepo = styleRepo;
    }

    public async Task<ControllerResult> DetailsAsync(int id)
    {
        if (id <= 0)
            return new HtmlResult("<h1>Tour not found</h1>", 404);

        var tour = await _tourRepo.GetByIdWithDetailsAsync(id);
        if (tour is null)
            return new HtmlResult("<h1>Tour not found</h1>", 404);

                var itineraryItems = new List<object>();
        if (!string.IsNullOrEmpty(tour.Itinerary))
        {
            try
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<ItineraryItem>>(tour.Itinerary);
                if (items != null)
                {
                    itineraryItems = items.Select(i => (object)new 
                    { 
                        Day = i.dayTo.HasValue && i.dayTo > i.day 
                            ? $"{i.day} - {i.dayTo}" 
                            : i.day.ToString(),
                        Title = i.title ?? "",
                        Description = i.description ?? ""
                    }).ToList();
                }
            }
            catch { }
        }

                var includedItems = new List<string>();
        if (!string.IsNullOrEmpty(tour.WhatsIncluded))
        {
            try
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<string>>(tour.WhatsIncluded);
                if (items != null)
                {
                    includedItems = items;
                }
            }
            catch { }
        }

        var viewPath = Path.Combine(_projectRoot, "Views", "Tours", "Details.html");

        var model = new Dictionary<string, object?>
        {
            ["Title"] = tour.Name,
            ["Tour"] = new
            {
                Id = tour.Id,
                Name = tour.Name,
                Days = tour.DurationDays,
                Price = tour.BasePrice,
                StartDate = tour.StartDate?.ToString("dd.MM.yyyy") ?? "TBA",
                DestinationName = tour.DestinationName ?? "Unknown",
                TravelStyleName = tour.TravelStyleName ?? "Classic",
                Description = tour.Description ?? "",
                ImageUrl = tour.ImageUrl ?? "https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?w=1200"
            },
            ["Itinerary"] = itineraryItems,
            ["WhatsIncluded"] = includedItems.Select(i => new { Text = i }).ToList()
        };

        return new ViewResult(viewPath, model, 200);
    }
}

public class ItineraryItem
{
    public int day { get; set; }
    public int? dayTo { get; set; }
    public string? title { get; set; }
    public string? description { get; set; }
}
