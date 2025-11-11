using System;
using System.Net;
using System.Windows.Forms;

namespace ShimmerAPI
{
    /// <summary>
    /// Dialog for configuring UDP marker settings (inbound and outbound)
    /// </summary>
    public partial class UdpMarkerSettingsDialog : Form
    {
        public bool MarkersEnabled { get; private set; }
        public int InboundPort { get; private set; }
        public string OutboundIP { get; private set; }
        public int OutboundPort { get; private set; }

        public UdpMarkerSettingsDialog(bool enabled, int inboundPort, string outboundIP, int outboundPort)
        {
            InitializeComponent();

            // Set initial values
            checkBoxEnableMarkers.Checked = enabled;
            textBoxInboundPort.Text = inboundPort.ToString();
            textBoxOutboundIP.Text = outboundIP;
            textBoxOutboundPort.Text = outboundPort.ToString();

            // Enable/disable based on checkbox
            UpdateControlState();

            // Wire up events
            checkBoxEnableMarkers.CheckedChanged += (s, e) => UpdateControlState();
            buttonOK.Click += ButtonOK_Click;
            buttonCancel.Click += ButtonCancel_Click;
            buttonTest.Click += ButtonTest_Click;
        }

        private void UpdateControlState()
        {
            bool enabled = checkBoxEnableMarkers.Checked;
            textBoxInboundPort.Enabled = enabled;
            textBoxOutboundIP.Enabled = enabled;
            textBoxOutboundPort.Enabled = enabled;
            buttonTest.Enabled = enabled;
        }

        private bool ValidateSettings()
        {
            if (!checkBoxEnableMarkers.Checked)
            {
                // If disabled, no validation needed
                return true;
            }

            // Validate inbound port
            if (!int.TryParse(textBoxInboundPort.Text, out int inPort) || inPort < 1 || inPort > 65535)
            {
                MessageBox.Show("Inbound port must be a number between 1 and 65535.",
                    "Invalid Inbound Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxInboundPort.Focus();
                return false;
            }

            // Validate outbound IP
            if (!IPAddress.TryParse(textBoxOutboundIP.Text, out _))
            {
                MessageBox.Show("Invalid IP address format. Please enter a valid IPv4 address (e.g., 127.0.0.1).",
                    "Invalid IP Address", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxOutboundIP.Focus();
                return false;
            }

            // Validate outbound port
            if (!int.TryParse(textBoxOutboundPort.Text, out int outPort) || outPort < 1 || outPort > 65535)
            {
                MessageBox.Show("Outbound port must be a number between 1 and 65535.",
                    "Invalid Outbound Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxOutboundPort.Focus();
                return false;
            }

            return true;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (!ValidateSettings())
                return;

            // Store values
            MarkersEnabled = checkBoxEnableMarkers.Checked;

            if (MarkersEnabled)
            {
                InboundPort = int.Parse(textBoxInboundPort.Text);
                OutboundIP = textBoxOutboundIP.Text;
                OutboundPort = int.Parse(textBoxOutboundPort.Text);
            }
            else
            {
                // Keep previous values when disabled
                InboundPort = int.TryParse(textBoxInboundPort.Text, out int p) ? p : 5501;
                OutboundIP = textBoxOutboundIP.Text;
                OutboundPort = int.TryParse(textBoxOutboundPort.Text, out int op) ? op : 5501;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButtonTest_Click(object sender, EventArgs e)
        {
            if (!ValidateSettings())
                return;

            // Test sending a UDP packet
            try
            {
                int port = int.Parse(textBoxOutboundPort.Text);
                string ip = textBoxOutboundIP.Text;

                using (var udpClient = new System.Net.Sockets.UdpClient())
                {
                    var endpoint = new System.Net.IPEndPoint(IPAddress.Parse(ip), port);
                    byte[] testData = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }; // Test marker

                    udpClient.Send(testData, testData.Length, endpoint);

                    MessageBox.Show($"Test marker sent successfully to {ip}:{port}",
                        "Test Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Test failed: {ex.Message}\n\nPlease check your network settings.",
                    "Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
