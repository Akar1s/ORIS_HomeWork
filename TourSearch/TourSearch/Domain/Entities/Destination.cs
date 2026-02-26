namespace TourSearch.Domain.Entities;

public class Destination
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
    public string? Description { get; set; }
}
