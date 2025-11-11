using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ShimmerAPI
{
    class Logging : IDisposable
    {
        private StreamWriter PCsvFile = null;
        private readonly String FileName;
        private readonly String Delimeter = ",";
        private Boolean FirstWrite = true;
        private readonly object fileLock = new object();
        private bool disposed = false;
        private readonly DateTime sessionStartTime;
        private readonly string subjectName;
        private readonly string softwareVersion;
        private readonly string deviceInfo;
        private readonly string deviceSerial;
        private readonly Stopwatch stopwatch;
        private readonly long initialQpcTicks;

        public Logging(String fileName, String delimeter, string subjectName = "", string softwareVersion = "", string deviceInfo = "", string deviceSerial = "")
        {
            Delimeter = delimeter;
            FileName = fileName;
            this.subjectName = subjectName;
            this.softwareVersion = softwareVersion;
            this.deviceInfo = deviceInfo;
            this.deviceSerial = deviceSerial;
            this.sessionStartTime = DateTime.Now;

            // Initialize high-resolution timer
            stopwatch = new Stopwatch();
            initialQpcTicks = Stopwatch.GetTimestamp();

            try
            {
                PCsvFile = new StreamWriter(FileName, false);
                WriteFileHeader();

                // Start the stopwatch after header is written
                stopwatch.Start();

                ErrorLogger.LogInfo($"CSV file opened: {fileName}", "Logging.Constructor");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Failed to open CSV file: {fileName}", ex, "Logging.Constructor");
                MessageBox.Show(ex.Message, "Save to CSV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Re-throw so caller knows file creation failed
                throw;
            }
        }

        private void WriteFileHeader()
        {
            lock (fileLock)
            {
                try
                {
                    if (PCsvFile == null)
                        return;

                    // Write metadata header
                    PCsvFile.WriteLine("# ShimmerCapture Data Log");
                    PCsvFile.WriteLine($"# Software Version: {(string.IsNullOrEmpty(softwareVersion) ? "N/A" : softwareVersion)}");
                    PCsvFile.WriteLine($"# Session Start Date: {sessionStartTime:yyyy-MM-dd}");
                    PCsvFile.WriteLine($"# Session Start Time: {sessionStartTime:HH:mm:ss.fff}");
                    PCsvFile.WriteLine($"# Subject: {(string.IsNullOrEmpty(subjectName) ? "N/A" : subjectName)}");
                    PCsvFile.WriteLine($"# Device: {(string.IsNullOrEmpty(deviceInfo) ? "N/A" : deviceInfo)}");
                    PCsvFile.WriteLine($"# Device Serial Number: {(string.IsNullOrEmpty(deviceSerial) ? "N/A" : deviceSerial)}");
                    PCsvFile.WriteLine("#");
                    PCsvFile.WriteLine($"# Timer: System.Diagnostics.Stopwatch (High-Resolution Performance Counter)");
                    PCsvFile.WriteLine($"# Timer Frequency: {Stopwatch.Frequency} Hz");
                    PCsvFile.WriteLine($"# Timer Resolution: {1000.0 / Stopwatch.Frequency:F6} ms");
                    PCsvFile.WriteLine($"# Initial QPC Ticks: {initialQpcTicks}");
                    PCsvFile.WriteLine($"# Initial QPC Time (ms): {(double)initialQpcTicks * 1000.0 / Stopwatch.Frequency:F6}");
                    PCsvFile.WriteLine($"# Initial QPC Time (seconds): {(double)initialQpcTicks / Stopwatch.Frequency:F9}");
                    PCsvFile.WriteLine("#");
                    PCsvFile.WriteLine("# Data Format:");
                    PCsvFile.WriteLine("#   QPC_Ticks: Raw QueryPerformanceCounter ticks");
                    PCsvFile.WriteLine("#   TIMESTAMP: Device timestamp (RAW = counter, CAL = milliseconds)");
                    PCsvFile.WriteLine("#   Markers: UDP markers received (MRKxxx) or blank");
                    PCsvFile.WriteLine("#");

                    PCsvFile.Flush();
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("Error writing CSV file header", ex, "Logging.WriteFileHeader");
                }
            }
        }

        volatile bool bRowInProgress = false;
        volatile int bWriteMarkerNext = 0;
        public long mrkReceivedTime = 0;

        public void WriteMarker(int inp)
        {
            lock (fileLock)
            {
                bWriteMarkerNext = inp;
            }
        }

        public void WriteData(ObjectCluster obj)
        {
            if (disposed)
            {
                ErrorLogger.LogWarning("Attempted to write to disposed Logging object", "Logging.WriteData");
                return;
            }

            lock (fileLock)
            {
                try
                {
                    if (PCsvFile == null)
                    {
                        ErrorLogger.LogWarning("PCsvFile is null in WriteData", "Logging.WriteData");
                        return;
                    }

                    bRowInProgress = true;

                    if (FirstWrite)
                    {
                        WriteHeader(obj);
                        FirstWrite = false;
                    }

                    Double[] data = obj.GetData().ToArray();

                    // Write raw QPC ticks
                    long currentQpcTicks = Stopwatch.GetTimestamp();
                    PCsvFile.Write(currentQpcTicks.ToString() + Delimeter);

                    for (int i = 0; i < data.Length; i++)
                    {
                        PCsvFile.Write(data[i].ToString() + Delimeter);
                    }

                    if (bWriteMarkerNext > 0)
                    {
                        PCsvFile.Write("MRK" + bWriteMarkerNext + Delimeter);
                        bWriteMarkerNext = 0;
                    }
                    else
                    {
                        PCsvFile.Write(Delimeter);
                    }

                    PCsvFile.WriteLine();

                    // Flush periodically to ensure data is written
                    PCsvFile.Flush();

                    bRowInProgress = false;
                }
                catch (Exception ex)
                {
                    bRowInProgress = false;
                    ErrorLogger.LogError("Error writing data to CSV file", ex, "Logging.WriteData");
                }
            }
        }

        private void WriteHeader(ObjectCluster obj)
        {
            try
            {
                if (PCsvFile == null)
                {
                    ErrorLogger.LogWarning("PCsvFile is null in WriteHeader", "Logging.WriteHeader");
                    return;
                }

                ObjectCluster objectCluster = new ObjectCluster(obj);
                List<String> names = objectCluster.GetNames();
                List<String> formats = objectCluster.GetFormats();
                List<String> units = objectCluster.GetUnits();
                List<Double> data = objectCluster.GetData();
                String deviceId = objectCluster.GetShimmerID();

                //for (int i = 0; i < data.Count; i++)
                //{
                PCsvFile.Write(deviceId);
                //}
                PCsvFile.WriteLine();

                // Column names row
                PCsvFile.Write("QPC_Ticks" + Delimeter);

                for (int i = 0; i < data.Count; i++)
                {
                    PCsvFile.Write(names[i] + Delimeter);
                }
                PCsvFile.Write("Markers" + Delimeter);
                PCsvFile.WriteLine();

                // Format row
                PCsvFile.Write("RAW" + Delimeter);
                for (int i = 0; i < data.Count; i++)
                {
                    PCsvFile.Write(formats[i] + Delimeter);
                }
                PCsvFile.Write("NA" + Delimeter);

                PCsvFile.WriteLine();

                // Units row
                PCsvFile.Write("ticks" + Delimeter);
                for (int i = 0; i < data.Count; i++)
                {
                    PCsvFile.Write(units[i] + Delimeter);
                }
                PCsvFile.Write("NA" + Delimeter);
                PCsvFile.WriteLine();
                PCsvFile.Flush();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("Error writing header to CSV file", ex, "Logging.WriteHeader");
            }
        }

        public void CloseFile()
        {
            Dispose();
        }

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
                        // Stop the stopwatch
                        if (stopwatch != null && stopwatch.IsRunning)
                        {
                            stopwatch.Stop();
                        }

                        if (PCsvFile != null)
                        {
                            PCsvFile.Flush();
                            PCsvFile.Close();
                            PCsvFile.Dispose();
                            PCsvFile = null;
                            ErrorLogger.LogInfo($"CSV file closed: {FileName}", "Logging.Dispose");
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError("Error closing CSV file", ex, "Logging.Dispose");
                    }
                }
            }

            disposed = true;
        }

        ~Logging()
        {
            Dispose(false);
        }
    }
}
