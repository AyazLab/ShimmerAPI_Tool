using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;

namespace ShimmerAPI
{
    /// <summary>
    /// Represents information about a COM port with optional Bluetooth device details
    /// </summary>
    public class ComPortInfo
    {
        public string PortName { get; set; }        // e.g., "COM5"
        public string DeviceName { get; set; }      // e.g., "Shimmer3-A4D2" (null if unknown)
        public string Description { get; set; }     // e.g., "Standard Serial over Bluetooth link"
        public bool IsShimmer { get; set; }         // true if identified as Shimmer device
        public string BluetoothAddress { get; set; }// Bluetooth MAC address if available

        /// <summary>
        /// Returns formatted string for display in ComboBox
        /// </summary>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DeviceName))
            {
                return $"{PortName} ({DeviceName})";
            }
            return PortName;
        }

        /// <summary>
        /// Returns just the port name (e.g., "COM5")
        /// </summary>
        public string GetPortName()
        {
            return PortName;
        }
    }

    /// <summary>
    /// Helper class for enhanced COM port detection with Bluetooth device identification
    /// </summary>
    public static class ComPortHelper
    {
        /// <summary>
        /// Gets list of COM ports with enhanced information (Bluetooth device names, etc.)
        /// Combines WMI queries and Bluetooth device discovery for best results
        /// </summary>
        /// <returns>List of ComPortInfo objects</returns>
        public static List<ComPortInfo> GetEnhancedComPorts()
        {
            List<ComPortInfo> ports = new List<ComPortInfo>();

            try
            {
                // Step 1: Get basic COM port list
                string[] portNames = SerialPort.GetPortNames();
                if (portNames == null || portNames.Length == 0)
                {
                    ErrorLogger.LogInfo("No COM ports found", "ComPortHelper.GetEnhancedComPorts");
                    return ports;
                }

                // Step 2: Get WMI information for COM ports
                Dictionary<string, string> wmiInfo = GetComPortDescriptionsFromWMI();

                // Step 3: Get paired Shimmer Bluetooth devices
                Dictionary<string, string> shimmerDevices = GetPairedShimmerDevices();

                // Step 4: Try to map Bluetooth devices to COM ports
                Dictionary<string, string> portToDevice = MapBluetoothToComPorts(shimmerDevices);

                // Step 5: Combine all information
                foreach (string portName in portNames)
                {
                    ComPortInfo info = new ComPortInfo
                    {
                        PortName = portName,
                        Description = wmiInfo.ContainsKey(portName) ? wmiInfo[portName] : null,
                        DeviceName = portToDevice.ContainsKey(portName) ? portToDevice[portName] : null,
                        IsShimmer = portToDevice.ContainsKey(portName)
                    };

                    ports.Add(info);

                    if (info.IsShimmer)
                    {
                        ErrorLogger.LogInfo($"Shimmer device detected: {info.PortName} - {info.DeviceName}", "ComPortHelper.GetEnhancedComPorts");
                    }
                }

                // Sort: Shimmer devices first, then by port number
                ports = ports.OrderByDescending(p => p.IsShimmer)
                            .ThenBy(p => ExtractPortNumber(p.PortName))
                            .ToList();

                ErrorLogger.LogInfo($"Found {ports.Count} COM ports, {ports.Count(p => p.IsShimmer)} Shimmer devices", "ComPortHelper.GetEnhancedComPorts");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("Error in enhanced COM port detection", ex, "ComPortHelper.GetEnhancedComPorts");

                // Fallback: return simple port list
                try
                {
                    string[] portNames = SerialPort.GetPortNames();
                    foreach (string portName in portNames)
                    {
                        ports.Add(new ComPortInfo { PortName = portName });
                    }
                }
                catch (Exception fallbackEx)
                {
                    ErrorLogger.LogError("Fallback COM port detection also failed", fallbackEx, "ComPortHelper.GetEnhancedComPorts");
                }
            }

            return ports;
        }

        /// <summary>
        /// Query Windows WMI to get COM port descriptions
        /// </summary>
        private static Dictionary<string, string> GetComPortDescriptionsFromWMI()
        {
            Dictionary<string, string> descriptions = new Dictionary<string, string>();

            try
            {
                // Query for all devices with COM ports in their caption
                // IMPORTANT: Use 'using' statement to properly dispose ManagementObjectSearcher
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        try
                        {
                            string caption = obj["Caption"]?.ToString();
                            if (string.IsNullOrEmpty(caption))
                                continue;

                            // Extract COM port number from caption like "Device Name (COM5)"
                            int startIdx = caption.LastIndexOf("(COM");
                            int endIdx = caption.LastIndexOf(")");

                            if (startIdx >= 0 && endIdx > startIdx)
                            {
                                string portName = caption.Substring(startIdx + 1, endIdx - startIdx - 1); // "COM5"
                                string description = caption.Substring(0, startIdx).Trim(); // Device name part

                                descriptions[portName] = description;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogWarning($"Error parsing WMI object: {ex.Message}", "ComPortHelper.GetComPortDescriptionsFromWMI");
                        }
                    }

                    ErrorLogger.LogInfo($"WMI found {descriptions.Count} COM port descriptions", "ComPortHelper.GetComPortDescriptionsFromWMI");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogWarning($"WMI COM port query failed (may need admin privileges): {ex.Message}", "ComPortHelper.GetComPortDescriptionsFromWMI");
            }

            return descriptions;
        }

        /// <summary>
        /// Use 32feet.NET to discover paired Bluetooth devices with "Shimmer" in the name
        /// Returns dictionary with Key: BT address (normalized, no separators), Value: device name
        /// </summary>
        private static Dictionary<string, string> GetPairedShimmerDevices()
        {
            Dictionary<string, string> shimmerDevices = new Dictionary<string, string>(); // Key: BT address, Value: device name

            try
            {
                using (BluetoothClient client = new BluetoothClient())
                {
                    // Discover paired devices only (faster, no new pairing attempts)
                    BluetoothDeviceInfo[] devices = client.DiscoverDevices(255, false, true, false, false);

                    foreach (BluetoothDeviceInfo device in devices)
                    {
                        try
                        {
                            string deviceName = device.DeviceName;

                            // Check if this is a Shimmer device
                            if (!string.IsNullOrEmpty(deviceName) &&
                                (deviceName.Contains("Shimmer") || deviceName.Contains("SHIMMER")))
                            {
                                string address = device.DeviceAddress.ToString();
                                // Normalize address for matching (remove separators)
                                string normalizedAddress = address.Replace(":", "").Replace("-", "").ToUpper();
                                shimmerDevices[normalizedAddress] = deviceName;

                                ErrorLogger.LogInfo($"Found paired Shimmer: {deviceName} ({address})", "ComPortHelper.GetPairedShimmerDevices");
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogWarning($"Error processing Bluetooth device: {ex.Message}", "ComPortHelper.GetPairedShimmerDevices");
                        }
                    }
                }

                ErrorLogger.LogInfo($"Bluetooth discovery found {shimmerDevices.Count} Shimmer devices", "ComPortHelper.GetPairedShimmerDevices");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogWarning($"Bluetooth device discovery failed: {ex.Message}", "ComPortHelper.GetPairedShimmerDevices");
            }

            return shimmerDevices;
        }

        /// <summary>
        /// Try to map Bluetooth device addresses to COM ports using WMI
        /// Returns dictionary with Key: COM port, Value: device name
        /// Also populates the reverse mapping (COM port -> BT address) for the pairing dialog
        /// </summary>
        private static Dictionary<string, string> MapBluetoothToComPorts(Dictionary<string, string> shimmerDevices)
        {
            Dictionary<string, string> portMapping = new Dictionary<string, string>(); // Key: COM port, Value: device name

            if (shimmerDevices.Count == 0)
            {
                ErrorLogger.LogInfo("No Shimmer devices to map", "ComPortHelper.MapBluetoothToComPorts");
                return portMapping;
            }

            try
            {
                // Query Win32_SerialPort for Bluetooth COM ports
                // IMPORTANT: Use 'using' statement to properly dispose ManagementObjectSearcher
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_SerialPort WHERE Description LIKE '%Bluetooth%' OR PNPDeviceID LIKE '%BTHENUM%'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        try
                        {
                            string deviceID = obj["DeviceID"]?.ToString(); // e.g., "COM5"
                            string pnpID = obj["PNPDeviceID"]?.ToString();

                            if (string.IsNullOrEmpty(deviceID))
                                continue;

                            // Try to extract Bluetooth address from PNP ID
                            // PNP ID format: BTHENUM\{00001101-0000-1000-8000-00805F9B34FB}_VID&...MAC_ADDRESS...
                            if (!string.IsNullOrEmpty(pnpID))
                            {
                                // Look for MAC address patterns in the PNP ID
                                foreach (var shimmer in shimmerDevices)
                                {
                                    string btAddress = shimmer.Key; // Already normalized

                                    if (pnpID.ToUpper().Contains(btAddress.ToUpper()))
                                    {
                                        portMapping[deviceID] = shimmer.Value;
                                        ErrorLogger.LogInfo($"Mapped {deviceID} to {shimmer.Value}", "ComPortHelper.MapBluetoothToComPorts");
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogWarning($"Error mapping COM port: {ex.Message}", "ComPortHelper.MapBluetoothToComPorts");
                        }
                    }

                    // If mapping failed but we have exactly one Shimmer and one Bluetooth COM port, make educated guess
                    if (portMapping.Count == 0 && shimmerDevices.Count == 1)
                    {
                        int bluetoothPortCount = 0;
                        string lastBluetoothPort = null;

                        // IMPORTANT: Use 'using' statement for nested searcher too
                        using (ManagementObjectSearcher countSearcher = new ManagementObjectSearcher(
                            "SELECT DeviceID FROM Win32_SerialPort WHERE Description LIKE '%Bluetooth%'"))
                        {
                            foreach (ManagementObject obj in countSearcher.Get())
                            {
                                lastBluetoothPort = obj["DeviceID"]?.ToString();
                                bluetoothPortCount++;
                            }
                        }

                        if (bluetoothPortCount == 1 && !string.IsNullOrEmpty(lastBluetoothPort))
                        {
                            portMapping[lastBluetoothPort] = shimmerDevices.Values.First();
                            ErrorLogger.LogInfo($"Made educated guess: {lastBluetoothPort} -> {shimmerDevices.Values.First()}", "ComPortHelper.MapBluetoothToComPorts");
                        }
                    }

                    ErrorLogger.LogInfo($"Successfully mapped {portMapping.Count} COM ports to Shimmer devices", "ComPortHelper.MapBluetoothToComPorts");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogWarning($"COM port mapping via WMI failed: {ex.Message}", "ComPortHelper.MapBluetoothToComPorts");
            }

            return portMapping;
        }

        /// <summary>
        /// Get a mapping of Bluetooth addresses to COM ports for paired Shimmer devices
        /// This is used by the Bluetooth pairing dialog to show COM port assignments
        /// Returns dictionary with Key: Normalized BT address (no separators, uppercase), Value: COM port
        /// </summary>
        public static Dictionary<string, string> GetBluetoothAddressToComPortMapping()
        {
            Dictionary<string, string> addressToPort = new Dictionary<string, string>();

            try
            {
                // Get paired Shimmer devices
                Dictionary<string, string> shimmerDevices = GetPairedShimmerDevices();

                if (shimmerDevices.Count == 0)
                {
                    return addressToPort;
                }

                // Query Win32_SerialPort for Bluetooth COM ports
                // IMPORTANT: Use 'using' statement to properly dispose ManagementObjectSearcher
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_SerialPort WHERE Description LIKE '%Bluetooth%' OR PNPDeviceID LIKE '%BTHENUM%'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        try
                        {
                            string deviceID = obj["DeviceID"]?.ToString(); // e.g., "COM5"
                            string pnpID = obj["PNPDeviceID"]?.ToString();

                            if (string.IsNullOrEmpty(deviceID) || string.IsNullOrEmpty(pnpID))
                                continue;

                            // Look for MAC address patterns in the PNP ID
                            foreach (var shimmer in shimmerDevices)
                            {
                                string btAddress = shimmer.Key; // Already normalized

                                if (pnpID.ToUpper().Contains(btAddress))
                                {
                                    addressToPort[btAddress] = deviceID;
                                    ErrorLogger.LogInfo($"Address mapping: {btAddress} -> {deviceID}", "ComPortHelper.GetBluetoothAddressToComPortMapping");
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogWarning($"Error creating address mapping: {ex.Message}", "ComPortHelper.GetBluetoothAddressToComPortMapping");
                        }
                    }

                    ErrorLogger.LogInfo($"Created {addressToPort.Count} Bluetooth address to COM port mappings", "ComPortHelper.GetBluetoothAddressToComPortMapping");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogWarning($"Failed to create Bluetooth address to COM port mapping: {ex.Message}", "ComPortHelper.GetBluetoothAddressToComPortMapping");
            }

            return addressToPort;
        }

        /// <summary>
        /// Extract numeric port number from port name for sorting
        /// </summary>
        private static int ExtractPortNumber(string portName)
        {
            if (string.IsNullOrEmpty(portName))
                return 0;

            // Extract digits from "COM5" -> 5
            string digits = new string(portName.Where(char.IsDigit).ToArray());

            if (int.TryParse(digits, out int portNumber))
                return portNumber;

            return 0;
        }
    }
}
