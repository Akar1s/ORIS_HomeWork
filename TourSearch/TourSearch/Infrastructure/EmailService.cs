
using System.Net;
using System.Net.Mail;
using System.Text;

namespace TourSearch.Infrastructure;

public interface IEmailService
{
    Task SendLoginNotificationAsync(string recipientEmail, string recipientName, DateTime loginTime, string ipAddress);
    Task SendPasswordResetAsync(string recipientEmail, string resetToken);
    Task SendPasswordChangedNotificationAsync(string recipientEmail);
}

public class EmailService : IEmailService
{
    private readonly EmailConfig _config;

    public EmailService(EmailConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task SendLoginNotificationAsync(string recipientEmail, string recipientName, DateTime loginTime, string ipAddress)
    {
        var subject = "Уведомление о входе в аккаунт - TourSearch";

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6B46C1; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
        .footer {{ text-align: center; padding: 10px; font-size: 12px; color: #777; }}
        .info-block {{ background-color: white; padding: 15px; margin: 10px 0; border-left: 4px solid #6B46C1; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🔐 TourSearch</h2>
        </div>
        <div class='content'>
            <h3>Здравствуйте, {recipientName}!</h3>
            <p>В ваш аккаунт был выполнен вход.</p>
            
            <div class='info-block'>
                <p><strong>Дата и время:</strong> {loginTime:dd.MM.yyyy HH:mm:ss} MSK</p>
                <p><strong>IP-адрес:</strong> {ipAddress}</p>
            </div>
            
            <p>Если это были не вы, немедленно смените пароль и свяжитесь с нами.</p>
        </div>
        <div class='footer'>
            <p>Это автоматическое письмо. Пожалуйста, не отвечайте на него.</p>
            <p>&copy; 2025 TourSearch. Все права защищены.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    public async Task SendPasswordResetAsync(string recipientEmail, string resetToken)
    {
        var subject = "Сброс пароля - TourSearch";

        var resetLink = $"http://localhost:8080/account/reset-password?token={resetToken}";

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6B46C1; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #6B46C1; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 10px; font-size: 12px; color: #777; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 10px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🔑 Сброс пароля</h2>
        </div>
        <div class='content'>
            <h3>Запрос на сброс пароля</h3>
            <p>Вы запросили сброс пароля для вашего аккаунта TourSearch.</p>
            
            <p>Нажмите на кнопку ниже, чтобы установить новый пароль:</p>
            
            <a href='{resetLink}' class='button'>Сбросить пароль</a>
            
            <p>Или скопируйте эту ссылку в браузер:</p>
            <p style='word-break: break-all; color: #6B46C1;'>{resetLink}</p>
            
            <div class='warning'>
                <strong>⚠️ Внимание:</strong> Ссылка действительна в течение 1 часа.
            </div>
            
            <p>Если вы не запрашивали сброс пароля, просто проигнорируйте это письмо.</p>
        </div>
        <div class='footer'>
            <p>Это автоматическое письмо. Пожалуйста, не отвечайте на него.</p>
            <p>&copy; 2025 TourSearch. Все права защищены.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    public async Task SendPasswordChangedNotificationAsync(string recipientEmail)
    {
        var subject = "Пароль изменен - TourSearch";

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6B46C1; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
        .footer {{ text-align: center; padding: 10px; font-size: 12px; color: #777; }}
        .success {{ background-color: #d4edda; border: 1px solid #28a745; padding: 10px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>✅ Пароль изменен</h2>
        </div>
        <div class='content'>
            <div class='success'>
                <strong>Ваш пароль успешно изменен</strong>
            </div>
            
            <p>Пароль для вашего аккаунта TourSearch был изменен {DateTime.UtcNow:dd.MM.yyyy в HH:mm:ss} UTC.</p>
            
            <p>Если это были не вы, немедленно свяжитесь с нами по адресу: <a href='mailto:{_config.FromEmail}'>{_config.FromEmail}</a></p>
        </div>
        <div class='footer'>
            <p>Это автоматическое письмо. Пожалуйста, не отвечайте на него.</p>
            <p>&copy; 2025 TourSearch. Все права защищены.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        Logger.Info($"Attempting to send email to {to}");
        Logger.Info($"SMTP: {_config.SmtpHost}:{_config.SmtpPort}");
        Logger.Info($"From: {_config.FromEmail}");
        Logger.Info($"Password set: {!string.IsNullOrEmpty(_config.Password)}");

        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_config.FromEmail, _config.FromName);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;
            message.BodyEncoding = Encoding.UTF8;
            message.SubjectEncoding = Encoding.UTF8;

            using var smtp = new SmtpClient(_config.SmtpHost, _config.SmtpPort);
            smtp.Credentials = new NetworkCredential(_config.Username, _config.Password);
            smtp.EnableSsl = true;
            smtp.Timeout = 30000; 
            Logger.Info("Sending email...");
            await smtp.SendMailAsync(message);

            Logger.Info($"✅ Email sent successfully to {to}");
        }
        catch (SmtpException ex)
        {
            Logger.Error(ex, $"❌ SMTP error while sending email to {to}");
            Logger.Error($"Status code: {ex.StatusCode}");
            throw new EmailServiceException($"Ошибка SMTP: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"❌ Unexpected error while sending email to {to}");
            throw new EmailServiceException($"Не удалось отправить письмо: {ex.Message}", ex);
        }
    }
}

public class EmailServiceException : Exception
{
    public EmailServiceException(string message) : base(message) { }
    public EmailServiceException(string message, Exception innerException) : base(message, innerException) { }
}
