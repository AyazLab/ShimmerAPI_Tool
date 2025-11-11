using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ShimmerCapture
{
    /// <summary>
    /// Handles persistence of signal selection preferences
    /// Saves/loads which signals are selected for each graph
    /// </summary>
    public static class SignalSelectionSettings
    {
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShimmerCapture");

        private static readonly string SettingsFilePath = Path.Combine(
            SettingsDirectory,
            "signal_selections.json");

        /// <summary>
        /// Simple logging helper (avoids dependency on ShimmerAPI.ErrorLogger)
        /// </summary>
        private static void Log(string message)
        {
            try
            {
                // Use ShimmerAPI.ErrorLogger if available
                var errorLoggerType = Type.GetType("ShimmerAPI.ErrorLogger, ShimmerAPI");
                if (errorLoggerType != null)
                {
                    var logMethod = errorLoggerType.GetMethod("Log", new[] { typeof(string) });
                    if (logMethod != null)
                    {
                        logMethod.Invoke(null, new object[] { message });
                        return;
                    }
                }
                // Fallback: silent operation (settings are not critical)
            }
            catch
            {
                // Silent failure - settings persistence is not critical
            }
        }

        /// <summary>
        /// Data structure for persisting selections
        /// </summary>
        [DataContract]
        public class SignalSelections
        {
            [DataMember]
            public Dictionary<int, List<string>> GraphSelections { get; set; }

            [DataMember]
            public DateTime LastSaved { get; set; }

            [DataMember]
            public string Version { get; set; }

            public SignalSelections()
            {
                GraphSelections = new Dictionary<int, List<string>>();
                LastSaved = DateTime.Now;
                Version = "1.0";
            }
        }

        /// <summary>
        /// Saves the current signal selections for all graphs
        /// </summary>
        /// <param name="graph1Selections">Selected signals for graph 1</param>
        /// <param name="graph2Selections">Selected signals for graph 2</param>
        /// <param name="graph3Selections">Selected signals for graph 3</param>
        /// <returns>True if save successful, false otherwise</returns>
        public static bool SaveSelections(
            List<string> graph1Selections,
            List<string> graph2Selections,
            List<string> graph3Selections)
        {
            try
            {
                // Create directory if it doesn't exist
                if (!Directory.Exists(SettingsDirectory))
                {
                    Directory.CreateDirectory(SettingsDirectory);
                }

                var selections = new SignalSelections
                {
                    GraphSelections = new Dictionary<int, List<string>>
                    {
                        { 1, graph1Selections ?? new List<string>() },
                        { 2, graph2Selections ?? new List<string>() },
                        { 3, graph3Selections ?? new List<string>() }
                    },
                    LastSaved = DateTime.Now,
                    Version = "1.0"
                };

                var serializer = new DataContractJsonSerializer(typeof(SignalSelections));
                using (var stream = new FileStream(SettingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    serializer.WriteObject(stream, selections);
                }

                Log($"Signal selections saved successfully to {SettingsFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Failed to save signal selections: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads saved signal selections
        /// Best-effort: returns empty lists if file doesn't exist or is invalid
        /// </summary>
        /// <param name="graph1Selections">Output: selections for graph 1</param>
        /// <param name="graph2Selections">Output: selections for graph 2</param>
        /// <param name="graph3Selections">Output: selections for graph 3</param>
        /// <returns>True if loaded successfully, false if using defaults</returns>
        public static bool LoadSelections(
            out List<string> graph1Selections,
            out List<string> graph2Selections,
            out List<string> graph3Selections)
        {
            // Initialize with empty lists (graceful default)
            graph1Selections = new List<string>();
            graph2Selections = new List<string>();
            graph3Selections = new List<string>();

            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    Log("No saved signal selections found. Using defaults (empty).");
                    return false;
                }

                var serializer = new DataContractJsonSerializer(typeof(SignalSelections));
                SignalSelections selections;

                using (var stream = new FileStream(SettingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    selections = (SignalSelections)serializer.ReadObject(stream);
                }

                if (selections == null || selections.GraphSelections == null)
                {
                    Log("Signal selections file is invalid. Using defaults.");
                    return false;
                }

                // Extract selections for each graph (graceful if keys don't exist)
                if (selections.GraphSelections.ContainsKey(1))
                    graph1Selections = selections.GraphSelections[1] ?? new List<string>();

                if (selections.GraphSelections.ContainsKey(2))
                    graph2Selections = selections.GraphSelections[2] ?? new List<string>();

                if (selections.GraphSelections.ContainsKey(3))
                    graph3Selections = selections.GraphSelections[3] ?? new List<string>();

                Log($"Signal selections loaded successfully. Last saved: {selections.LastSaved}");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Failed to load signal selections: {ex.Message}. Using defaults.");
                // Gracefully return empty lists (already initialized above)
                return false;
            }
        }

        /// <summary>
        /// Validates saved selections against currently available signals
        /// Returns only the signals that exist in the available list (best-effort matching)
        /// </summary>
        /// <param name="savedSelections">Previously saved signal selections</param>
        /// <param name="availableSignals">Currently available signal names</param>
        /// <returns>Filtered list containing only valid signals</returns>
        public static List<string> ValidateAndFilter(
            List<string> savedSelections,
            List<string> availableSignals)
        {
            if (savedSelections == null || savedSelections.Count == 0)
                return new List<string>();

            if (availableSignals == null || availableSignals.Count == 0)
            {
                Log("No available signals to validate against. Returning empty selection.");
                return new List<string>();
            }

            // Best-effort: keep only signals that still exist
            var validated = savedSelections.Where(s => availableSignals.Contains(s)).ToList();

            // Log what was filtered out
            var missing = savedSelections.Where(s => !availableSignals.Contains(s)).ToList();
            if (missing.Count > 0)
            {
                Log($"Some saved signals are no longer available and were removed: {string.Join(", ", missing)}");
            }

            return validated;
        }

        /// <summary>
        /// Clears saved selections (resets to defaults)
        /// </summary>
        public static bool ClearSelections()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    File.Delete(SettingsFilePath);
                    Log("Signal selections cleared successfully.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log($"Failed to clear signal selections: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the path to the settings file for debugging purposes
        /// </summary>
        public static string GetSettingsFilePath()
        {
            return SettingsFilePath;
        }
    }
}
