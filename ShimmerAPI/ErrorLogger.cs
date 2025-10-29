using System;
using System.IO;
using System.Text;
using System.Threading;

namespace ShimmerAPI
{
    /// <summary>
    /// Centralized error logging system for diagnostic purposes
    /// Thread-safe logging to file with timestamps and stack traces
    /// </summary>
    public static class ErrorLogger
    {
        private static readonly object lockObject = new object();
        private static string logFilePath;
        private static bool isInitialized = false;

        /// <summary>
        /// Initialize the error logger with a specific log file path
        /// </summary>
        /// <param name="filePath">Path to the log file. If null, uses default location.</param>
        public static void Initialize(string filePath = null)
        {
            lock (lockObject)
            {
                if (isInitialized)
                    return;

                if (string.IsNullOrEmpty(filePath))
                {
                    // Default: Create log in user's temp directory
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    logFilePath = Path.Combine(Path.GetTempPath(), $"ShimmerAPI_ErrorLog_{timestamp}.txt");
                }
                else
                {
                    logFilePath = filePath;
                }

                try
                {
                    // Create directory if it doesn't exist
                    string directory = Path.GetDirectoryName(logFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Write initial log entry
                    File.AppendAllText(logFilePath,
                        $"=== ShimmerAPI Error Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==={Environment.NewLine}");

                    isInitialized = true;
                }
                catch (Exception ex)
                {
                    // If we can't write to the log file, at least write to console
                    Console.WriteLine($"Failed to initialize error logger: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Log an error message with optional exception details
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="ex">Optional exception object</param>
        /// <param name="source">Optional source identifier (class name, method name, etc.)</param>
        public static void LogError(string message, Exception ex = null, string source = null)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            lock (lockObject)
            {
                try
                {
                    StringBuilder logEntry = new StringBuilder();

                    // Timestamp and thread info
                    logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [Thread {Thread.CurrentThread.ManagedThreadId}]");

                    // Source information
                    if (!string.IsNullOrEmpty(source))
                    {
                        logEntry.AppendLine($"Source: {source}");
                    }

                    // Message
                    logEntry.AppendLine($"Message: {message}");

                    // Exception details
                    if (ex != null)
                    {
                        logEntry.AppendLine($"Exception Type: {ex.GetType().FullName}");
                        logEntry.AppendLine($"Exception Message: {ex.Message}");
                        logEntry.AppendLine($"Stack Trace:");
                        logEntry.AppendLine(ex.StackTrace);

                        // Inner exception
                        if (ex.InnerException != null)
                        {
                            logEntry.AppendLine($"Inner Exception: {ex.InnerException.GetType().FullName}");
                            logEntry.AppendLine($"Inner Exception Message: {ex.InnerException.Message}");
                            logEntry.AppendLine($"Inner Stack Trace:");
                            logEntry.AppendLine(ex.InnerException.StackTrace);
                        }
                    }

                    logEntry.AppendLine(new string('-', 80));

                    // Write to file
                    File.AppendAllText(logFilePath, logEntry.ToString());
                }
                catch (Exception logEx)
                {
                    // Last resort: write to console if file writing fails
                    Console.WriteLine($"Failed to write to error log: {logEx.Message}");
                    Console.WriteLine($"Original error: {message}");
                }
            }
        }

        /// <summary>
        /// Log an informational message (for debugging)
        /// </summary>
        /// <param name="message">Info message</param>
        /// <param name="source">Optional source identifier</param>
        public static void LogInfo(string message, string source = null)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            lock (lockObject)
            {
                try
                {
                    StringBuilder logEntry = new StringBuilder();
                    logEntry.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [INFO]");

                    if (!string.IsNullOrEmpty(source))
                    {
                        logEntry.Append($" [{source}]");
                    }

                    logEntry.AppendLine($" {message}");

                    File.AppendAllText(logFilePath, logEntry.ToString());
                }
                catch
                {
                    // Silently fail for info messages
                }
            }
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="source">Optional source identifier</param>
        public static void LogWarning(string message, string source = null)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            lock (lockObject)
            {
                try
                {
                    StringBuilder logEntry = new StringBuilder();
                    logEntry.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [WARNING]");

                    if (!string.IsNullOrEmpty(source))
                    {
                        logEntry.Append($" [{source}]");
                    }

                    logEntry.AppendLine($" {message}");

                    File.AppendAllText(logFilePath, logEntry.ToString());
                }
                catch
                {
                    // Silently fail for warning messages
                }
            }
        }

        /// <summary>
        /// Get the current log file path
        /// </summary>
        public static string GetLogFilePath()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            return logFilePath;
        }

        /// <summary>
        /// Log memory usage information for leak detection
        /// </summary>
        public static void LogMemoryUsage(string context = null)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            try
            {
                long memoryUsed = GC.GetTotalMemory(false);
                int threadCount = System.Diagnostics.Process.GetCurrentProcess().Threads.Count;

                string message = $"Memory: {memoryUsed / 1024 / 1024} MB, Threads: {threadCount}";
                if (!string.IsNullOrEmpty(context))
                {
                    message = $"{context} - {message}";
                }

                LogInfo(message, "MemoryMonitor");
            }
            catch (Exception ex)
            {
                LogError("Failed to log memory usage", ex, "MemoryMonitor");
            }
        }
    }
}
