using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;

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

        public Logging(String fileName, String delimeter)
        {
            Delimeter = delimeter;
            FileName = fileName;
            try
            {
                PCsvFile = new StreamWriter(FileName, false);
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

                    PCsvFile.Write(System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond + Delimeter);

                    PCsvFile.Write(obj.elapsedTimer.ToString() + Delimeter);

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

                PCsvFile.Write("SystemTime" + Delimeter);
                PCsvFile.Write("ElapsedTime" + Delimeter);

                for (int i = 0; i < data.Count; i++)
                {
                    PCsvFile.Write(names[i] + Delimeter);
                }
                PCsvFile.Write("Markers" + Delimeter);
                PCsvFile.WriteLine();

                PCsvFile.Write("SystemTime" + Delimeter);
                PCsvFile.Write("ElapsedTime" + Delimeter);
                for (int i = 0; i < data.Count; i++)
                {
                    PCsvFile.Write(formats[i] + Delimeter);
                }
                PCsvFile.Write("NA" + Delimeter);

                PCsvFile.WriteLine();

                PCsvFile.Write("DateTime" + Delimeter);
                PCsvFile.Write("ms" + Delimeter);
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
