namespace ShimmerAPI
{
    partial class Control
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
            if (disposing)
            {
                // CRITICAL FIX: Dispose all resources to prevent memory leaks
                try
                {
                    // Dispose ShimmerDevice and unsubscribe events
                    if (ShimmerDevice != null)
                    {
                        try
                        {
                            ShimmerDevice.UICallback -= this.HandleEvent;
                            ShimmerDevice.Dispose();
                            ShimmerDevice = null;
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Error disposing ShimmerDevice in form Dispose", ex, "Control.Dispose");
                        }
                    }

                    // Dispose UDPListener
                    if (udpListener != null)
                    {
                        try
                        {
                            udpListener.StopListener();
                            udpListener = null;
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Error disposing UDPListener in form Dispose", ex, "Control.Dispose");
                        }
                    }

                    // Dispose WriteToFile (Logging)
                    if (WriteToFile != null)
                    {
                        try
                        {
                            WriteToFile.CloseFile();
                            WriteToFile = null;
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Error disposing WriteToFile in form Dispose", ex, "Control.Dispose");
                        }
                    }

                    // Dispose markerLogger
                    if (markerLogger != null)
                    {
                        try
                        {
                            markerLogger.Dispose();
                            markerLogger = null;
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Error disposing markerLogger in form Dispose", ex, "Control.Dispose");
                        }
                    }

                    // Dispose dead SerialPort field (cleanup)
                    if (SerialPort != null)
                    {
                        try
                        {
                            if (SerialPort.IsOpen)
                            {
                                SerialPort.Close();
                            }
                            SerialPort.Dispose();
                            SerialPort = null;
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Error disposing SerialPort in form Dispose", ex, "Control.Dispose");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("Error during form disposal cleanup", ex, "Control.Dispose");
                }

                if (components != null)
                {
                    components.Dispose();
                }
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Control));
            this.buttonStreamandLog = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.textBoxShimmerState = new System.Windows.Forms.TextBox();
            this.labelState = new System.Windows.Forms.Label();
            this.groupBoxGraph1 = new System.Windows.Forms.GroupBox();
            this.ZedGraphControl1 = new ZedGraph.ZedGraphControl();
            this.groupBoxGraph2 = new System.Windows.Forms.GroupBox();
            this.groupBoxGraph3 = new System.Windows.Forms.GroupBox();
            this.comboBoxComPorts = new System.Windows.Forms.ComboBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripItemFile = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItemQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripItemTools = new System.Windows.Forms.ToolStripDropDownButton();
            this.configureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemSaveToCSV = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemShow3DOrientation = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemOpenDataFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemOpenLogFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.configureSignalDisplayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configureUDPMarkersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pairBluetoothDeviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bluetoothSetupHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDialog = new System.Windows.Forms.OpenFileDialog();
            this.buttonAddGraph = new System.Windows.Forms.Button();
            this.buttonRemoveGraph = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonReload = new System.Windows.Forms.Button();
            this.labelPRR = new System.Windows.Forms.Label();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.textBoxLeadOffStatus1 = new System.Windows.Forms.TextBox();
            this.textBoxLeadOffStatus3 = new System.Windows.Forms.TextBox();
            this.textBoxLeadOffStatus2 = new System.Windows.Forms.TextBox();
            this.textBoxLeadOffStatus4 = new System.Windows.Forms.TextBox();
            this.textBoxLeadOffStatus5 = new System.Windows.Forms.TextBox();
            this.labelLeadOffStatus2 = new System.Windows.Forms.Label();
            this.labelLeadOffStatus1 = new System.Windows.Forms.Label();
            this.labelLeadOffStatus5 = new System.Windows.Forms.Label();
            this.labelLeadOffStatus3 = new System.Windows.Forms.Label();
            this.labelLeadOffStatus4 = new System.Windows.Forms.Label();
            this.labelExGLeadOffDetection = new System.Windows.Forms.Label();
            this.buttonReadDirectory = new System.Windows.Forms.Button();
            this.checkBoxTSACheck = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBoxSubj = new System.Windows.Forms.TextBox();
            this.groupBoxExGLeadOff = new System.Windows.Forms.GroupBox();
            this.groupBoxPPG = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelUDPStatus = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBoxManualMarker = new System.Windows.Forms.GroupBox();
            this.labelMarkerIP = new System.Windows.Forms.Label();
            this.textBoxMarkerIP = new System.Windows.Forms.TextBox();
            this.labelMarkerPort = new System.Windows.Forms.Label();
            this.textBoxMarkerPort = new System.Windows.Forms.TextBox();
            this.buttonSendMarker = new System.Windows.Forms.Button();
            this.button_ConfigureSignals = new System.Windows.Forms.Button();
            this.groupBoxGraph1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBoxExGLeadOff.SuspendLayout();
            this.groupBoxPPG.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBoxManualMarker.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStreamandLog
            // 
            this.buttonStreamandLog.Enabled = false;
            this.buttonStreamandLog.Location = new System.Drawing.Point(183, 91);
            this.buttonStreamandLog.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.buttonStreamandLog.Name = "buttonStreamandLog";
            this.buttonStreamandLog.Size = new System.Drawing.Size(251, 48);
            this.buttonStreamandLog.TabIndex = 2;
            this.buttonStreamandLog.Text = "Record";
            this.buttonStreamandLog.UseVisualStyleBackColor = true;
            this.buttonStreamandLog.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(394, 52);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(251, 54);
            this.buttonConnect.TabIndex = 1;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // textBoxShimmerState
            // 
            this.textBoxShimmerState.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxShimmerState.Enabled = false;
            this.textBoxShimmerState.Location = new System.Drawing.Point(220, 43);
            this.textBoxShimmerState.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.textBoxShimmerState.Name = "textBoxShimmerState";
            this.textBoxShimmerState.Size = new System.Drawing.Size(267, 31);
            this.textBoxShimmerState.TabIndex = 122;
            this.textBoxShimmerState.Text = "None";
            // 
            // labelState
            // 
            this.labelState.AutoSize = true;
            this.labelState.Location = new System.Drawing.Point(18, 42);
            this.labelState.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelState.Name = "labelState";
            this.labelState.Size = new System.Drawing.Size(183, 32);
            this.labelState.TabIndex = 121;
            this.labelState.Text = "Device State:";
            // 
            // groupBoxGraph1
            // 
            this.groupBoxGraph1.Controls.Add(this.ZedGraphControl1);
            this.groupBoxGraph1.Location = new System.Drawing.Point(31, 480);
            this.groupBoxGraph1.Name = "groupBoxGraph1";
            this.groupBoxGraph1.Size = new System.Drawing.Size(450, 300);
            this.groupBoxGraph1.TabIndex = 122;
            this.groupBoxGraph1.TabStop = false;
            this.groupBoxGraph1.Text = "Graph 1";
            // 
            // ZedGraphControl1
            // 
            this.ZedGraphControl1.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.ZedGraphControl1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ZedGraphControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ZedGraphControl1.Location = new System.Drawing.Point(3, 34);
            this.ZedGraphControl1.Margin = new System.Windows.Forms.Padding(11, 9, 11, 9);
            this.ZedGraphControl1.Name = "ZedGraphControl1";
            this.ZedGraphControl1.ScrollGrace = 0D;
            this.ZedGraphControl1.ScrollMaxX = 0D;
            this.ZedGraphControl1.ScrollMaxY = 0D;
            this.ZedGraphControl1.ScrollMaxY2 = 0D;
            this.ZedGraphControl1.ScrollMinX = 0D;
            this.ZedGraphControl1.ScrollMinY = 0D;
            this.ZedGraphControl1.ScrollMinY2 = 0D;
            this.ZedGraphControl1.Size = new System.Drawing.Size(444, 263);
            this.ZedGraphControl1.TabIndex = 0;
            this.ZedGraphControl1.Load += new System.EventHandler(this.ZedGraphControl1_Load);
            // 
            // groupBoxGraph2
            // 
            this.groupBoxGraph2.Location = new System.Drawing.Point(490, 480);
            this.groupBoxGraph2.Name = "groupBoxGraph2";
            this.groupBoxGraph2.Size = new System.Drawing.Size(450, 300);
            this.groupBoxGraph2.TabIndex = 123;
            this.groupBoxGraph2.TabStop = false;
            this.groupBoxGraph2.Text = "Graph 2";
            this.groupBoxGraph2.Visible = false;
            // 
            // groupBoxGraph3
            // 
            this.groupBoxGraph3.Location = new System.Drawing.Point(949, 480);
            this.groupBoxGraph3.Name = "groupBoxGraph3";
            this.groupBoxGraph3.Size = new System.Drawing.Size(450, 300);
            this.groupBoxGraph3.TabIndex = 124;
            this.groupBoxGraph3.TabStop = false;
            this.groupBoxGraph3.Text = "Graph 3";
            this.groupBoxGraph3.Visible = false;
            // 
            // comboBoxComPorts
            // 
            this.comboBoxComPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxComPorts.FormattingEnabled = true;
            this.comboBoxComPorts.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.comboBoxComPorts.Location = new System.Drawing.Point(41, 61);
            this.comboBoxComPorts.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.comboBoxComPorts.Name = "comboBoxComPorts";
            this.comboBoxComPorts.Size = new System.Drawing.Size(248, 39);
            this.comboBoxComPorts.Sorted = true;
            this.comboBoxComPorts.TabIndex = 0;
            this.comboBoxComPorts.SelectedIndexChanged += new System.EventHandler(this.comboBoxComPorts_SelectedIndexChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripItemFile,
            this.toolStripItemTools,
            this.toolStripSplitButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.toolStrip1.Size = new System.Drawing.Size(2172, 52);
            this.toolStrip1.TabIndex = 38;
            this.toolStrip1.Text = "Check For Updates";
            this.toolStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip1_ItemClicked);
            // 
            // toolStripItemFile
            // 
            this.toolStripItemFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemQuit});
            this.toolStripItemFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripItemFile.Name = "toolStripItemFile";
            this.toolStripItemFile.Size = new System.Drawing.Size(89, 45);
            this.toolStripItemFile.Text = "File";
            // 
            // ToolStripMenuItemQuit
            // 
            this.ToolStripMenuItemQuit.Name = "ToolStripMenuItemQuit";
            this.ToolStripMenuItemQuit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.ToolStripMenuItemQuit.ShowShortcutKeys = false;
            this.ToolStripMenuItemQuit.Size = new System.Drawing.Size(223, 54);
            this.ToolStripMenuItemQuit.Text = "Quit";
            this.ToolStripMenuItemQuit.Click += new System.EventHandler(this.ToolStripMenuItemQuit_Click);
            // 
            // toolStripItemTools
            // 
            this.toolStripItemTools.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripItemTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configureToolStripMenuItem,
            this.ToolStripMenuItemSaveToCSV,
            this.ToolStripMenuItemShow3DOrientation,
            this.ToolStripMenuItemOpenDataFolder,
            this.ToolStripMenuItemOpenLogFolder,
            this.configureSignalDisplayToolStripMenuItem,
            this.configureUDPMarkersToolStripMenuItem,
            this.pairBluetoothDeviceToolStripMenuItem});
            this.toolStripItemTools.Image = ((System.Drawing.Image)(resources.GetObject("toolStripItemTools.Image")));
            this.toolStripItemTools.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripItemTools.Name = "toolStripItemTools";
            this.toolStripItemTools.Size = new System.Drawing.Size(113, 45);
            this.toolStripItemTools.Text = "Tools";
            // 
            // configureToolStripMenuItem
            // 
            this.configureToolStripMenuItem.Enabled = false;
            this.configureToolStripMenuItem.Name = "configureToolStripMenuItem";
            this.configureToolStripMenuItem.Size = new System.Drawing.Size(507, 54);
            this.configureToolStripMenuItem.Text = "Configuration";
            this.configureToolStripMenuItem.Click += new System.EventHandler(this.configureToolStripMenuItem_Click);
            // 
            // ToolStripMenuItemSaveToCSV
            // 
            this.ToolStripMenuItemSaveToCSV.Name = "ToolStripMenuItemSaveToCSV";
            this.ToolStripMenuItemSaveToCSV.Size = new System.Drawing.Size(507, 54);
            this.ToolStripMenuItemSaveToCSV.Text = "CSV Auto-Save Info";
            this.ToolStripMenuItemSaveToCSV.Click += new System.EventHandler(this.ToolStripMenuItemSaveToCSV_Click);
            // 
            // ToolStripMenuItemShow3DOrientation
            // 
            this.ToolStripMenuItemShow3DOrientation.Enabled = false;
            this.ToolStripMenuItemShow3DOrientation.Name = "ToolStripMenuItemShow3DOrientation";
            this.ToolStripMenuItemShow3DOrientation.Size = new System.Drawing.Size(507, 54);
            this.ToolStripMenuItemShow3DOrientation.Text = "Show 3D Orientation";
            this.ToolStripMenuItemShow3DOrientation.Click += new System.EventHandler(this.ToolStripMenuItemShow3DOrientation_Click);
            // 
            // ToolStripMenuItemOpenDataFolder
            // 
            this.ToolStripMenuItemOpenDataFolder.Name = "ToolStripMenuItemOpenDataFolder";
            this.ToolStripMenuItemOpenDataFolder.Size = new System.Drawing.Size(507, 54);
            this.ToolStripMenuItemOpenDataFolder.Text = "Open Data Folder";
            this.ToolStripMenuItemOpenDataFolder.Click += new System.EventHandler(this.ToolStripMenuItemOpenDataFolder_Click);
            // 
            // ToolStripMenuItemOpenLogFolder
            // 
            this.ToolStripMenuItemOpenLogFolder.Name = "ToolStripMenuItemOpenLogFolder";
            this.ToolStripMenuItemOpenLogFolder.Size = new System.Drawing.Size(507, 54);
            this.ToolStripMenuItemOpenLogFolder.Text = "Open Log Folder";
            this.ToolStripMenuItemOpenLogFolder.Click += new System.EventHandler(this.ToolStripMenuItemOpenLogFolder_Click);
            // 
            // configureSignalDisplayToolStripMenuItem
            // 
            this.configureSignalDisplayToolStripMenuItem.Name = "configureSignalDisplayToolStripMenuItem";
            this.configureSignalDisplayToolStripMenuItem.Size = new System.Drawing.Size(507, 54);
            this.configureSignalDisplayToolStripMenuItem.Text = "Configure Signal Display";
            this.configureSignalDisplayToolStripMenuItem.Click += new System.EventHandler(this.configureSignalDisplayToolStripMenuItem_Click);
            // 
            // configureUDPMarkersToolStripMenuItem
            // 
            this.configureUDPMarkersToolStripMenuItem.Name = "configureUDPMarkersToolStripMenuItem";
            this.configureUDPMarkersToolStripMenuItem.Size = new System.Drawing.Size(507, 54);
            this.configureUDPMarkersToolStripMenuItem.Text = "Configure UDP Markers";
            this.configureUDPMarkersToolStripMenuItem.Click += new System.EventHandler(this.configureUDPMarkersToolStripMenuItem_Click);
            //
            // pairBluetoothDeviceToolStripMenuItem
            //
            this.pairBluetoothDeviceToolStripMenuItem.Name = "pairBluetoothDeviceToolStripMenuItem";
            this.pairBluetoothDeviceToolStripMenuItem.Size = new System.Drawing.Size(507, 54);
            this.pairBluetoothDeviceToolStripMenuItem.Text = "Pair Bluetooth Device...";
            this.pairBluetoothDeviceToolStripMenuItem.Click += new System.EventHandler(this.pairBluetoothDeviceToolStripMenuItem_Click);
            //
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkForUpdatesToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.bluetoothSetupHelpToolStripMenuItem});
            this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(106, 45);
            this.toolStripSplitButton1.Text = "Help";
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(469, 54);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for Updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(469, 54);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // bluetoothSetupHelpToolStripMenuItem
            // 
            this.bluetoothSetupHelpToolStripMenuItem.Name = "bluetoothSetupHelpToolStripMenuItem";
            this.bluetoothSetupHelpToolStripMenuItem.Size = new System.Drawing.Size(469, 54);
            this.bluetoothSetupHelpToolStripMenuItem.Text = "Bluetooth Setup Help";
            this.bluetoothSetupHelpToolStripMenuItem.Click += new System.EventHandler(this.bluetoothSetupHelpToolStripMenuItem_Click);
            // 
            // openDialog
            // 
            this.openDialog.DefaultExt = "csv";
            this.openDialog.FileName = "ShimmerData.csv";
            this.openDialog.Filter = "csv files|*.csv| All files|*.*";
            // 
            // buttonAddGraph
            // 
            this.buttonAddGraph.Location = new System.Drawing.Point(248, 413);
            this.buttonAddGraph.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.buttonAddGraph.Name = "buttonAddGraph";
            this.buttonAddGraph.Size = new System.Drawing.Size(89, 54);
            this.buttonAddGraph.TabIndex = 112;
            this.buttonAddGraph.Text = "+";
            this.buttonAddGraph.UseVisualStyleBackColor = true;
            this.buttonAddGraph.Click += new System.EventHandler(this.buttonAddGraph_Click);
            // 
            // buttonRemoveGraph
            // 
            this.buttonRemoveGraph.Location = new System.Drawing.Point(666, 413);
            this.buttonRemoveGraph.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.buttonRemoveGraph.Name = "buttonRemoveGraph";
            this.buttonRemoveGraph.Size = new System.Drawing.Size(89, 54);
            this.buttonRemoveGraph.TabIndex = 111;
            this.buttonRemoveGraph.Text = "-";
            this.buttonRemoveGraph.UseVisualStyleBackColor = true;
            this.buttonRemoveGraph.Click += new System.EventHandler(this.buttonRemoveGraph_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(75, 426);
            this.label1.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 32);
            this.label1.TabIndex = 78;
            this.label1.Text = "Add Graph";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(440, 427);
            this.label2.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(205, 32);
            this.label2.TabIndex = 79;
            this.label2.Text = "Remove Graph";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 919);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(4, 0, 37, 0);
            this.statusStrip1.Size = new System.Drawing.Size(2172, 54);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsStatusLabel
            // 
            this.tsStatusLabel.Name = "tsStatusLabel";
            this.tsStatusLabel.Size = new System.Drawing.Size(297, 41);
            this.tsStatusLabel.Text = "toolStripStatusLabel1";
            // 
            // buttonReload
            // 
            this.buttonReload.Enabled = false;
            this.buttonReload.Location = new System.Drawing.Point(307, 52);
            this.buttonReload.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size(69, 54);
            this.buttonReload.TabIndex = 124;
            this.buttonReload.Text = "🔃";
            this.buttonReload.UseVisualStyleBackColor = true;
            this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
            // 
            // labelPRR
            // 
            this.labelPRR.AutoSize = true;
            this.labelPRR.Location = new System.Drawing.Point(14, 102);
            this.labelPRR.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelPRR.Name = "labelPRR";
            this.labelPRR.Size = new System.Drawing.Size(312, 32);
            this.labelPRR.TabIndex = 125;
            this.labelPRR.Text = "Packet Reception Rate:";
            this.labelPRR.Click += new System.EventHandler(this.labelPRR_Click);
            // 
            // textBoxLeadOffStatus1
            // 
            this.textBoxLeadOffStatus1.Location = new System.Drawing.Point(20, 100);
            this.textBoxLeadOffStatus1.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.textBoxLeadOffStatus1.Name = "textBoxLeadOffStatus1";
            this.textBoxLeadOffStatus1.Size = new System.Drawing.Size(88, 38);
            this.textBoxLeadOffStatus1.TabIndex = 136;
            // 
            // textBoxLeadOffStatus3
            // 
            this.textBoxLeadOffStatus3.Location = new System.Drawing.Point(239, 100);
            this.textBoxLeadOffStatus3.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.textBoxLeadOffStatus3.Name = "textBoxLeadOffStatus3";
            this.textBoxLeadOffStatus3.Size = new System.Drawing.Size(88, 38);
            this.textBoxLeadOffStatus3.TabIndex = 137;
            this.textBoxLeadOffStatus3.TextChanged += new System.EventHandler(this.textBoxLeadOffStatus3_TextChanged);
            // 
            // textBoxLeadOffStatus2
            // 
            this.textBoxLeadOffStatus2.Location = new System.Drawing.Point(128, 100);
            this.textBoxLeadOffStatus2.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.textBoxLeadOffStatus2.Name = "textBoxLeadOffStatus2";
            this.textBoxLeadOffStatus2.Size = new System.Drawing.Size(88, 38);
            this.textBoxLeadOffStatus2.TabIndex = 138;
            this.textBoxLeadOffStatus2.TextChanged += new System.EventHandler(this.textBoxLeadOffStatus2_TextChanged);
            // 
            // textBoxLeadOffStatus4
            // 
            this.textBoxLeadOffStatus4.Location = new System.Drawing.Point(347, 100);
            this.textBoxLeadOffStatus4.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.textBoxLeadOffStatus4.Name = "textBoxLeadOffStatus4";
            this.textBoxLeadOffStatus4.Size = new System.Drawing.Size(88, 38);
            this.textBoxLeadOffStatus4.TabIndex = 139;
            this.textBoxLeadOffStatus4.TextChanged += new System.EventHandler(this.textBoxLeadOffStatus4_TextChanged);
            // 
            // textBoxLeadOffStatus5
            // 
            this.textBoxLeadOffStatus5.Location = new System.Drawing.Point(456, 99);
            this.textBoxLeadOffStatus5.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.textBoxLeadOffStatus5.Name = "textBoxLeadOffStatus5";
            this.textBoxLeadOffStatus5.Size = new System.Drawing.Size(88, 38);
            this.textBoxLeadOffStatus5.TabIndex = 140;
            this.textBoxLeadOffStatus5.TextChanged += new System.EventHandler(this.textBoxLeadOffStatus5_TextChanged);
            // 
            // labelLeadOffStatus2
            // 
            this.labelLeadOffStatus2.AutoSize = true;
            this.labelLeadOffStatus2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLeadOffStatus2.Location = new System.Drawing.Point(123, 71);
            this.labelLeadOffStatus2.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelLeadOffStatus2.Name = "labelLeadOffStatus2";
            this.labelLeadOffStatus2.Size = new System.Drawing.Size(39, 25);
            this.labelLeadOffStatus2.TabIndex = 141;
            this.labelLeadOffStatus2.Text = "RA";
            this.labelLeadOffStatus2.Click += new System.EventHandler(this.labelLeadOffStatus2_Click);
            // 
            // labelLeadOffStatus1
            // 
            this.labelLeadOffStatus1.AutoSize = true;
            this.labelLeadOffStatus1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLeadOffStatus1.Location = new System.Drawing.Point(15, 71);
            this.labelLeadOffStatus1.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelLeadOffStatus1.Name = "labelLeadOffStatus1";
            this.labelLeadOffStatus1.Size = new System.Drawing.Size(37, 25);
            this.labelLeadOffStatus1.TabIndex = 142;
            this.labelLeadOffStatus1.Text = "LA";
            this.labelLeadOffStatus1.Click += new System.EventHandler(this.labelLeadOffStatus1_Click);
            // 
            // labelLeadOffStatus5
            // 
            this.labelLeadOffStatus5.AutoSize = true;
            this.labelLeadOffStatus5.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLeadOffStatus5.Location = new System.Drawing.Point(452, 71);
            this.labelLeadOffStatus5.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelLeadOffStatus5.Name = "labelLeadOffStatus5";
            this.labelLeadOffStatus5.Size = new System.Drawing.Size(50, 25);
            this.labelLeadOffStatus5.TabIndex = 143;
            this.labelLeadOffStatus5.Text = "RLD";
            this.labelLeadOffStatus5.Click += new System.EventHandler(this.labelLeadOffStatus5_Click);
            // 
            // labelLeadOffStatus3
            // 
            this.labelLeadOffStatus3.AutoSize = true;
            this.labelLeadOffStatus3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLeadOffStatus3.Location = new System.Drawing.Point(232, 71);
            this.labelLeadOffStatus3.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelLeadOffStatus3.Name = "labelLeadOffStatus3";
            this.labelLeadOffStatus3.Size = new System.Drawing.Size(34, 25);
            this.labelLeadOffStatus3.TabIndex = 144;
            this.labelLeadOffStatus3.Text = "LL";
            this.labelLeadOffStatus3.Click += new System.EventHandler(this.labelLeadOffStatus3_Click);
            // 
            // labelLeadOffStatus4
            // 
            this.labelLeadOffStatus4.AutoSize = true;
            this.labelLeadOffStatus4.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLeadOffStatus4.Location = new System.Drawing.Point(342, 71);
            this.labelLeadOffStatus4.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelLeadOffStatus4.Name = "labelLeadOffStatus4";
            this.labelLeadOffStatus4.Size = new System.Drawing.Size(37, 25);
            this.labelLeadOffStatus4.TabIndex = 145;
            this.labelLeadOffStatus4.Text = "V1";
            this.labelLeadOffStatus4.Click += new System.EventHandler(this.labelLeadOffStatus4_Click);
            // 
            // labelExGLeadOffDetection
            // 
            this.labelExGLeadOffDetection.AutoSize = true;
            this.labelExGLeadOffDetection.Location = new System.Drawing.Point(11, 30);
            this.labelExGLeadOffDetection.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelExGLeadOffDetection.Name = "labelExGLeadOffDetection";
            this.labelExGLeadOffDetection.Size = new System.Drawing.Size(0, 32);
            this.labelExGLeadOffDetection.TabIndex = 146;
            this.labelExGLeadOffDetection.Click += new System.EventHandler(this.labelExGLeadOffDetection_Click);
            // 
            // buttonReadDirectory
            // 
            this.buttonReadDirectory.Location = new System.Drawing.Point(1753, 321);
            this.buttonReadDirectory.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.buttonReadDirectory.Name = "buttonReadDirectory";
            this.buttonReadDirectory.Size = new System.Drawing.Size(75, 54);
            this.buttonReadDirectory.TabIndex = 147;
            this.buttonReadDirectory.Text = "Read Directory";
            this.buttonReadDirectory.UseVisualStyleBackColor = true;
            this.buttonReadDirectory.Click += new System.EventHandler(this.buttonReadDirectory_Click);
            // 
            // checkBoxTSACheck
            // 
            this.checkBoxTSACheck.AutoSize = true;
            this.checkBoxTSACheck.Location = new System.Drawing.Point(1226, 339);
            this.checkBoxTSACheck.Margin = new System.Windows.Forms.Padding(5);
            this.checkBoxTSACheck.Name = "checkBoxTSACheck";
            this.checkBoxTSACheck.Size = new System.Drawing.Size(426, 36);
            this.checkBoxTSACheck.TabIndex = 149;
            this.checkBoxTSACheck.Text = "Time Stamp Alignment Check";
            this.toolTip1.SetToolTip(this.checkBoxTSACheck, resources.GetString("checkBoxTSACheck.ToolTip"));
            this.checkBoxTSACheck.UseVisualStyleBackColor = true;
            this.checkBoxTSACheck.CheckedChanged += new System.EventHandler(this.checkBoxTSACheck_CheckedChanged);
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 2000;
            this.toolTip1.AutoPopDelay = 20000;
            this.toolTip1.InitialDelay = 200;
            this.toolTip1.ReshowDelay = 400;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(20, 30);
            this.button1.Margin = new System.Windows.Forms.Padding(5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(249, 50);
            this.button1.TabIndex = 150;
            this.button1.Text = "Reset PPGtoHR";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // timer1
            // 
            this.timer1.Interval = 10000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // textBoxSubj
            // 
            this.textBoxSubj.Location = new System.Drawing.Point(183, 42);
            this.textBoxSubj.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.textBoxSubj.Name = "textBoxSubj";
            this.textBoxSubj.Size = new System.Drawing.Size(248, 38);
            this.textBoxSubj.TabIndex = 151;
            // 
            // groupBoxExGLeadOff
            // 
            this.groupBoxExGLeadOff.Controls.Add(this.labelExGLeadOffDetection);
            this.groupBoxExGLeadOff.Controls.Add(this.labelLeadOffStatus1);
            this.groupBoxExGLeadOff.Controls.Add(this.labelLeadOffStatus2);
            this.groupBoxExGLeadOff.Controls.Add(this.labelLeadOffStatus3);
            this.groupBoxExGLeadOff.Controls.Add(this.labelLeadOffStatus4);
            this.groupBoxExGLeadOff.Controls.Add(this.labelLeadOffStatus5);
            this.groupBoxExGLeadOff.Controls.Add(this.textBoxLeadOffStatus1);
            this.groupBoxExGLeadOff.Controls.Add(this.textBoxLeadOffStatus2);
            this.groupBoxExGLeadOff.Controls.Add(this.textBoxLeadOffStatus3);
            this.groupBoxExGLeadOff.Controls.Add(this.textBoxLeadOffStatus4);
            this.groupBoxExGLeadOff.Controls.Add(this.textBoxLeadOffStatus5);
            this.groupBoxExGLeadOff.Location = new System.Drawing.Point(630, 229);
            this.groupBoxExGLeadOff.Margin = new System.Windows.Forms.Padding(5);
            this.groupBoxExGLeadOff.Name = "groupBoxExGLeadOff";
            this.groupBoxExGLeadOff.Padding = new System.Windows.Forms.Padding(5);
            this.groupBoxExGLeadOff.Size = new System.Drawing.Size(564, 164);
            this.groupBoxExGLeadOff.TabIndex = 158;
            this.groupBoxExGLeadOff.TabStop = false;
            this.groupBoxExGLeadOff.Text = "EXG Lead-Off Detection";
            // 
            // groupBoxPPG
            // 
            this.groupBoxPPG.Controls.Add(this.button1);
            this.groupBoxPPG.Location = new System.Drawing.Point(1226, 229);
            this.groupBoxPPG.Margin = new System.Windows.Forms.Padding(5);
            this.groupBoxPPG.Name = "groupBoxPPG";
            this.groupBoxPPG.Padding = new System.Windows.Forms.Padding(5);
            this.groupBoxPPG.Size = new System.Drawing.Size(289, 100);
            this.groupBoxPPG.TabIndex = 159;
            this.groupBoxPPG.TabStop = false;
            this.groupBoxPPG.Text = "PPG";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 46);
            this.label3.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 32);
            this.label3.TabIndex = 152;
            this.label3.Text = "Subject ID:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.comboBoxComPorts);
            this.groupBox1.Controls.Add(this.buttonConnect);
            this.groupBox1.Controls.Add(this.buttonReload);
            this.groupBox1.Location = new System.Drawing.Point(37, 54);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox1.Size = new System.Drawing.Size(708, 155);
            this.groupBox1.TabIndex = 155;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "COM / Bluetooth Connection";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(348, 103);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 32);
            this.label4.TabIndex = 126;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBoxSubj);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.buttonStreamandLog);
            this.groupBox2.Location = new System.Drawing.Point(789, 51);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox2.Size = new System.Drawing.Size(462, 157);
            this.groupBox2.TabIndex = 156;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Logging";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labelUDPStatus);
            this.groupBox3.Location = new System.Drawing.Point(1301, 43);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox3.Size = new System.Drawing.Size(414, 164);
            this.groupBox3.TabIndex = 157;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "UDP Markers";
            // 
            // labelUDPStatus
            // 
            this.labelUDPStatus.AutoSize = true;
            this.labelUDPStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelUDPStatus.ForeColor = System.Drawing.Color.Gray;
            this.labelUDPStatus.Location = new System.Drawing.Point(20, 45);
            this.labelUDPStatus.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelUDPStatus.Name = "labelUDPStatus";
            this.labelUDPStatus.Size = new System.Drawing.Size(308, 32);
            this.labelUDPStatus.TabIndex = 0;
            this.labelUDPStatus.Text = "UDP Markers: Disabled";
            this.labelUDPStatus.Click += new System.EventHandler(this.configureUDPMarkersToolStripMenuItem_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.textBoxShimmerState);
            this.groupBox4.Controls.Add(this.labelState);
            this.groupBox4.Controls.Add(this.labelPRR);
            this.groupBox4.Location = new System.Drawing.Point(37, 219);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox4.Size = new System.Drawing.Size(558, 184);
            this.groupBox4.TabIndex = 158;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Shimmer Status";
            // 
            // groupBoxManualMarker
            // 
            this.groupBoxManualMarker.Controls.Add(this.labelMarkerIP);
            this.groupBoxManualMarker.Controls.Add(this.textBoxMarkerIP);
            this.groupBoxManualMarker.Controls.Add(this.labelMarkerPort);
            this.groupBoxManualMarker.Controls.Add(this.textBoxMarkerPort);
            this.groupBoxManualMarker.Controls.Add(this.buttonSendMarker);
            this.groupBoxManualMarker.Location = new System.Drawing.Point(1738, 43);
            this.groupBoxManualMarker.Margin = new System.Windows.Forms.Padding(5);
            this.groupBoxManualMarker.Name = "groupBoxManualMarker";
            this.groupBoxManualMarker.Padding = new System.Windows.Forms.Padding(5);
            this.groupBoxManualMarker.Size = new System.Drawing.Size(400, 211);
            this.groupBoxManualMarker.TabIndex = 160;
            this.groupBoxManualMarker.TabStop = false;
            this.groupBoxManualMarker.Text = "Outbound UDP Markers";
            // 
            // labelMarkerIP
            // 
            this.labelMarkerIP.AutoSize = true;
            this.labelMarkerIP.Location = new System.Drawing.Point(15, 40);
            this.labelMarkerIP.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelMarkerIP.Name = "labelMarkerIP";
            this.labelMarkerIP.Size = new System.Drawing.Size(159, 32);
            this.labelMarkerIP.TabIndex = 0;
            this.labelMarkerIP.Text = "IP Address:";
            // 
            // textBoxMarkerIP
            // 
            this.textBoxMarkerIP.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxMarkerIP.Location = new System.Drawing.Point(180, 37);
            this.textBoxMarkerIP.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.textBoxMarkerIP.Name = "textBoxMarkerIP";
            this.textBoxMarkerIP.ReadOnly = true;
            this.textBoxMarkerIP.Size = new System.Drawing.Size(200, 38);
            this.textBoxMarkerIP.TabIndex = 1;
            this.textBoxMarkerIP.Text = "127.0.0.1";
            // 
            // labelMarkerPort
            // 
            this.labelMarkerPort.AutoSize = true;
            this.labelMarkerPort.Location = new System.Drawing.Point(15, 85);
            this.labelMarkerPort.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.labelMarkerPort.Name = "labelMarkerPort";
            this.labelMarkerPort.Size = new System.Drawing.Size(74, 32);
            this.labelMarkerPort.TabIndex = 2;
            this.labelMarkerPort.Text = "Port:";
            // 
            // textBoxMarkerPort
            // 
            this.textBoxMarkerPort.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxMarkerPort.Location = new System.Drawing.Point(180, 82);
            this.textBoxMarkerPort.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.textBoxMarkerPort.Name = "textBoxMarkerPort";
            this.textBoxMarkerPort.ReadOnly = true;
            this.textBoxMarkerPort.Size = new System.Drawing.Size(100, 38);
            this.textBoxMarkerPort.TabIndex = 3;
            this.textBoxMarkerPort.Text = "5501";
            // 
            // buttonSendMarker
            // 
            this.buttonSendMarker.Location = new System.Drawing.Point(15, 130);
            this.buttonSendMarker.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.buttonSendMarker.Name = "buttonSendMarker";
            this.buttonSendMarker.Size = new System.Drawing.Size(365, 68);
            this.buttonSendMarker.TabIndex = 4;
            this.buttonSendMarker.Text = "Send Marker (230)";
            this.buttonSendMarker.UseVisualStyleBackColor = true;
            this.buttonSendMarker.Click += new System.EventHandler(this.ButtonSendMarker_Click);
            // 
            // button_ConfigureSignals
            // 
            this.button_ConfigureSignals.Enabled = false;
            this.button_ConfigureSignals.Location = new System.Drawing.Point(893, 416);
            this.button_ConfigureSignals.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.button_ConfigureSignals.Name = "button_ConfigureSignals";
            this.button_ConfigureSignals.Size = new System.Drawing.Size(251, 48);
            this.button_ConfigureSignals.TabIndex = 159;
            this.button_ConfigureSignals.Text = "Configure Signals";
            this.button_ConfigureSignals.UseVisualStyleBackColor = true;
            this.button_ConfigureSignals.Click += new System.EventHandler(this.button_ConfigureSignals_Click);
            // 
            // Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(2172, 973);
            this.Controls.Add(this.button_ConfigureSignals);
            this.Controls.Add(this.groupBoxManualMarker);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxExGLeadOff);
            this.Controls.Add(this.groupBoxPPG);
            this.Controls.Add(this.checkBoxTSACheck);
            this.Controls.Add(this.buttonReadDirectory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonRemoveGraph);
            this.Controls.Add(this.buttonAddGraph);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.groupBoxGraph1);
            this.Controls.Add(this.groupBoxGraph2);
            this.Controls.Add(this.groupBoxGraph3);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.Name = "Control";
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.buttonDisconnect_Click);
            this.Load += new System.EventHandler(this.ControlForm_Load);
            this.groupBoxGraph1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBoxExGLeadOff.ResumeLayout(false);
            this.groupBoxExGLeadOff.PerformLayout();
            this.groupBoxPPG.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBoxManualMarker.ResumeLayout(false);
            this.groupBoxManualMarker.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Button buttonStreamandLog;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TextBox textBoxShimmerState;
        private System.Windows.Forms.Label labelState;
        private System.Windows.Forms.GroupBox groupBoxGraph1;
        private ZedGraph.ZedGraphControl ZedGraphControl1;
        private System.Windows.Forms.GroupBox groupBoxGraph2;
        private System.Windows.Forms.GroupBox groupBoxGraph3;
        private System.Windows.Forms.ComboBox comboBoxComPorts;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripItemFile;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemQuit;
        private System.Windows.Forms.ToolStripDropDownButton toolStripItemTools;
        private System.Windows.Forms.OpenFileDialog openDialog;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemSaveToCSV;
        public System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemShow3DOrientation;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemOpenDataFolder;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemOpenLogFolder;
        private System.Windows.Forms.Button buttonAddGraph;
        private System.Windows.Forms.Button buttonRemoveGraph;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem configureToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsStatusLabel;
        private System.Windows.Forms.Button buttonReload;
        private System.Windows.Forms.Label labelPRR;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.TextBox textBoxLeadOffStatus1;
        private System.Windows.Forms.TextBox textBoxLeadOffStatus3;
        private System.Windows.Forms.TextBox textBoxLeadOffStatus2;
        private System.Windows.Forms.TextBox textBoxLeadOffStatus4;
        private System.Windows.Forms.TextBox textBoxLeadOffStatus5;
        private System.Windows.Forms.Label labelLeadOffStatus2;
        private System.Windows.Forms.Label labelLeadOffStatus1;
        private System.Windows.Forms.Label labelLeadOffStatus5;
        private System.Windows.Forms.Label labelLeadOffStatus3;
        private System.Windows.Forms.Label labelLeadOffStatus4;
        private System.Windows.Forms.Label labelExGLeadOffDetection;
        private System.Windows.Forms.ToolStripDropDownButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bluetoothSetupHelpToolStripMenuItem;
        private System.Windows.Forms.Button buttonReadDirectory;
        private System.Windows.Forms.CheckBox checkBoxTSACheck;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBoxSubj;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labelUDPStatus;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBoxManualMarker;
        private System.Windows.Forms.Label labelMarkerIP;
        private System.Windows.Forms.TextBox textBoxMarkerIP;
        private System.Windows.Forms.Label labelMarkerPort;
        private System.Windows.Forms.TextBox textBoxMarkerPort;
        private System.Windows.Forms.Button buttonSendMarker;
        private System.Windows.Forms.ToolStripMenuItem configureSignalDisplayToolStripMenuItem;
        private System.Windows.Forms.Button button_ConfigureSignals;
        private System.Windows.Forms.ToolStripMenuItem configureUDPMarkersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pairBluetoothDeviceToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxExGLeadOff;
        private System.Windows.Forms.GroupBox groupBoxPPG;
        private System.Windows.Forms.Label label4;
    }
}

