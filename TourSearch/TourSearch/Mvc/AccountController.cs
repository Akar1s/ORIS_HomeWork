
using System.Text.RegularExpressions;
using TourSearch.Data;
using TourSearch.Domain.Entities;
using TourSearch.Infrastructure;
using TourSearch.Mvc;

namespace TourSearch.Controllers;

public class AccountController
{
    private readonly string _projectRoot;
    private readonly UserRepository _userRepo;
    private readonly IEmailService? _emailService;

    public AccountController(string projectRoot, UserRepository userRepo, IEmailService? emailService)
    {
        _projectRoot = projectRoot ?? throw new ArgumentNullException(nameof(projectRoot));
        _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        _emailService = emailService;     }

    private string LoginViewPath =>
        Path.Combine(_projectRoot, "Views", "Account", "Login.html");

    public Task<ControllerResult> ShowLoginAsync(string? error = null)
    {
        try
        {
            var model = new Dictionary<string, object?>
            {
                ["ErrorMessage"] = error ?? ""
            };

            return Task.FromResult<ControllerResult>(new ViewResult(LoginViewPath, model, 200));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in ShowLoginAsync");
            return Task.FromResult<ControllerResult>(new HtmlResult("<h1>Error loading login page</h1>", 500));
        }
    }

    public async Task<(bool Ok, string? Error, User? User)> LoginAsync(string email, string password, string ipAddress)
    {
        try
        {
            email = (email ?? "").Trim();
            password = password ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Введите email и пароль.", null);

            User? user;
            try
            {
                user = await _userRepo.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database error in LoginAsync");
                return (false, "Ошибка при проверке учетных данных. Попробуйте позже.", null);
            }

            if (user == null)
                return (false, "Неверный email или пароль.", null);

            bool ok;
            try
            {
                ok = PasswordHasher.VerifyPassword(password, user.Salt, user.PasswordHash);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error verifying password");
                return (false, "Ошибка при проверке пароля.", null);
            }

            if (!ok)
                return (false, "Неверный email или пароль.", null);

                        if (_emailService != null)
            {
                try
                {
                    await _emailService.SendLoginNotificationAsync(
                        user.Email,
                        user.Email.Split('@')[0],
                        DateTime.UtcNow,
                        ipAddress ?? "unknown"
                    );
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to send login notification email: {ex.Message}");
                                    }
            }

            return (true, null, user);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected error in LoginAsync");
            return (false, "Произошла ошибка. Попробуйте позже.", null);
        }
    }

    private string RegisterViewPath =>
        Path.Combine(_projectRoot, "Views", "Account", "Register.html");

    public Task<ControllerResult> ShowRegisterAsync(string? error = null, string? success = null)
    {
        try
        {
            var model = new Dictionary<string, object?>
            {
                ["ErrorMessage"] = error ?? "",
                ["SuccessMessage"] = success ?? ""
            };

            return Task.FromResult<ControllerResult>(new ViewResult(RegisterViewPath, model, 200));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in ShowRegisterAsync");
            return Task.FromResult<ControllerResult>(new HtmlResult("<h1>Error loading register page</h1>", 500));
        }
    }

    public async Task<(bool Ok, string? Error)> RegisterAsync(string email, string password, string confirm)
    {
        try
        {
            email = (email ?? "").Trim();
            password = password ?? "";
            confirm = confirm ?? "";

                        var emailRegex = new Regex(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$", RegexOptions.IgnoreCase);
            if (!emailRegex.IsMatch(email) || email.Length > 150)
                return (false, "Некорректный email.");

                        var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
            if (!passwordRegex.IsMatch(password))
                return (false, "Пароль слишком простой.");

            if (password != confirm)
                return (false, "Пароли не совпадают.");

                        User? existing;
            try
            {
                existing = await _userRepo.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database error checking existing user");
                return (false, "Ошибка при проверке email. Попробуйте позже.");
            }

            if (existing != null)
                return (false, "Пользователь с таким email уже существует.");

            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.HashPassword(password, salt);

            var user = new User
            {
                Email = email,
                Salt = salt,
                PasswordHash = hash,
                Role = "user",
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _userRepo.CreateAsync(user);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database error creating user");
                return (false, "Ошибка при создании пользователя. Попробуйте позже.");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected error in RegisterAsync");
            return (false, "Произошла ошибка. Попробуйте позже.");
        }
    }

    
    private string ForgotPasswordViewPath =>
        Path.Combine(_projectRoot, "Views", "Account", "ForgotPassword.html");

    public Task<ControllerResult> ShowForgotPasswordAsync(string? error = null, string? success = null)
    {
        try
        {
            var model = new Dictionary<string, object?>
            {
                ["ErrorMessage"] = error ?? "",
                ["SuccessMessage"] = success ?? ""
            };

            return Task.FromResult<ControllerResult>(new ViewResult(ForgotPasswordViewPath, model, 200));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in ShowForgotPasswordAsync");
            return Task.FromResult<ControllerResult>(new HtmlResult("<h1>Error loading page</h1>", 500));
        }
    }

    public async Task<(bool Ok, string? Error)> RequestPasswordResetAsync(string email)
    {
        try
        {
            email = (email ?? "").Trim();

            User? user;
            try
            {
                user = await _userRepo.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database error in RequestPasswordResetAsync");
                return (false, "Ошибка при обработке запроса. Попробуйте позже.");
            }

            if (user == null)
            {
                                return (true, null);
            }

            var token = PasswordResetTokenHelper.GenerateToken(user.Id);
            
            try
            {
                await _userRepo.SavePasswordResetTokenAsync(user.Id, token, DateTime.UtcNow.AddHours(1));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database error saving reset token");
                return (false, "Ошибка при создании запроса на сброс. Попробуйте позже.");
            }

            if (_emailService != null)
            {
                try
                {
                    await _emailService.SendPasswordResetAsync(user.Email, token);
                    Logger.Info($"Password reset email sent to {user.Email}");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to send password reset email: {ex.Message}");

                                        var resetLink = $"http://localhost:5000/account/reset-password?token={token}";
                    Console.WriteLine($"[DEBUG] Reset link for {user.Email}: {resetLink}");

                    return (false, "Не удалось отправить письмо. Проверьте настройки email.");
                }
            }
            else
            {
                                var resetLink = $"http://localhost:5000/account/reset-password?token={token}";
                Console.WriteLine($"[INFO] Reset link for {user.Email}: {resetLink}");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected error in RequestPasswordResetAsync");
            return (false, "Произошла ошибка. Попробуйте позже.");
        }
    }

    private string ResetPasswordViewPath =>
        Path.Combine(_projectRoot, "Views", "Account", "ResetPassword.html");

    public Task<ControllerResult> ShowResetPasswordAsync(string token, string? error = null)
    {
        try
        {
            var model = new Dictionary<string, object?>
            {
                ["Token"] = token ?? "",
                ["ErrorMessage"] = error ?? ""
            };

            return Task.FromResult<ControllerResult>(new ViewResult(ResetPasswordViewPath, model, 200));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in ShowResetPasswordAsync");
            return Task.FromResult<ControllerResult>(new HtmlResult("<h1>Error loading page</h1>", 500));
        }
    }

    public async Task<(bool Ok, string? Error)> ResetPasswordAsync(string token, string newPassword, string confirm)
    {
        try
        {
            token = token ?? "";
            newPassword = newPassword ?? "";
            confirm = confirm ?? "";

            if (newPassword != confirm)
                return (false, "Пароли не совпадают.");

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
            if (!passwordRegex.IsMatch(newPassword))
                return (false, "Пароль слишком простой.");

                        int? userId;
            try
            {
                userId = await _userRepo.ValidatePasswordResetTokenAsync(token);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database error validating reset token");
                return (false, "Ошибка при проверке токена. Попробуйте позже.");
            }

            if (userId == null)
                return (false, "Неверный или истекший токен.");

            User? user;
            try
            {
                user = await _userRepo.GetByIdAsync(userId.Value);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database error getting user");
                return (false, "Ошибка при получении данных пользователя.");
            }

            if (user == null)
                return (false, "Пользователь не найден.");

                        var newSalt = PasswordHasher.GenerateSalt();
            var newHash = PasswordHasher.HashPassword(newPassword, newSalt);

            user.Salt = newSalt;
            user.PasswordHash = newHash;

            try
            {
                await _userRepo.UpdateAsync(user);
                await _userRepo.DeletePasswordResetTokenAsync(token);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database error updating password");
                return (false, "Ошибка при смене пароля. Попробуйте позже.");
            }

                        if (_emailService != null)
            {
                try
                {
                    await _emailService.SendPasswordChangedNotificationAsync(user.Email);
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to send password changed notification: {ex.Message}");
                }
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected error in ResetPasswordAsync");
            return (false, "Произошла ошибка. Попробуйте позже.");
        }
    }
}
