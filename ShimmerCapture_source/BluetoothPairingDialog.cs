using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using ShimmerAPI;

/*
 * Copyright (c) 2025, Drexel University
 * All rights reserved
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:

 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 *       copyright notice, this list of conditions and the following
 *       disclaimer in the documentation and/or other materials provided
 *       with the distribution.
 *     * Neither the name of Shimmer Research, Ltd. nor the names of its
 *       contributors may be used to endorse or promote products derived
 *       from this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * @author Cathy Swanton, Mike Healy, Jong Chern Lim
 * @date   October, 2014
 */

namespace ShimmerAPI
{
    /// <summary>
    /// Dialog for discovering and pairing Bluetooth Shimmer devices
    /// Shows available devices, their paired status, and assigned COM ports
    /// </summary>
    public partial class BluetoothPairingDialog : Form
    {
        private const string PIN_CODE = "1234";
        private BackgroundWorker discoveryWorker;
        private List<BluetoothDeviceItem> discoveredDevices;

        public BluetoothPairingDialog()
        {
            InitializeComponent();
            discoveredDevices = new List<BluetoothDeviceItem>();
            InitializeDiscoveryWorker();

            // Start discovery automatically when dialog opens
            StartDiscovery();
        }

        /// <summary>
        /// Initialize background worker for asynchronous device discovery
        /// </summary>
        private void InitializeDiscoveryWorker()
        {
            discoveryWorker = new BackgroundWorker();
            discoveryWorker.WorkerReportsProgress = true;
            discoveryWorker.WorkerSupportsCancellation = true;
            discoveryWorker.DoWork += DiscoveryWorker_DoWork;
            discoveryWorker.ProgressChanged += DiscoveryWorker_ProgressChanged;
            discoveryWorker.RunWorkerCompleted += DiscoveryWorker_RunWorkerCompleted;
        }

        /// <summary>
        /// Start Bluetooth device discovery
        /// </summary>
        private void StartDiscovery()
        {
            if (discoveryWorker.IsBusy)
                return;

            discoveredDevices.Clear();
            listViewDevices.Items.Clear();
            buttonRefresh.Enabled = false;
            buttonPair.Enabled = false;
            labelStatus.Text = "Discovering Bluetooth devices...";
            labelStatus.ForeColor = Color.Blue;

            discoveryWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Background worker: Discover Bluetooth devices
        /// </summary>
        private void DiscoveryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<BluetoothDeviceItem> devices = new List<BluetoothDeviceItem>();

            try
            {
                ErrorLogger.LogInfo("Starting Bluetooth device discovery", "BluetoothPairingDialog.DiscoveryWorker_DoWork");

                using (BluetoothClient client = new BluetoothClient())
                {
                    // Discover all devices (paired and unpaired)
                    // Parameters: maxDevices, authenticated, remembered, unknown, discoverableOnly
                    BluetoothDeviceInfo[] allDevices = client.DiscoverDevices(255, true, true, true, false);

                    discoveryWorker.ReportProgress(0, $"Found {allDevices.Length} Bluetooth devices");

                    // Get COM port mapping for paired devices
                    Dictionary<string, string> portMapping = GetComPortMapping();

                    foreach (BluetoothDeviceInfo device in allDevices)
                    {
                        if (discoveryWorker.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        try
                        {
                            string deviceName = device.DeviceName;

                            // Filter for Shimmer devices only
                            if (string.IsNullOrEmpty(deviceName) ||
                                (!deviceName.Contains("Shimmer") && !deviceName.Contains("SHIMMER")))
                            {
                                continue;
                            }

                            BluetoothDeviceItem item = new BluetoothDeviceItem
                            {
                                DeviceInfo = device,
                                DeviceName = deviceName,
                                BluetoothAddress = device.DeviceAddress.ToString(),
                                IsPaired = device.Authenticated,
                                IsConnected = device.Connected,
                                ComPort = null
                            };

                            // Try to find COM port for this device
                            string normalizedAddress = device.DeviceAddress.ToString().Replace(":", "").Replace("-", "").ToUpper();
                            if (portMapping.ContainsKey(normalizedAddress))
                            {
                                item.ComPort = portMapping[normalizedAddress];
                            }

                            devices.Add(item);
                            discoveryWorker.ReportProgress(0, $"Found Shimmer: {deviceName}");

                            ErrorLogger.LogInfo($"Discovered Shimmer device: {deviceName} ({item.BluetoothAddress}) - Paired: {item.IsPaired}, COM: {item.ComPort ?? "N/A"}",
                                "BluetoothPairingDialog.DiscoveryWorker_DoWork");
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogWarning($"Error processing device: {ex.Message}", "BluetoothPairingDialog.DiscoveryWorker_DoWork");
                        }
                    }
                }

                e.Result = devices;
                ErrorLogger.LogInfo($"Discovery complete. Found {devices.Count} Shimmer devices", "BluetoothPairingDialog.DiscoveryWorker_DoWork");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("Bluetooth discovery failed", ex, "BluetoothPairingDialog.DiscoveryWorker_DoWork");
                e.Result = new Exception("Bluetooth discovery failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Background worker: Progress update
        /// </summary>
        private void DiscoveryWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string message)
            {
                labelStatus.Text = message;
            }
        }

        /// <summary>
        /// Background worker: Discovery completed
        /// </summary>
        private void DiscoveryWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonRefresh.Enabled = true;

            if (e.Cancelled)
            {
                labelStatus.Text = "Discovery cancelled";
                labelStatus.ForeColor = Color.Gray;
                return;
            }

            if (e.Result is Exception ex)
            {
                labelStatus.Text = "Discovery failed: " + ex.Message;
                labelStatus.ForeColor = Color.Red;
                MessageBox.Show("Bluetooth discovery failed:\n\n" + ex.Message +
                    "\n\nMake sure Bluetooth is enabled on your computer.",
                    "Discovery Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (e.Result is List<BluetoothDeviceItem> devices)
            {
                discoveredDevices = devices;
                PopulateDeviceList();

                if (devices.Count == 0)
                {
                    labelStatus.Text = "No Shimmer devices found";
                    labelStatus.ForeColor = Color.Orange;
                }
                else
                {
                    labelStatus.Text = $"Found {devices.Count} Shimmer device(s)";
                    labelStatus.ForeColor = Color.Green;
                }
            }
        }

        /// <summary>
        /// Populate the ListView with discovered devices
        /// </summary>
        private void PopulateDeviceList()
        {
            listViewDevices.Items.Clear();

            foreach (var device in discoveredDevices)
            {
                ListViewItem item = new ListViewItem(device.DeviceName);
                item.SubItems.Add(device.BluetoothAddress);
                item.SubItems.Add(device.IsPaired ? "Yes" : "No");
                item.SubItems.Add(device.ComPort ?? "N/A");
                item.Tag = device;

                // Color code based on pairing status
                if (device.IsPaired)
                {
                    item.ForeColor = Color.Green;
                }
                else
                {
                    item.ForeColor = Color.Blue;
                }

                listViewDevices.Items.Add(item);
            }
        }

        /// <summary>
        /// Get COM port mapping for paired Bluetooth devices
        /// Returns dictionary with Key: Normalized BT address, Value: COM port
        /// </summary>
        private Dictionary<string, string> GetComPortMapping()
        {
            try
            {
                // Use the ComPortHelper to get the mapping
                return ComPortHelper.GetBluetoothAddressToComPortMapping();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogWarning($"Failed to get COM port mapping: {ex.Message}", "BluetoothPairingDialog.GetComPortMapping");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Refresh button click handler
        /// </summary>
        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            StartDiscovery();
        }

        /// <summary>
        /// Pair button click handler
        /// </summary>
        private void buttonPair_Click(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to pair.", "No Device Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListViewItem selectedItem = listViewDevices.SelectedItems[0];
            BluetoothDeviceItem device = selectedItem.Tag as BluetoothDeviceItem;

            if (device == null)
                return;

            if (device.IsPaired)
            {
                MessageBox.Show($"{device.DeviceName} is already paired.", "Already Paired",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Confirm pairing
            DialogResult result = MessageBox.Show(
                $"Pair with {device.DeviceName}?\n\n" +
                $"Bluetooth Address: {device.BluetoothAddress}\n" +
                $"PIN Code: {PIN_CODE}\n\n" +
                "The pairing process may take a few seconds.",
                "Confirm Pairing",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // Perform pairing
            PairDevice(device);
        }

        /// <summary>
        /// Pair with a Bluetooth device
        /// </summary>
        private void PairDevice(BluetoothDeviceItem device)
        {
            try
            {
                labelStatus.Text = $"Pairing with {device.DeviceName}...";
                labelStatus.ForeColor = Color.Blue;
                buttonPair.Enabled = false;
                buttonRefresh.Enabled = false;
                Application.DoEvents();

                ErrorLogger.LogInfo($"Attempting to pair with {device.DeviceName} ({device.BluetoothAddress})",
                    "BluetoothPairingDialog.PairDevice");

                // Set the PIN for pairing
                BluetoothSecurity.SetPin(device.DeviceInfo.DeviceAddress, PIN_CODE);

                // Attempt pairing
                bool paired = BluetoothSecurity.PairRequest(device.DeviceInfo.DeviceAddress, PIN_CODE);

                if (paired)
                {
                    labelStatus.Text = $"Successfully paired with {device.DeviceName}";
                    labelStatus.ForeColor = Color.Green;

                    ErrorLogger.LogInfo($"Successfully paired with {device.DeviceName}", "BluetoothPairingDialog.PairDevice");

                    MessageBox.Show(
                        $"Successfully paired with {device.DeviceName}!\n\n" +
                        "Windows will now assign a COM port to this device.\n" +
                        "Click Refresh to see the assigned COM port.",
                        "Pairing Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // IMPROVEMENT: Instead of blocking with Thread.Sleep, just enable the Refresh button
                    // User can manually refresh when ready
                    buttonRefresh.Enabled = true;
                    buttonPair.Enabled = false;
                }
                else
                {
                    labelStatus.Text = $"Failed to pair with {device.DeviceName}";
                    labelStatus.ForeColor = Color.Red;

                    ErrorLogger.LogWarning($"Failed to pair with {device.DeviceName}", "BluetoothPairingDialog.PairDevice");

                    MessageBox.Show(
                        $"Failed to pair with {device.DeviceName}.\n\n" +
                        "Please make sure:\n" +
                        "1. The device is powered on\n" +
                        "2. The device is in range\n" +
                        "3. The device is not already paired with another computer\n" +
                        "4. Bluetooth is enabled on your computer",
                        "Pairing Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    buttonPair.Enabled = true;
                    buttonRefresh.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Pairing error";
                labelStatus.ForeColor = Color.Red;
                buttonPair.Enabled = true;
                buttonRefresh.Enabled = true;

                ErrorLogger.LogError("Pairing error", ex, "BluetoothPairingDialog.PairDevice");

                MessageBox.Show(
                    $"Error during pairing:\n\n{ex.Message}",
                    "Pairing Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ListView selection changed handler
        /// </summary>
        private void listViewDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewDevices.SelectedItems[0];
                BluetoothDeviceItem device = selectedItem.Tag as BluetoothDeviceItem;

                if (device != null)
                {
                    // Enable pair button only for unpaired devices
                    buttonPair.Enabled = !device.IsPaired;
                }
            }
            else
            {
                buttonPair.Enabled = false;
            }
        }

        /// <summary>
        /// Close button click handler
        /// </summary>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Form closing event handler
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // CRITICAL FIX: Cancel discovery and wait for worker to complete to prevent race conditions
            if (discoveryWorker != null && discoveryWorker.IsBusy)
            {
                try
                {
                    discoveryWorker.CancelAsync();

                    // Wait for worker to complete (with timeout to prevent hang)
                    int timeout = 0;
                    while (discoveryWorker.IsBusy && timeout < 3000)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                        timeout += 100;
                    }

                    if (discoveryWorker.IsBusy)
                    {
                        ErrorLogger.LogWarning("BackgroundWorker did not stop within timeout", "BluetoothPairingDialog.OnFormClosing");
                    }
                    else
                    {
                        ErrorLogger.LogInfo("BackgroundWorker stopped successfully before form close", "BluetoothPairingDialog.OnFormClosing");
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("Error waiting for BackgroundWorker to complete", ex, "BluetoothPairingDialog.OnFormClosing");
                }
            }

            // Dispose BackgroundWorker to prevent memory leak
            if (discoveryWorker != null)
            {
                try
                {
                    discoveryWorker.Dispose();
                    discoveryWorker = null;
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("Error disposing BackgroundWorker", ex, "BluetoothPairingDialog.OnFormClosing");
                }
            }

            base.OnFormClosing(e);
        }
    }

    /// <summary>
    /// Represents a discovered Bluetooth device
    /// </summary>
    internal class BluetoothDeviceItem
    {
        public BluetoothDeviceInfo DeviceInfo { get; set; }
        public string DeviceName { get; set; }
        public string BluetoothAddress { get; set; }
        public bool IsPaired { get; set; }
        public bool IsConnected { get; set; }
        public string ComPort { get; set; }
    }
}
