namespace TourSearch.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public int PersonsCount { get; set; }
    public string Status { get; set; } = "new";
    public DateTime CreatedAt { get; set; }

    public Tour? Tour { get; set; }
}
