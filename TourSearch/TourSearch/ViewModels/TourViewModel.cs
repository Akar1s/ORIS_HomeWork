namespace TourSearch.ViewModels;

public class TourViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Days { get; set; }
    public decimal Price { get; set; }
    public string Destination { get; set; } = "";
    public string TravelStyle { get; set; } = "";
    public string StartDate { get; set; } = "";
}
