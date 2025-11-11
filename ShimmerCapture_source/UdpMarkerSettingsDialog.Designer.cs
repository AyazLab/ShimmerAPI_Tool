namespace ShimmerAPI
{
    partial class UdpMarkerSettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBoxEnableMarkers = new System.Windows.Forms.CheckBox();
            this.groupBoxInbound = new System.Windows.Forms.GroupBox();
            this.textBoxInboundPort = new System.Windows.Forms.TextBox();
            this.labelInboundPort = new System.Windows.Forms.Label();
            this.labelInboundInfo = new System.Windows.Forms.Label();
            this.groupBoxOutbound = new System.Windows.Forms.GroupBox();
            this.buttonTest = new System.Windows.Forms.Button();
            this.textBoxOutboundPort = new System.Windows.Forms.TextBox();
            this.labelOutboundPort = new System.Windows.Forms.Label();
            this.textBoxOutboundIP = new System.Windows.Forms.TextBox();
            this.labelOutboundIP = new System.Windows.Forms.Label();
            this.labelOutboundInfo = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxInbound.SuspendLayout();
            this.groupBoxOutbound.SuspendLayout();
            this.SuspendLayout();
            //
            // checkBoxEnableMarkers
            //
            this.checkBoxEnableMarkers.AutoSize = true;
            this.checkBoxEnableMarkers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEnableMarkers.Location = new System.Drawing.Point(12, 12);
            this.checkBoxEnableMarkers.Name = "checkBoxEnableMarkers";
            this.checkBoxEnableMarkers.Size = new System.Drawing.Size(180, 19);
            this.checkBoxEnableMarkers.TabIndex = 0;
            this.checkBoxEnableMarkers.Text = "Enable UDP Markers";
            this.checkBoxEnableMarkers.UseVisualStyleBackColor = true;
            //
            // groupBoxInbound
            //
            this.groupBoxInbound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInbound.Controls.Add(this.labelInboundInfo);
            this.groupBoxInbound.Controls.Add(this.textBoxInboundPort);
            this.groupBoxInbound.Controls.Add(this.labelInboundPort);
            this.groupBoxInbound.Location = new System.Drawing.Point(12, 40);
            this.groupBoxInbound.Name = "groupBoxInbound";
            this.groupBoxInbound.Size = new System.Drawing.Size(460, 95);
            this.groupBoxInbound.TabIndex = 1;
            this.groupBoxInbound.TabStop = false;
            this.groupBoxInbound.Text = "Inbound (Receiving Markers)";
            //
            // textBoxInboundPort
            //
            this.textBoxInboundPort.Location = new System.Drawing.Point(50, 25);
            this.textBoxInboundPort.Name = "textBoxInboundPort";
            this.textBoxInboundPort.Size = new System.Drawing.Size(80, 20);
            this.textBoxInboundPort.TabIndex = 1;
            //
            // labelInboundPort
            //
            this.labelInboundPort.AutoSize = true;
            this.labelInboundPort.Location = new System.Drawing.Point(10, 28);
            this.labelInboundPort.Name = "labelInboundPort";
            this.labelInboundPort.Size = new System.Drawing.Size(29, 13);
            this.labelInboundPort.TabIndex = 0;
            this.labelInboundPort.Text = "Port:";
            //
            // labelInboundInfo
            //
            this.labelInboundInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInboundInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelInboundInfo.Location = new System.Drawing.Point(10, 55);
            this.labelInboundInfo.Name = "labelInboundInfo";
            this.labelInboundInfo.Size = new System.Drawing.Size(440, 30);
            this.labelInboundInfo.TabIndex = 2;
            this.labelInboundInfo.Text = "Port where the application listens for incoming UDP markers. Changing this requir" +
    "es restarting the listener.";
            //
            // groupBoxOutbound
            //
            this.groupBoxOutbound.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOutbound.Controls.Add(this.labelOutboundInfo);
            this.groupBoxOutbound.Controls.Add(this.buttonTest);
            this.groupBoxOutbound.Controls.Add(this.textBoxOutboundPort);
            this.groupBoxOutbound.Controls.Add(this.labelOutboundPort);
            this.groupBoxOutbound.Controls.Add(this.textBoxOutboundIP);
            this.groupBoxOutbound.Controls.Add(this.labelOutboundIP);
            this.groupBoxOutbound.Location = new System.Drawing.Point(12, 145);
            this.groupBoxOutbound.Name = "groupBoxOutbound";
            this.groupBoxOutbound.Size = new System.Drawing.Size(460, 135);
            this.groupBoxOutbound.TabIndex = 2;
            this.groupBoxOutbound.TabStop = false;
            this.groupBoxOutbound.Text = "Outbound (Sending Markers)";
            //
            // buttonTest
            //
            this.buttonTest.Location = new System.Drawing.Point(260, 48);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(100, 23);
            this.buttonTest.TabIndex = 5;
            this.buttonTest.Text = "Test Connection";
            this.buttonTest.UseVisualStyleBackColor = true;
            //
            // textBoxOutboundPort
            //
            this.textBoxOutboundPort.Location = new System.Drawing.Point(50, 50);
            this.textBoxOutboundPort.Name = "textBoxOutboundPort";
            this.textBoxOutboundPort.Size = new System.Drawing.Size(80, 20);
            this.textBoxOutboundPort.TabIndex = 3;
            //
            // labelOutboundPort
            //
            this.labelOutboundPort.AutoSize = true;
            this.labelOutboundPort.Location = new System.Drawing.Point(10, 53);
            this.labelOutboundPort.Name = "labelOutboundPort";
            this.labelOutboundPort.Size = new System.Drawing.Size(29, 13);
            this.labelOutboundPort.TabIndex = 2;
            this.labelOutboundPort.Text = "Port:";
            //
            // textBoxOutboundIP
            //
            this.textBoxOutboundIP.Location = new System.Drawing.Point(50, 25);
            this.textBoxOutboundIP.Name = "textBoxOutboundIP";
            this.textBoxOutboundIP.Size = new System.Drawing.Size(150, 20);
            this.textBoxOutboundIP.TabIndex = 1;
            //
            // labelOutboundIP
            //
            this.labelOutboundIP.AutoSize = true;
            this.labelOutboundIP.Location = new System.Drawing.Point(10, 28);
            this.labelOutboundIP.Name = "labelOutboundIP";
            this.labelOutboundIP.Size = new System.Drawing.Size(20, 13);
            this.labelOutboundIP.TabIndex = 0;
            this.labelOutboundIP.Text = "IP:";
            //
            // labelOutboundInfo
            //
            this.labelOutboundInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelOutboundInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelOutboundInfo.Location = new System.Drawing.Point(10, 80);
            this.labelOutboundInfo.Name = "labelOutboundInfo";
            this.labelOutboundInfo.Size = new System.Drawing.Size(440, 45);
            this.labelOutboundInfo.TabIndex = 6;
            this.labelOutboundInfo.Text = "IP address and port where manual markers will be sent. Automatic markers (start/" +
    "stop recording) are sent to this destination. Use 127.0.0.1 for localhost.";
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(316, 290);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(397, 290);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            //
            // UdpMarkerSettingsDialog
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(484, 325);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxOutbound);
            this.Controls.Add(this.groupBoxInbound);
            this.Controls.Add(this.checkBoxEnableMarkers);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UdpMarkerSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "UDP Marker Settings";
            this.groupBoxInbound.ResumeLayout(false);
            this.groupBoxInbound.PerformLayout();
            this.groupBoxOutbound.ResumeLayout(false);
            this.groupBoxOutbound.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxEnableMarkers;
        private System.Windows.Forms.GroupBox groupBoxInbound;
        private System.Windows.Forms.TextBox textBoxInboundPort;
        private System.Windows.Forms.Label labelInboundPort;
        private System.Windows.Forms.Label labelInboundInfo;
        private System.Windows.Forms.GroupBox groupBoxOutbound;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.TextBox textBoxOutboundPort;
        private System.Windows.Forms.Label labelOutboundPort;
        private System.Windows.Forms.TextBox textBoxOutboundIP;
        private System.Windows.Forms.Label labelOutboundIP;
        private System.Windows.Forms.Label labelOutboundInfo;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
