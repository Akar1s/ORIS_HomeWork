namespace TourSearch.Domain.Entities;

public class Tour
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DurationDays { get; set; }
    public decimal BasePrice { get; set; }
    public DateTime? StartDate { get; set; }

    public int DestinationId { get; set; }
    public int TravelStyleId { get; set; }
    
    public string? Description { get; set; }
    public string? Itinerary { get; set; }      public string? WhatsIncluded { get; set; }      public string? ImageUrl { get; set; }  
        public Destination? Destination { get; set; }
    public TravelStyle? TravelStyle { get; set; }
}
