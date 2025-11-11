using System;
using System.Diagnostics;
using System.IO;

namespace ShimmerAPI
{
    /// <summary>
    /// Logs UDP markers (both inbound and outbound) to a separate CSV file with high-resolution timing.
    /// Uses System.Diagnostics.Stopwatch for microsecond-precision elapsed time tracking.
    /// </summary>
    public class MarkerLogger : IDisposable
    {
        private StreamWriter markerFile = null;
        private readonly string fileName;
        private readonly string delimiter = ",";
        private readonly Stopwatch stopwatch;
        private readonly object fileLock = new object();
        private bool disposed = false;
        private readonly DateTime sessionStartTime;
        private readonly long initialQpcTicks;
        private readonly string subjectName;
        private readonly string deviceInfo;
        private readonly string softwareVersion;
        private readonly string deviceSerialNumber;

        /// <summary>
        /// Creates a new marker logger with the specified filename.
        /// Automatically starts the high-resolution timer.
        /// </summary>
        /// <param name="fileName">Full path to the marker log CSV file</param>
        /// <param name="subjectName">Subject/session name</param>
        /// <param name="deviceInfo">Device information (optional)</param>
        /// <param name="softwareVersion">Software version (optional)</param>
        /// <param name="deviceSerialNumber">Device serial number (optional)</param>
        public MarkerLogger(string fileName, string subjectName = "", string deviceInfo = "", string softwareVersion = "", string deviceSerialNumber = "")
        {
            this.fileName = fileName;
            this.subjectName = subjectName;
            this.deviceInfo = deviceInfo;
            this.softwareVersion = softwareVersion;
            this.deviceSerialNumber = deviceSerialNumber;
            this.sessionStartTime = DateTime.Now;
            stopwatch = new Stopwatch();

            // Capture the initial QPC timestamp
            this.initialQpcTicks = Stopwatch.GetTimestamp();

            try
            {
                // Create the file and write header
                markerFile = new StreamWriter(fileName, false);
                WriteFileHeader();
                WriteColumnHeader();

                // Start the high-resolution timer
                stopwatch.Start();

                ErrorLogger.LogInfo($"Marker log file opened: {fileName}", "MarkerLogger.Constructor");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Failed to create marker log file: {fileName}", ex, "MarkerLogger.Constructor");
                throw;
            }
        }

        /// <summary>
        /// Writes the file header with session information
        /// </summary>
        private void WriteFileHeader()
        {
            lock (fileLock)
            {
                try
                {
                    if (markerFile == null)
                        return;

                    // Write metadata header
                    markerFile.WriteLine("# ShimmerCapture UDP Marker Log");
                    markerFile.WriteLine($"# Software Version: {(string.IsNullOrEmpty(softwareVersion) ? "N/A" : softwareVersion)}");
                    markerFile.WriteLine($"# Session Start Date: {sessionStartTime:yyyy-MM-dd}");
                    markerFile.WriteLine($"# Session Start Time: {sessionStartTime:HH:mm:ss.fff}");
                    markerFile.WriteLine($"# Subject: {(string.IsNullOrEmpty(subjectName) ? "N/A" : subjectName)}");
                    markerFile.WriteLine($"# Device: {(string.IsNullOrEmpty(deviceInfo) ? "N/A" : deviceInfo)}");
                    markerFile.WriteLine($"# Device Serial Number: {(string.IsNullOrEmpty(deviceSerialNumber) ? "N/A" : deviceSerialNumber)}");
                    markerFile.WriteLine("#");
                    markerFile.WriteLine($"# Timer: System.Diagnostics.Stopwatch (High-Resolution Performance Counter)");
                    markerFile.WriteLine($"# Timer Frequency: {Stopwatch.Frequency} Hz");
                    markerFile.WriteLine($"# Timer Resolution: {1000.0 / Stopwatch.Frequency:F6} ms");
                    markerFile.WriteLine($"# Initial QPC Ticks: {initialQpcTicks}");
                    markerFile.WriteLine($"# Initial QPC Time (ms): {(double)initialQpcTicks * 1000.0 / Stopwatch.Frequency:F6}");
                    markerFile.WriteLine($"# Initial QPC Time (seconds): {(double)initialQpcTicks / Stopwatch.Frequency:F9}");
                    markerFile.WriteLine("#");
                    markerFile.WriteLine("# Marker Definitions:");
                    markerFile.WriteLine("#   230 = Manual marker (button press)");
                    markerFile.WriteLine("#   231 = Recording started");
                    markerFile.WriteLine("#   232 = Recording stopped");
                    markerFile.WriteLine("#   Other = Custom markers received via UDP");
                    markerFile.WriteLine("#");

                    markerFile.Flush();
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("Error writing marker log file header", ex, "MarkerLogger.WriteFileHeader");
                }
            }
        }

        /// <summary>
        /// Writes the CSV column header rows
        /// </summary>
        private void WriteColumnHeader()
        {
            lock (fileLock)
            {
                try
                {
                    if (markerFile == null)
                        return;

                    // Header row with column names
                    markerFile.Write("DateTime" + delimiter);
                    markerFile.Write("ElapsedTime_ms" + delimiter);
                    markerFile.Write("Direction" + delimiter);
                    markerFile.Write("MarkerValue");
                    markerFile.WriteLine();

                    // Unit row
                    markerFile.Write("Timestamp" + delimiter);
                    markerFile.Write("milliseconds" + delimiter);
                    markerFile.Write("Inbound/Outbound" + delimiter);
                    markerFile.Write("Numeric");
                    markerFile.WriteLine();

                    markerFile.Flush();
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("Error writing marker log column header", ex, "MarkerLogger.WriteColumnHeader");
                }
            }
        }

        /// <summary>
        /// Logs an inbound marker (received via UDP)
        /// </summary>
        /// <param name="markerValue">The marker value received</param>
        public void LogInboundMarker(int markerValue)
        {
            LogMarker(markerValue, "Inbound");
        }

        /// <summary>
        /// Logs an outbound marker (sent via UDP)
        /// </summary>
        /// <param name="markerValue">The marker value sent</param>
        public void LogOutboundMarker(int markerValue)
        {
            LogMarker(markerValue, "Outbound");
        }

        /// <summary>
        /// Internal method to log a marker with timestamp and direction
        /// </summary>
        /// <param name="markerValue">The marker value</param>
        /// <param name="direction">Direction: "Inbound" or "Outbound"</param>
        private void LogMarker(int markerValue, string direction)
        {
            if (disposed)
            {
                ErrorLogger.LogWarning("Attempted to log marker to disposed MarkerLogger", "MarkerLogger.LogMarker");
                return;
            }

            lock (fileLock)
            {
                try
                {
                    if (markerFile == null)
                    {
                        ErrorLogger.LogWarning("markerFile is null in LogMarker", "MarkerLogger.LogMarker");
                        return;
                    }

                    // Get current timestamp
                    DateTime now = DateTime.Now;

                    // Get high-resolution elapsed time in milliseconds
                    double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

                    // Write: DateTime, ElapsedTime_ms, Direction, MarkerValue
                    markerFile.Write($"{now:yyyy-MM-dd HH:mm:ss.fff}{delimiter}");
                    markerFile.Write($"{elapsedMs:F3}{delimiter}");
                    markerFile.Write($"{direction}{delimiter}");
                    markerFile.Write($"{markerValue}");
                    markerFile.WriteLine();

                    // Flush to ensure data is written immediately
                    markerFile.Flush();

                    ErrorLogger.LogInfo($"{direction} marker logged: {markerValue} at {elapsedMs:F3}ms", "MarkerLogger.LogMarker");
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError($"Error logging {direction} marker {markerValue}", ex, "MarkerLogger.LogMarker");
                }
            }
        }

        /// <summary>
        /// Closes the marker log file
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the elapsed time in milliseconds since logging started
        /// </summary>
        /// <returns>Elapsed milliseconds</returns>
        public double GetElapsedMilliseconds()
        {
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// Gets the file path of the marker log
        /// </summary>
        /// <returns>Full path to the marker log file</returns>
        public string GetFilePath()
        {
            return fileName;
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                lock (fileLock)
                {
                    try
                    {
                        // Stop the timer
                        if (stopwatch != null && stopwatch.IsRunning)
                        {
                            stopwatch.Stop();
                        }

                        // Close the file
                        if (markerFile != null)
                        {
                            markerFile.Flush();
                            markerFile.Close();
                            markerFile.Dispose();
                            markerFile = null;
                            ErrorLogger.LogInfo($"Marker log file closed: {fileName}", "MarkerLogger.Dispose");
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError("Error closing marker log file", ex, "MarkerLogger.Dispose");
                    }
                }
            }

            disposed = true;
        }

        ~MarkerLogger()
        {
            Dispose(false);
        }

        #endregion
    }
}
