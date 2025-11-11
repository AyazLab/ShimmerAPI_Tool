using System;
using System.Collections.Generic;
using System.Linq;

namespace ShimmerCapture
{
    /// <summary>
    /// Helper class for categorizing and grouping Shimmer sensor signals
    /// </summary>
    public static class SignalGroupHelper
    {
        public enum SignalCategory
        {
            All,
            IMU,
            ExG,
            Physiological,
            Environmental,
            Timestamp,
            Other
        }

        /// <summary>
        /// Gets the category for a given signal name
        /// </summary>
        public static SignalCategory GetSignalCategory(string signalName)
        {
            if (string.IsNullOrWhiteSpace(signalName))
                return SignalCategory.Other;

            string lower = signalName.ToLowerInvariant();

            // IMU sensors (Inertial Measurement Unit)
            if (lower.Contains("accelerometer") || lower.Contains("accel") ||
                lower.Contains("gyroscope") || lower.Contains("gyro") ||
                lower.Contains("magnetometer") || lower.Contains("mag"))
            {
                return SignalCategory.IMU;
            }

            // ExG sensors (ECG, EMG, ExG channels)
            if (lower.Contains("ecg") || lower.Contains("emg") ||
                lower.Contains("exg") || lower.Contains("eeg"))
            {
                return SignalCategory.ExG;
            }

            // Physiological sensors
            if (lower.Contains("gsr") || lower.Contains("ppg") ||
                lower.Contains("heart") || lower.Contains("skin") ||
                lower.Contains("resistance"))
            {
                return SignalCategory.Physiological;
            }

            // Environmental/System sensors
            if (lower.Contains("battery") || lower.Contains("volt") ||
                lower.Contains("temperature") || lower.Contains("temp") ||
                lower.Contains("pressure"))
            {
                return SignalCategory.Environmental;
            }

            // Timestamp signals
            if (lower.Contains("timestamp") || lower.Contains("time") ||
                lower == "ts" || lower.Contains("clock"))
            {
                return SignalCategory.Timestamp;
            }

            return SignalCategory.Other;
        }

        /// <summary>
        /// Gets all categories that have at least one signal in the provided list
        /// </summary>
        public static List<SignalCategory> GetAvailableCategories(List<string> signalNames)
        {
            if (signalNames == null || signalNames.Count == 0)
                return new List<SignalCategory> { SignalCategory.All };

            var categories = new HashSet<SignalCategory>();
            categories.Add(SignalCategory.All); // Always available

            foreach (string signal in signalNames)
            {
                var category = GetSignalCategory(signal);
                if (category != SignalCategory.Other || signalNames.Count(s => GetSignalCategory(s) == SignalCategory.Other) > 0)
                {
                    categories.Add(category);
                }
            }

            return categories.OrderBy(c => c.ToString()).ToList();
        }

        /// <summary>
        /// Filters a list of signal names by category
        /// </summary>
        public static List<string> FilterByCategory(List<string> signalNames, SignalCategory category)
        {
            if (signalNames == null || signalNames.Count == 0)
                return new List<string>();

            if (category == SignalCategory.All)
                return new List<string>(signalNames);

            return signalNames.Where(s => GetSignalCategory(s) == category).ToList();
        }

        /// <summary>
        /// Filters a list of signal names by search text (case-insensitive partial match)
        /// </summary>
        public static List<string> FilterBySearchText(List<string> signalNames, string searchText)
        {
            if (signalNames == null || signalNames.Count == 0)
                return new List<string>();

            if (string.IsNullOrWhiteSpace(searchText))
                return new List<string>(signalNames);

            string lowerSearch = searchText.ToLowerInvariant();
            return signalNames.Where(s => s.ToLowerInvariant().Contains(lowerSearch)).ToList();
        }

        /// <summary>
        /// Gets a display-friendly name for a category
        /// </summary>
        public static string GetCategoryDisplayName(SignalCategory category)
        {
            switch (category)
            {
                case SignalCategory.All:
                    return "All Signals";
                case SignalCategory.IMU:
                    return "IMU (Accelerometer, Gyro, Magnetometer)";
                case SignalCategory.ExG:
                    return "ExG (ECG, EMG, EEG)";
                case SignalCategory.Physiological:
                    return "Physiological (GSR, PPG, Heart Rate)";
                case SignalCategory.Environmental:
                    return "Environmental (Battery, Temperature)";
                case SignalCategory.Timestamp:
                    return "Timestamps";
                case SignalCategory.Other:
                    return "Other Signals";
                default:
                    return category.ToString();
            }
        }
    }
}
