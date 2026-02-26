using TourSearch.Server;
using TourSearch.TemplateEngine;
using TourSearch.Data;
using TourSearch.Data.Orm;
using TourSearch.Controllers;
using TourSearch.Infrastructure;
using Npgsql;

AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    var ex = args.ExceptionObject as Exception;
    Logger.Error(ex, "Unhandled exception in AppDomain");
    Console.WriteLine($"[FATAL] Unhandled exception: {ex?.Message}");
};

TaskScheduler.UnobservedTaskException += (sender, args) =>
{
    Logger.Error(args.Exception, "Unobserved task exception");
    Console.WriteLine($"[ERROR] Unobserved task exception: {args.Exception?.Message}");
    args.SetObserved(); };

try
{
    await RunApplicationAsync();
}
catch (Exception ex)
{
    Logger.Error(ex, "Fatal error in application startup");
    Console.WriteLine($"[FATAL] Application startup failed: {ex.Message}");
    Environment.Exit(1);
}

static async Task RunApplicationAsync()
{
    await WaitForDatabaseAsync();
    
            var prefixes = GetServerPrefixes();

    var baseDir = AppContext.BaseDirectory;
    var projectRoot = FindProjectRoot(baseDir);
    var wwwroot = FindWwwRoot(projectRoot, baseDir);

    Console.WriteLine($"Project root: {projectRoot}");
    Console.WriteLine($"Wwwroot: {wwwroot}");

    var templateEngine = new SimpleTemplateEngine();
    var viewRenderer = new ViewRenderer(templateEngine);

        EmailService? emailService = null;
    try
    {
        var emailConfig = EmailConfig.Default;
        emailService = new EmailService(emailConfig);
    }
    catch (Exception ex)
    {
        Logger.Warning($"Email service initialization failed: {ex.Message}. Email notifications will be disabled.");
        Console.WriteLine($"[WARN] Email service disabled: {ex.Message}");
    }

        var sessionFactory = new DbSessionFactory(DbConfig.ConnectionString);

    var userRepo = new UserRepository(sessionFactory);
    var tourRepo = new TourRepository(sessionFactory);
    var destinationRepo = new DestinationRepository(sessionFactory);
    var travelStyleRepo = new TravelStyleRepository(sessionFactory);
    var bookingRepo = new BookingRepository(sessionFactory);

        var accountController = new AccountController(projectRoot, userRepo, emailService);
    var homeController = new HomeController(projectRoot, templateEngine, tourRepo, destinationRepo, travelStyleRepo);
    var landingController = new LandingController(projectRoot);
    var toursController = new ToursController(projectRoot, tourRepo, destinationRepo, travelStyleRepo);
    var adminToursController = new AdminToursController(projectRoot, tourRepo, destinationRepo, travelStyleRepo);
    var placeholderController = new PlaceholderController();

        var landingHandler = new LandingHandler(landingController, viewRenderer);
    var searchHandler = new SearchHandler(homeController, viewRenderer, userRepo);
    var tourDetailsHandler = new TourDetailsHandler(toursController, viewRenderer);
    var adminToursHandler = new AdminToursHandler(adminToursController, viewRenderer, userRepo);
    var accountRegisterHandler = new AccountRegisterHandler(accountController, viewRenderer);
    var accountLoginHandler = new AccountLoginHandler(accountController, viewRenderer, userRepo);
    var accountHandler = new AccountHandler(userRepo, viewRenderer, projectRoot);
    var forgotPasswordHandler = new ForgotPasswordHandler(accountController, viewRenderer);
    var resetPasswordHandler = new ResetPasswordHandler(accountController, viewRenderer);
    var placeholderHandler = new PlaceholderHandler(placeholderController, viewRenderer);

    var toursApiHandler = new ToursApiHandler(tourRepo, destinationRepo, travelStyleRepo);

        var router = new SimpleRouter();
    router.RegisterHandler(landingHandler);           router.RegisterHandler(searchHandler);            router.RegisterHandler(tourDetailsHandler);       router.RegisterHandler(new TourBookingHandler(bookingRepo, tourRepo));
    router.RegisterHandler(accountRegisterHandler);
    router.RegisterHandler(accountLoginHandler);
    router.RegisterHandler(accountHandler);
    router.RegisterHandler(forgotPasswordHandler);
    router.RegisterHandler(resetPasswordHandler);
    router.RegisterHandler(adminToursHandler);
    router.RegisterHandler(new HealthHandler());
    router.RegisterHandler(toursApiHandler);
    router.RegisterHandler(placeholderHandler);       router.RegisterHandler(new LegacyRedirectHandler());
    router.RegisterHandler(new StaticFileHandler(wwwroot)); 
        var cts = new CancellationTokenSource();
    var server = new WebServer(prefixes, router);

    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
        server.RequestStop();
    };

    Console.WriteLine("=".PadRight(60, '='));
    Console.WriteLine("TourSearch Server");
    Console.WriteLine("=".PadRight(60, '='));
    Console.WriteLine($"URL: {prefixes[0]}");
    Console.WriteLine("Press Ctrl+C to stop.");
    Console.WriteLine();

    await server.RunAsync(cts.Token);
}

static string[] GetServerPrefixes()
{
        var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"  
                   || File.Exists("/.dockerenv");
    
    if (isDocker)
    {
                return new[] { "http://+:5000/" };
    }
    
        return new[] { "http://localhost:5000/", "http://127.0.0.1:5000/" };
}

static string FindProjectRoot(string baseDir)
{
        var candidates = new[]
    {
        baseDir,
        Path.Combine(baseDir, ".."),
        Path.Combine(baseDir, "..", ".."),
        Path.Combine(baseDir, "..", "..", ".."),
        Path.Combine(baseDir, "..", "..", "..", ".."),
        "/app",
        Directory.GetCurrentDirectory()
    };

    foreach (var candidate in candidates)
    {
        try
        {
            var fullPath = Path.GetFullPath(candidate);
            if (Directory.Exists(Path.Combine(fullPath, "Views")))
            {
                return fullPath;
            }
        }
        catch
        {
                    }
    }

    return baseDir;
}

static string FindWwwRoot(string projectRoot, string baseDir)
{
    var candidates = new[]
    {
        Path.Combine(projectRoot, "wwwroot"),
        Path.Combine(baseDir, "wwwroot"),
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
    };

    foreach (var candidate in candidates)
    {
        try
        {
            if (Directory.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }
        catch
        {
                    }
    }

    return Path.Combine(projectRoot, "wwwroot");
}

static async Task WaitForDatabaseAsync()
{
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(3);

    Console.WriteLine("Ожидание подключения к базе данных...");

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();
            Console.WriteLine("[OK] Подключение к БД установлено");
            return;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"[WARN] Попытка {i + 1}/{maxRetries} не удалась: {ex.Message}");
            Logger.Warning($"Database connection attempt {i + 1} failed: {ex.Message}");

            if (i == maxRetries - 1)
            {
                Console.WriteLine("[ERROR] Не удалось подключиться к БД. Выход.");
                throw new InvalidOperationException("Database connection failed after all retries", ex);
            }

            await Task.Delay(delay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Неожиданная ошибка при подключении к БД: {ex.Message}");
            Logger.Error(ex, "Unexpected database connection error");
            throw;
        }
    }
}
