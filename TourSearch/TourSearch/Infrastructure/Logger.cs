using System.Text;

namespace TourSearch.Infrastructure;

public static class Logger
{
    private static readonly object _lock = new();
    private static readonly string _logFilePath;
    private static bool _initialized;

    static Logger()
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;
            var logDir = Path.Combine(baseDir, "logs");
            Directory.CreateDirectory(logDir);

            _logFilePath = Path.Combine(logDir, $"log-{DateTime.UtcNow:yyyyMMdd}.txt");
            _initialized = true;
        }
        catch
        {
                        try
            {
                var tempDir = Path.GetTempPath();
                _logFilePath = Path.Combine(tempDir, $"toursearch-log-{DateTime.UtcNow:yyyyMMdd}.txt");
                _initialized = true;
            }
            catch
            {
                _initialized = false;
            }
        }
    }

    public static void Info(string message) => Write("INFO", message);
    public static void Warn(string message) => Write("WARN", message);
    public static void Warning(string message) => Write("WARN", message);
    public static void Error(string message) => Write("ERROR", message);

    public static void Error(Exception? ex, string context)
    {
        if (ex == null)
        {
            Write("ERROR", context);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine(context);
        sb.AppendLine(ex.ToString());
        Write("ERROR", sb.ToString());
    }

    private static void Write(string level, string message)
    {
        var line = $"[{DateTime.UtcNow:O}] [{level}] {message}{Environment.NewLine}";

                try
        {
            Console.WriteLine($"[{level}] {message}");
        }
        catch
        {
                    }

                if (!_initialized || string.IsNullOrEmpty(_logFilePath))
            return;

        try
        {
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, line, Encoding.UTF8);
            }
        }
        catch
        {
                    }
    }
}
