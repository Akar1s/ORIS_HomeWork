using TourSearch.Data;
using TourSearch.Domain.Entities;
using TourSearch.Mvc;
using TourSearch.ViewModels;

namespace TourSearch.Controllers;

public class AdminToursController
{
    private readonly string _projectRoot;
    private readonly TourRepository _tourRepo;
    private readonly DestinationRepository? _destRepo;
    private readonly TravelStyleRepository? _styleRepo;

    public AdminToursController(string projectRoot, TourRepository tourRepo, 
        DestinationRepository? destRepo = null, TravelStyleRepository? styleRepo = null)
    {
        _projectRoot = projectRoot;
        _tourRepo = tourRepo;
        _destRepo = destRepo;
        _styleRepo = styleRepo;
    }

    private string ListViewPath =>
        Path.Combine(_projectRoot, "Views", "Admin", "Tours", "List.html");

    private string EditViewPath =>
        Path.Combine(_projectRoot, "Views", "Admin", "Tours", "Edit.html");

    public async Task<ControllerResult> ShowCreateAsync(string? error = null)
    {
        var destinations = await GetDestinationsAsync();
        var travelStyles = await GetTravelStylesAsync();

        var model = new Dictionary<string, object?>
        {
            ["PageTitle"] = "Create Tour",
            ["FormAction"] = "/admin/tours/create",
            ["Tour"] = new
            {
                Name = "",
                Description = "",
                Days = 7,
                Price = 0,
                StartDate = "",
                DestinationId = 0,
                TravelStyleId = 0,
                Itinerary = "",
                WhatsIncluded = ""
            },
            ["Destinations"] = destinations,
            ["TravelStyles"] = travelStyles,
            ["ErrorMessage"] = error ?? ""
        };

        return new ViewResult(EditViewPath, model, 200);
    }

    public async Task<ControllerResult> ShowEditAsync(int id, string? error = null)
    {
        var tour = await _tourRepo.GetByIdAsync(id);
        if (tour == null)
        {
            return new HtmlResult("<h1>Tour not found</h1>", 404);
        }

        var destinations = await GetDestinationsAsync(tour.DestinationId);
        var travelStyles = await GetTravelStylesAsync(tour.TravelStyleId);

        var model = new Dictionary<string, object?>
        {
            ["PageTitle"] = "Edit Tour",
            ["FormAction"] = $"/admin/tours/edit/{tour.Id}",
            ["Tour"] = new
            {
                Id = tour.Id,
                Name = tour.Name,
                Description = tour.Description ?? "",
                Days = tour.DurationDays,
                Price = tour.BasePrice,
                StartDate = tour.StartDate?.ToString("yyyy-MM-dd") ?? "",
                DestinationId = tour.DestinationId,
                TravelStyleId = tour.TravelStyleId,
                Itinerary = tour.Itinerary ?? "",
                WhatsIncluded = tour.WhatsIncluded ?? "",
                ImageUrl = tour.ImageUrl ?? ""
            },
            ["Destinations"] = destinations,
            ["TravelStyles"] = travelStyles,
            ["ErrorMessage"] = error ?? ""
        };

        return new ViewResult(EditViewPath, model, 200);
    }

    private async Task<List<object>> GetDestinationsAsync(int? selectedId = null)
    {
        var result = new List<object>();
        if (_destRepo != null)
        {
            try
            {
                var destinations = await _destRepo.GetAllAsync();
                result = destinations.Select(d => (object)new 
                { 
                    Id = d.Id, 
                    Name = d.Name,
                    Country = d.Country,
                    Selected = d.Id == selectedId
                }).ToList();
            }
            catch { }
        }
        return result;
    }

    private async Task<List<object>> GetTravelStylesAsync(int? selectedId = null)
    {
        var result = new List<object>();
        if (_styleRepo != null)
        {
            try
            {
                var styles = await _styleRepo.GetAllAsync();
                result = styles.Select(s => (object)new 
                { 
                    Id = s.Id, 
                    Name = s.Name,
                    Selected = s.Id == selectedId
                }).ToList();
            }
            catch { }
        }
        return result;
    }

    public async Task<(bool Ok, string? Error)> SaveCreateAsync(
        string name, string description, string daysStr, string priceStr, string startStr,
        string destStr, string styleStr, string itinerary, string whatsIncluded,
        string? newDestName = null, string? newDestCountry = null, string? newStyleName = null,
        string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            return (false, "Invalid tour name.");

        if (!int.TryParse(daysStr, out var days) || days < 1 || days > 60)
            return (false, "Invalid duration.");

        if (!decimal.TryParse(priceStr, out var price) || price < 0)
            return (false, "Invalid price.");

        DateTime? startDate = null;
        if (!string.IsNullOrWhiteSpace(startStr))
        {
            if (!DateTime.TryParse(startStr, out var parsed))
                return (false, "Invalid start date.");
            startDate = parsed;
        }

        int destId;
                if (destStr == "new" && _destRepo != null)
        {
            if (string.IsNullOrWhiteSpace(newDestName) || string.IsNullOrWhiteSpace(newDestCountry))
                return (false, "Please provide destination name and country.");
            
            var newDest = new Destination
            {
                Name = newDestName.Trim(),
                Country = newDestCountry.Trim()
            };
            destId = await _destRepo.CreateAsync(newDest);
        }
        else if (!int.TryParse(destStr, out destId) || destId <= 0)
        {
            return (false, "Please select a destination.");
        }

        int styleId;
                if (styleStr == "new" && _styleRepo != null)
        {
            if (string.IsNullOrWhiteSpace(newStyleName))
                return (false, "Please provide travel style name.");
            
            var newStyle = new TravelStyle
            {
                Name = newStyleName.Trim()
            };
            styleId = await _styleRepo.CreateAsync(newStyle);
        }
        else if (!int.TryParse(styleStr, out styleId) || styleId <= 0)
        {
            return (false, "Please select a travel style.");
        }

        var tour = new Tour
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            DurationDays = days,
            BasePrice = price,
            StartDate = startDate,
            DestinationId = destId,
            TravelStyleId = styleId,
            Itinerary = string.IsNullOrWhiteSpace(itinerary) ? null : itinerary,
            WhatsIncluded = string.IsNullOrWhiteSpace(whatsIncluded) ? null : whatsIncluded,
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim()
        };

        await _tourRepo.CreateAsync(tour);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> SaveEditAsync(
        int id,
        string name, string description, string daysStr, string priceStr, string startStr,
        string destStr, string styleStr, string itinerary, string whatsIncluded,
        string? newDestName = null, string? newDestCountry = null, string? newStyleName = null,
        string? imageUrl = null)
    {
        var existing = await _tourRepo.GetByIdAsync(id);
        if (existing == null)
            return (false, "Tour not found.");

        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            return (false, "Invalid tour name.");

        if (!int.TryParse(daysStr, out var days) || days < 1 || days > 60)
            return (false, "Invalid duration.");

        if (!decimal.TryParse(priceStr, out var price) || price < 0)
            return (false, "Invalid price.");

        DateTime? startDate = null;
        if (!string.IsNullOrWhiteSpace(startStr))
        {
            if (!DateTime.TryParse(startStr, out var parsed))
                return (false, "Invalid start date.");
            startDate = parsed;
        }

        int destId;
        if (destStr == "new" && _destRepo != null)
        {
            if (string.IsNullOrWhiteSpace(newDestName) || string.IsNullOrWhiteSpace(newDestCountry))
                return (false, "Please provide destination name and country.");
            
            var newDest = new Destination
            {
                Name = newDestName.Trim(),
                Country = newDestCountry.Trim()
            };
            destId = await _destRepo.CreateAsync(newDest);
        }
        else if (!int.TryParse(destStr, out destId) || destId <= 0)
        {
            return (false, "Please select a destination.");
        }

        int styleId;
        if (styleStr == "new" && _styleRepo != null)
        {
            if (string.IsNullOrWhiteSpace(newStyleName))
                return (false, "Please provide travel style name.");
            
            var newStyle = new TravelStyle
            {
                Name = newStyleName.Trim()
            };
            styleId = await _styleRepo.CreateAsync(newStyle);
        }
        else if (!int.TryParse(styleStr, out styleId) || styleId <= 0)
        {
            return (false, "Please select a travel style.");
        }

        existing.Name = name.Trim();
        existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        existing.DurationDays = days;
        existing.BasePrice = price;
        existing.StartDate = startDate;
        existing.DestinationId = destId;
        existing.TravelStyleId = styleId;
        existing.Itinerary = string.IsNullOrWhiteSpace(itinerary) ? null : itinerary;
        existing.WhatsIncluded = string.IsNullOrWhiteSpace(whatsIncluded) ? null : whatsIncluded;
        existing.ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();

        await _tourRepo.UpdateAsync(existing);
        return (true, null);
    }

    public async Task DeleteAsync(int id)
    {
        await _tourRepo.DeleteAsync(id);
    }

    public async Task<ControllerResult> ListAsync()
    {
        var toursFromDb = await _tourRepo.GetAllWithDetailsAsync();

        var tours = toursFromDb.Select(t => new TourViewModel
        {
            Id = t.Id,
            Name = t.Name,
            Days = t.DurationDays,
            Price = t.BasePrice,
            Destination = t.DestinationName ?? "Unknown",
            TravelStyle = t.TravelStyleName ?? "Classic"
        }).ToList();

        var model = new Dictionary<string, object?>
        {
            ["Tours"] = tours
        };

        return new ViewResult(ListViewPath, model, 200);
    }
}
