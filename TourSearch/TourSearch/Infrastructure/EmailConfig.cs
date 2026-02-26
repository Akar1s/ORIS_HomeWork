
namespace TourSearch.Infrastructure;

public class EmailConfig
{
    public string SmtpHost { get; set; } = "smtp.yandex.ru";
    public int SmtpPort { get; set; } = 587;
    public string FromEmail { get; set; } = "S1nhao@yandex.ru";
    public string FromName { get; set; } = "TourSearch";
    public string Username { get; set; } = "S1nhao@yandex.ru";

    public string Password { get; set; } = "nbmxbyjdgcmaplfh";

    public static EmailConfig Default => new EmailConfig
    {
        SmtpHost = "smtp.yandex.ru",
        SmtpPort = 587,
        FromEmail = "S1nhao@yandex.ru",
        FromName = "TourSearch",
        Username = "S1nhao@yandex.ru",
        Password = "nbmxbyjdgcmaplfh"     };
}
