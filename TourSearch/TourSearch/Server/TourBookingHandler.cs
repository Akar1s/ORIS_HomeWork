using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TourSearch.Data;
using TourSearch.Domain.Entities;
using TourSearch.Infrastructure;

namespace TourSearch.Server;

public class TourBookingHandler : IRouteHandler
{
    private readonly BookingRepository _bookingRepo;
    private readonly TourRepository _tourRepo;

    public TourBookingHandler(BookingRepository bookingRepo, TourRepository tourRepo)
    {
        _bookingRepo = bookingRepo ?? throw new ArgumentNullException(nameof(bookingRepo));
        _tourRepo = tourRepo ?? throw new ArgumentNullException(nameof(tourRepo));
    }

    public bool CanHandle(HttpListenerRequest request)
    {
        if (request.HttpMethod != "POST")
            return false;

        var path = request.Url?.AbsolutePath ?? "";
                if (!path.StartsWith("/tours/", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!path.EndsWith("/book", StringComparison.OrdinalIgnoreCase))
            return false;

        var withoutPrefix = path["/tours/".Length..];           var idPart = withoutPrefix[..withoutPrefix.IndexOf('/')];

        return int.TryParse(idPart, out var id) && id > 0;
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            var path = request.Url!.AbsolutePath;
            var withoutPrefix = path["/tours/".Length..];               var idPart = withoutPrefix[..withoutPrefix.IndexOf('/')];

            if (!int.TryParse(idPart, out var tourId) || tourId <= 0)
            {
                await WriteBadRequest(response, "Invalid tour id.");
                return;
            }

                        var tour = await _tourRepo.GetByIdAsync(tourId);
            if (tour is null)
            {
                await WriteNotFound(response, "Tour not found.");
                return;
            }

                        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var body = await reader.ReadToEndAsync();
            var form = FormHelper.ParseForm(body);

            var name = form.TryGetValue("name", out var n) ? n.Trim() : "";
            var email = form.TryGetValue("email", out var e) ? e.Trim() : "";
            var personsStr = form.TryGetValue("persons", out var p) ? p.Trim() : "";

                        var errors = Validate(name, email, personsStr);

            if (errors.Count > 0)
            {
                await WriteBadRequest(response, string.Join("; ", errors));
                return;
            }

            var persons = int.Parse(personsStr);

            var booking = new Booking
            {
                TourId = tourId,
                CustomerName = name,
                CustomerEmail = email,
                PersonsCount = persons,
                Status = "new",
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepo.CreateAsync(booking);

            response.StatusCode = 302;
            response.Headers["Location"] = $"/tours/{tourId}";
            response.Close();
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            response.ContentType = "text/plain; charset=utf-8";
            var buffer = Encoding.UTF8.GetBytes("Ошибка сервера при обработке бронирования.");
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }
    }

    private static List<string> Validate(string name, string email, string personsStr)
    {
        var errors = new List<string>();

                var nameRegex = new Regex(@"^[\p{L}\s'-]{1,100}$", RegexOptions.Compiled);
        if (!nameRegex.IsMatch(name))
            errors.Add("Некорректное имя.");

        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(email) || email.Length > 150)
            errors.Add("Некорректный email.");

        if (!int.TryParse(personsStr, out var persons) || persons < 1 || persons > 20)
            errors.Add("Некорректное количество человек.");

        return errors;
    }

    private static async Task WriteBadRequest(HttpListenerResponse response, string message)
    {
        response.StatusCode = 400;
        response.ContentType = "text/plain; charset=utf-8";
        var buffer = Encoding.UTF8.GetBytes(message);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }

    private static async Task WriteNotFound(HttpListenerResponse response, string message)
    {
        response.StatusCode = 404;
        response.ContentType = "text/plain; charset=utf-8";
        var buffer = Encoding.UTF8.GetBytes(message);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }
}
