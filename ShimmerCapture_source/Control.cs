using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Diagnostics;
using ZedGraph;
using ShimmerAPI;
using ShimmerLibrary;
//using ExceptionReporting;   // For ExceptionReporting

/*d
 * Changes since 0.1.15
 * - Method for ExG lead off dectection by decoding ExG status bytes
 * - UI TextBoxes and Labels for ExG lead off detection
 * 
 * Changes 0.1.10
 * - Updated ppg channel selection
 * - various UI fixes to configuration page
 * - moved ecg to hr conversion to after filtering
 * 
 * Changes since rev 0.1.9
 * - write to sd when each tab page is updated
 * - won't send a stop command before disconnect
 * 
 * * Changes since rev 0.1.5
 * - updates according to Weibo's latest work
 * 
 * Changes since rev 0.1.4 (ShimmerCapture)
 * - Updated ShimmerBluetooth.cs and ShimmerSDBT.cs and ShimmerBluetooth.cs 
 * 
 * Changes since rev 0.13
 * - 3D Orientation cube
 * - Logging moved to separate class
 * - PPG options in config window
 * - ECG-HR algorithm added not tested
 * - Bug with zed graph - show point values - fixed
 * - HPF for PPG - 0.5 corner frequency
 * - streamlining Shimmer classes - filtering, PPG-HR, ECG-HR
 * - changes to config window
 *      - general config, exg and sd log config all in same window - different tab for each
 * - using ShimmerSDBT object instead of Shimmer
 * - ExG -> ECG/EMG (Shimmer3)
 * - ExG reg bytes in Hex (just on GUI)
 * - ExG reg byte1 (both chips) being updated to match sampling rate (for default configs too). 
 * 
 * + changes in API - see API header
 * */

namespace ShimmerAPI
{
    public partial class Control : Form
    {
        public static readonly string ApplicationName = Application.ProductName.ToString().Replace("_", " ");
        //        private string versionNumber = Application.ProductVersion.ToString().Substring(0, Application.ProductVersion.ToString().LastIndexOf(".")).ToLower();
        private string versionNumber = Application.ProductVersion.ToString().ToLower();

        private delegate void ShowChannelTextBoxesCallback(int i);
        private delegate void ShowChannelLabelsCallback(List<String> s);
        private delegate void UpdateChannelsCallback(List<Double> d);
        private delegate void UpdateExGLeadOffDetectionCallback(double d1, double d2);
        private int ShowTBcount = 0;
        List<TextBox> ListofTextBox = new List<TextBox>();
        List<System.Windows.Forms.Label> ListofLabels = new List<System.Windows.Forms.Label>();
        List<String> ListofSignals = new List<String>();

        //public string status_text = "";
        private Configuration Configure;
        private Logging WriteToFile;
        private MarkerLogger markerLogger;
        private Orientation3D Orientation3DForm;
        private System.IO.Ports.SerialPort SerialPort = new SerialPort();
        private string ComPort;
        public ShimmerLogAndStreamSystemSerialPort ShimmerDevice = new ShimmerLogAndStreamSystemSerialPort("Shimmer", "");

        // Manual UDP Marker Controls are declared in Control.Designer.cs
        // labelUDPStatus, groupBoxManualMarker, textBoxMarkerIP, textBoxMarkerPort, buttonSendMarker, labelMarkerIP, labelMarkerPort

        //Plot
        private ZedGraph.ZedGraphControl ZedGraphControl2 = new ZedGraph.ZedGraphControl(); //These need to be defined here for Linux. Otherwise can't later be added
        private ZedGraph.ZedGraphControl ZedGraphControl3 = new ZedGraph.ZedGraphControl();
        private GraphPane MyPaneGraph1;
        private GraphPane MyPaneGraph2;
        private GraphPane MyPaneGraph3;
        public static int CountXAxisDataPoints = 0;
        private List<RollingPointPairList> DataListGraph1 = new List<RollingPointPairList>();
        private List<LineItem> CurvesListGraph1 = new List<LineItem>();
        private List<RollingPointPairList> DataListGraph2 = new List<RollingPointPairList>();
        private List<LineItem> CurvesListGraph2 = new List<LineItem>();
        private List<RollingPointPairList> DataListGraph3 = new List<RollingPointPairList>();
        private List<LineItem> CurvesListGraph3 = new List<LineItem>();
        private int NumberOfTracesCountGraph1 = 0;
        private int NumberOfTracesCountGraph2 = 0;
        private int NumberOfTracesCountGraph3 = 0;
        private static int maxNumberOfTracesToPlot = 30;
        public static bool FirstTime = true;
        private List<String> SelectedSignalNameGroup1 = new List<String>();
        private List<String> SelectedSignalNameGroup2 = new List<String>();
        private List<String> SelectedSignalNameGroup3 = new List<String>();
        private List<String> AvailableSignalNames = new List<String>();
        private Random Rnd = new Random();
        private List<String> ShimmerIdSetup = new List<String>();
        private bool IsGraph2Visible = false;
        private bool IsGraph3Visible = false;
        private List<String> StreamingSignalNamesRaw = new List<String>();
        private List<String> StreamingSignalNamesCal = new List<String>();
        //Write to file
        private String Delimeter = ",";
        //Cube
        private double[,] SetOriMatrix = new double[,] { { 1, 0, 0 }, { 0, -1, 0 }, { 0, 0, -1 } };
        private Boolean SetOrientation = false;
        private Boolean Is3DCubeOpen = false;
        //PPG-HR
        private PPGToHRAlgorithm PPGtoHeartRateCalculation;
        private Boolean EnablePPGtoHRConversion = false;
        private int NumberOfHeartBeatsToAverage = 5;
        private int NumberOfHeartBeatsToAverageECG = 1;
        private int TrainingPeriodPPG = 10; //5 second buffer
        //ECG-HR
        private ECGToHR ECGtoHR;
        private Boolean EnableECGtoHRConversion = false;
        private int TrainingPeriodECG = 10; //5 second buffer
        //Filters
        Filter NQF_Exg1Ch1;
        Filter NQF_Exg1Ch2;
        Filter NQF_Exg2Ch1;
        Filter NQF_Exg2Ch2;
        Filter LPF_PPG;
        Filter HPF_PPG;
        Filter HPF_Exg1Ch1;
        Filter HPF_Exg1Ch2;
        Filter HPF_Exg2Ch1;
        Filter HPF_Exg2Ch2;
        Filter BSF_Exg1Ch1;
        Filter BSF_Exg1Ch2;
        Filter BSF_Exg2Ch1;
        Filter BSF_Exg2Ch2;
        public bool EnableHPF_0_05HZ = false;
        public bool EnableNQF = false;
        public bool EnableHPF_0_5HZ = false;
        public bool EnableHPF_5HZ = false;
        public bool EnableBSF_49_51HZ = false;
        public bool EnableBSF_59_61HZ = false;
        public String PPGSignalName = "Internal ADC A13"; //This is used to identify which signal to feed into the PPF to HR algorithm
        //ExG
        public String ECGSignalName = "ECG LL-RA"; //This is used to identify which signal to feed into the ECG to HR algorithm
        private int ExGLeadOffCounter = 0;
        private int ExGLeadOffCounterSize = 0;

        private Color[] TraceColours = new Color[30]{Color.Blue, Color.Red, Color.Green, Color.Black, Color.Orange, Color.Gray,
        Color.Purple, Color.Brown, Color.Yellow, Color.Aqua, Color.Pink, Color.DarkBlue, Color.DarkRed, Color.DarkOrange, 
        Color.LightBlue, Color.Navy, Color.Cyan, Color.LightGreen, Color.Maroon, Color.Teal, Color.DarkGray, Color.Indigo,
        Color.Khaki, Color.LightGray, Color.OrangeRed, Color.Turquoise, Color.YellowGreen, Color.DarkCyan, Color.Magenta, 
        Color.MediumBlue};
        int count = 0;

        private UDPListener udpListener;
        private int udpPortNo=5501;
        private int udpMsgLen = 1;
        // private string udpIPaddr = "0.0.0.0";

        public static bool usingLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public Control()
        {
            InitializeComponent();
            // Catch unhandled exceptions
            Application.ThreadException += new ExceptionEventHandler().ApplicationThreadException;
            AppDomain.CurrentDomain.UnhandledException += new ExceptionEventHandler().CurrentDomainUnhandledException;
        }




        private void ControlForm_Load(object sender, EventArgs e)
        {
           
            //buttonSetBlinkLED.Visible = false;
            checkBoxTSACheck.Visible = false;
            buttonStreamandLog.Visible = true;
            buttonStreamandLog.Enabled = false;
            buttonReadDirectory.Visible = false;
            button1.Visible = false;
            labelPRR.Visible = false;
            if (!usingLinux)
            {
                this.ShowInTaskbar = true;
            }
            this.Text = ApplicationName + " v" + versionNumber;
            tsStatusLabel.Text = "";


            int numPorts=getNumPorts();
            ComPort = comboBoxComPorts.Text;

            // btsd changes1
            
            buttonReload.Enabled = true;
            comboBoxComPorts.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBoxComPorts.AutoCompleteSource = AutoCompleteSource.ListItems;



            this.Resize += new System.EventHandler(this.FormResize);

            if (usingLinux)
            {
                ZedGraphControl1.Size = new System.Drawing.Size(this.Size.Width - 1150, ZedGraphControl1.Size.Height);
                label1.Visible = false;
                buttonAddGraph.Visible = false;
                label2.Visible = false;
                buttonRemoveGraph.Visible = false;
                //ZedGraphControl2.Size = new System.Drawing.Size(this.Size.Width - 1000, ZedGraphControl2.Size.Height);
                //ZedGraphControl3.Size = new System.Drawing.Size(this.Size.Width - 1000, ZedGraphControl3.Size.Height);
            }

            InitializeGraphs();
            initializeExGLeadOff();


            if(numPorts>0)
                comboBoxComPorts.SelectedText = Properties.Settings.Default.COMport;
            textBoxSubj.Text = Properties.Settings.Default.Subject;

            // Load UDP settings
            udpMarkersEnabled = Properties.Settings.Default.UDPenabled;

            if (!int.TryParse(Properties.Settings.Default.UDPport, out udpPortNo))
            {
                udpPortNo = 5501; // Default port
            }

            udpListener = new UDPListener(udpPortNo);

            // Update UDP status display after form controls are initialized
            UpdateUDPStatusDisplay();

            buttonReload_Click(sender, null);

        }

      
        public void ChangeStatusLabel(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(ChangeStatusLabel), new object[] { text });
                return;
            }
            tsStatusLabel.Text = text;
        }

        public string GetStatusLabelText()
        {
            return tsStatusLabel.Text;
        }

        private void initializeExGLeadOff()
        {
            textBoxLeadOffStatus1.Text += "";
            textBoxLeadOffStatus2.Text += "";
            textBoxLeadOffStatus3.Text += "";
            textBoxLeadOffStatus4.Text += "";
            textBoxLeadOffStatus5.Text += "";
            labelExGLeadOffDetection.Visible = false;
            labelLeadOffStatus1.Visible = false;
            labelLeadOffStatus2.Visible = false;
            labelLeadOffStatus3.Visible = false;
            labelLeadOffStatus4.Visible = false;
            labelLeadOffStatus5.Visible = false;
            textBoxLeadOffStatus1.Visible = false;
            textBoxLeadOffStatus2.Visible = false;
            textBoxLeadOffStatus3.Visible = false;
            textBoxLeadOffStatus4.Visible = false;
            textBoxLeadOffStatus5.Visible = false;
        }

        private void InitializeGraphs()
        {
            if (!usingLinux)
            {
                this.ZedGraphControl2.BackColor = System.Drawing.SystemColors.AppWorkspace;
                this.ZedGraphControl2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
                this.ZedGraphControl2.Location = new System.Drawing.Point(31, 443);
                this.ZedGraphControl2.Name = "ZedGraphControl2";
                this.ZedGraphControl2.ScrollGrace = 0D;
                this.ZedGraphControl2.ScrollMaxX = 0D;
                this.ZedGraphControl2.ScrollMaxY = 0D;
                this.ZedGraphControl2.ScrollMaxY2 = 0D;
                this.ZedGraphControl2.ScrollMinX = 0D;
                this.ZedGraphControl2.ScrollMinY = 0D;
                this.ZedGraphControl2.ScrollMinY2 = 0D;
                this.ZedGraphControl2.Size = new System.Drawing.Size(490, 297);
                this.ZedGraphControl2.TabIndex = 45;
                this.ZedGraphControl2.Visible = false;
                this.Controls.Add(this.ZedGraphControl2);

                this.ZedGraphControl3.BackColor = System.Drawing.SystemColors.AppWorkspace;
                this.ZedGraphControl3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
                this.ZedGraphControl3.Location = new System.Drawing.Point(31, 756);
                this.ZedGraphControl3.Name = "ZedGraphControl3";
                this.ZedGraphControl3.ScrollGrace = 0D;
                this.ZedGraphControl3.ScrollMaxX = 0D;
                this.ZedGraphControl3.ScrollMaxY = 0D;
                this.ZedGraphControl3.ScrollMaxY2 = 0D;
                this.ZedGraphControl3.ScrollMinX = 0D;
                this.ZedGraphControl3.ScrollMinY = 0D;
                this.ZedGraphControl3.ScrollMinY2 = 0D;
                this.ZedGraphControl3.Size = new System.Drawing.Size(490, 297);
                this.ZedGraphControl3.TabIndex = 80;
                this.ZedGraphControl3.Visible = false;
                this.Controls.Add(this.ZedGraphControl3);
            }
            ZedGraphControl1.GraphPane.CurveList.Clear();
            MyPaneGraph1 = ZedGraphControl1.GraphPane;
            ZedGraphControl1.IsShowPointValues = false;
            ZedGraphControl1.IsShowCursorValues = false;

            ZedGraphControl2.GraphPane.CurveList.Clear();
            MyPaneGraph2 = ZedGraphControl2.GraphPane;
            ZedGraphControl2.IsShowPointValues = false;
            ZedGraphControl2.IsShowCursorValues = false;

            ZedGraphControl3.GraphPane.CurveList.Clear();
            MyPaneGraph3 = ZedGraphControl3.GraphPane;
            ZedGraphControl3.IsShowPointValues = false;
            ZedGraphControl3.IsShowCursorValues = false;

            MyPaneGraph1.XAxis.IsVisible = false;
            MyPaneGraph1.YAxis.Title.Text = "Amplitude";
            MyPaneGraph1.Title.IsVisible = false;
            MyPaneGraph1.Legend.Position = LegendPos.Float;
            MyPaneGraph1.Legend.Location = new Location(0f, 0f, CoordType.ChartFraction, AlignH.Left, AlignV.Top);
            MyPaneGraph1.YAxis.MajorGrid.IsZeroLine = false;

            MyPaneGraph2.XAxis.IsVisible = false;
            MyPaneGraph2.YAxis.Title.Text = "Amplitude";
            MyPaneGraph2.Title.IsVisible = false;
            MyPaneGraph2.Legend.Position = LegendPos.Float;
            MyPaneGraph2.Legend.Location = new Location(0f, 0f, CoordType.ChartFraction, AlignH.Left, AlignV.Top);
            MyPaneGraph2.YAxis.MajorGrid.IsZeroLine = false;

            MyPaneGraph3.XAxis.IsVisible = false;
            MyPaneGraph3.YAxis.Title.Text = "Amplitude";
            MyPaneGraph3.Title.IsVisible = false;
            MyPaneGraph3.Legend.Position = LegendPos.Float;
            MyPaneGraph3.Legend.Location = new Location(0f, 0f, CoordType.ChartFraction, AlignH.Left, AlignV.Top);
            MyPaneGraph3.YAxis.MajorGrid.IsZeroLine = false;

            // Manual Marker UI is now defined in Designer (no longer dynamically created)
            // InitializeManualMarkerControls(); // Removed - controls now in Designer
            // Load saved settings
            LoadManualMarkerSettings();
        }

        private void FormResize(object sender, EventArgs e)
        {
            int scale1 = 350;
            int scale2 = 145;
            int scale3 = 400;
            if (usingLinux)
            {
                System.Console.WriteLine("Using Linux");
                scale1 = 400;
                scale2 = 195;
                scale3 = 450;
            }

            System.Console.WriteLine("FORM RESIZE");
            /*
            for (int i = 0; i < 30; i++)
            {
                if (i % 2 == 0) //Checkboxes LHS
                {
                    CheckBoxArrayGroup1[i].Location = new System.Drawing.Point(this.Size.Width - scale1, CheckBoxArrayGroup1[i].Location.Y);
                    CheckBoxArrayGroup2[i].Location = new System.Drawing.Point(this.Size.Width - scale1, CheckBoxArrayGroup2[i].Location.Y);
                    CheckBoxArrayGroup3[i].Location = new System.Drawing.Point(this.Size.Width - scale1, CheckBoxArrayGroup3[i].Location.Y);
                }
                else //Checkboxes RHS
                {
                    CheckBoxArrayGroup1[i].Location = new System.Drawing.Point(this.Size.Width - scale2, CheckBoxArrayGroup1[i].Location.Y);
                    CheckBoxArrayGroup2[i].Location = new System.Drawing.Point(this.Size.Width - scale2, CheckBoxArrayGroup2[i].Location.Y);
                    CheckBoxArrayGroup3[i].Location = new System.Drawing.Point(this.Size.Width - scale2, CheckBoxArrayGroup3[i].Location.Y);
                }
            }
            
            ZedGraphControl1.Size = new System.Drawing.Size(this.Size.Width - scale3, ZedGraphControl1.Size.Height);
            ZedGraphControl2.Size = new System.Drawing.Size(this.Size.Width - scale3, ZedGraphControl2.Size.Height);
            ZedGraphControl3.Size = new System.Drawing.Size(this.Size.Width - scale3, ZedGraphControl3.Size.Height);*/
        }

        public void ShowChannelLabels(List<String> names)
        {
            if (this.buttonConnect.InvokeRequired)  // will be in the same thread as the controls to be added
            {
                ShowChannelLabelsCallback d = new ShowChannelLabelsCallback(ShowChannelLabels);
                this.Invoke(d, names);
            }
            else
            {
                for (int i = 0; i < names.Count; i++)
                {
                    System.Windows.Forms.Label lbl = new System.Windows.Forms.Label();
                    lbl.Text = names[i];
                    lbl.Size = new Size(250, 17);
                    lbl.Location = new Point(900, 150 + ((ListofLabels.Count - 1) * 22));
                    ListofLabels.Add(lbl);
                    this.Controls.Add(lbl);
                }
            }
        }


        public void ShowChannelTextBoxes(int count)
        {
            if (this.buttonConnect.InvokeRequired)  // will be in the same thread as the controls to be added
            {
                ShowChannelTextBoxesCallback d = new ShowChannelTextBoxesCallback(ShowChannelTextBoxes);
                this.Invoke(d, count);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    TextBox txtBX = new TextBox();
                    txtBX.Text = ListofTextBox.Count.ToString();
                    txtBX.Location = new Point(1150, 150 + ((ListofTextBox.Count - 1) * 22));
                    ListofTextBox.Add(txtBX);
                    this.Controls.Add(txtBX);
                }
            }
        }

        public void UpdateChannelTextBoxes(List<Double> listofdata)
        {
            if (this.buttonConnect.InvokeRequired)  // will be in the same thread as the controls to be added
            {
                UpdateChannelsCallback d = new UpdateChannelsCallback(UpdateChannelTextBoxes);
                this.Invoke(d, listofdata);
            }
            else
            {
                if (ListofTextBox.Count == listofdata.Count)
                {
                    for (int i = 0; i < listofdata.Count; i++)
                    {
                        ListofTextBox[i].Text = (Math.Truncate(listofdata[i] * 100) / 100).ToString();
                    }
                }
            }
        }

        public void updateExGLeadOffTextBoxes(double ExG1status, double ExG2status)
        {
            if (this.buttonConnect.InvokeRequired)  // will be in the same thread as the controls to be added
            {
                UpdateExGLeadOffDetectionCallback d = new UpdateExGLeadOffDetectionCallback(updateExGLeadOffTextBoxes);
                this.Invoke(d, ExG1status, ExG2status);
            }
            else
            {
                textBoxLeadOffStatus1.Text = ((((byte)ExG1status & (1 << 2)) == 0).ToString());
                textBoxLeadOffStatus2.Text = ((((byte)ExG1status & (1 << 3)) == 0).ToString());
                textBoxLeadOffStatus3.Text = ((((byte)ExG1status & (1 << 0)) == 0).ToString());
                textBoxLeadOffStatus4.Text = ((((byte)ExG2status & (1 << 2)) == 0).ToString());
                textBoxLeadOffStatus5.Text = ((((byte)ExG1status & (1 << 4)) == 0).ToString());
            }
        }

        private void RemoveAllTextBox()
        {
            foreach (TextBox tx in ListofTextBox)
            {
                this.Controls.Remove(tx);
            }
            foreach (System.Windows.Forms.Label l in ListofLabels)
            {
                this.Controls.Remove(l);
            }
            ListofTextBox.Clear();
            ListofSignals.Clear();
            ListofLabels.Clear();
            UserControlGeneralConfig.ExpansionBoard = "";

        }

        /// <summary>
        /// Loads saved signal selections from persistent storage
        /// </summary>
        private void LoadSavedSignalSelections(List<String> availableSignals)
        {
            try
            {
                // Store available signals for later use in dialog
                AvailableSignalNames = new List<String>(availableSignals);

                // Load saved selections
                List<string> savedGraph1, savedGraph2, savedGraph3;
                bool loaded = ShimmerCapture.SignalSelectionSettings.LoadSelections(
                    out savedGraph1, out savedGraph2, out savedGraph3);

                if (loaded)
                {
                    // Validate and filter against available signals (best-effort)
                    SelectedSignalNameGroup1 = ShimmerCapture.SignalSelectionSettings.ValidateAndFilter(
                        savedGraph1, AvailableSignalNames);
                    SelectedSignalNameGroup2 = ShimmerCapture.SignalSelectionSettings.ValidateAndFilter(
                        savedGraph2, AvailableSignalNames);
                    SelectedSignalNameGroup3 = ShimmerCapture.SignalSelectionSettings.ValidateAndFilter(
                        savedGraph3, AvailableSignalNames);

                    System.Diagnostics.Debug.WriteLine($"Loaded signal selections: Graph1={SelectedSignalNameGroup1.Count}, Graph2={SelectedSignalNameGroup2.Count}, Graph3={SelectedSignalNameGroup3.Count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading signal selections: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens the signal selection dialog for user to choose which signals to display
        /// </summary>
        private void OpenSignalSelectionDialog()
        {
            if (AvailableSignalNames == null || AvailableSignalNames.Count == 0)
            {
                MessageBox.Show("No signals available. Please start streaming first.",
                    "No Signals", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var dialog = new ShimmerCapture.SignalSelectionDialog(
                    AvailableSignalNames,
                    SelectedSignalNameGroup1,
                    SelectedSignalNameGroup2,
                    SelectedSignalNameGroup3);

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Update selections
                    SelectedSignalNameGroup1 = dialog.Graph1Selections;
                    SelectedSignalNameGroup2 = dialog.Graph2Selections;
                    SelectedSignalNameGroup3 = dialog.Graph3Selections;

                    // Save selections
                    ShimmerCapture.SignalSelectionSettings.SaveSelections(
                        SelectedSignalNameGroup1,
                        SelectedSignalNameGroup2,
                        SelectedSignalNameGroup3);

                    System.Diagnostics.Debug.WriteLine($"Signal selections updated and saved: Graph1={SelectedSignalNameGroup1.Count}, Graph2={SelectedSignalNameGroup2.Count}, Graph3={SelectedSignalNameGroup3.Count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening signal selection dialog: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void configureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configure = new Configuration(this);
            if (Configure.ShowDialog(this) == DialogResult.OK)
            {
                // Clear signal selections when configuration changes
                SelectedSignalNameGroup1.Clear();
                SelectedSignalNameGroup2.Clear();
                SelectedSignalNameGroup3.Clear();

                if (ShimmerDevice.Is3DOrientationEnabled())
                {
                    ToolStripMenuItemShow3DOrientation.Enabled = true;
                }
                else
                {
                    ToolStripMenuItemShow3DOrientation.Enabled = false;
                }
            }
        }

        private void ConfigureSignalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSignalSelectionDialog();
        }

        private void ToolStripMenuItemQuit_Click(object sender, EventArgs e)
        {
            if (WriteToFile != null)
            {
                WriteToFile.CloseFile();
                ToolStripMenuItemSaveToCSV.Checked = false;
            }

            if (ShimmerDevice != null)
            {
                if (ShimmerDevice.GetState() == (int)ShimmerBluetooth.SHIMMER_STATE_STREAMING)
                {
                    if (ShimmerDevice.GetFirmwareIdentifier() == 3)
                    {

                    }
                    else
                    {
                        ShimmerDevice.StopStreaming();
                        udpListener.StopListener();
                    }
                }
                ShimmerDevice.Disconnect();
                if (Orientation3DForm != null)
                {
                    Orientation3DForm.Close();
                }
            }
            Application.Exit();
        }

        private void ToolStripMenuItemSaveToCSV_Click(object sender, EventArgs e)
        {
            // CSV files are now auto-saved when recording starts
            // This menu item just shows information about the save location
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var dataFolder = Path.Combine(path, "Shimmer_GSR");

                string message;
                if (WriteToFile != null && ToolStripMenuItemSaveToCSV.Checked)
                {
                    message = "CSV logging is currently active.\n\n" +
                             "Files are automatically saved when recording starts.\n\n" +
                             $"Save location:\n{dataFolder}\n\n" +
                             "Click 'Open Data Folder' in the Tools menu to view saved files.";
                }
                else
                {
                    message = "CSV logging will start automatically when you begin recording.\n\n" +
                             $"Files will be saved to:\n{dataFolder}\n\n" +
                             "Filename format: SubjectName_yyyyMMdd_HHmmss.csv\n\n" +
                             "Click 'Open Data Folder' in the Tools menu to view the save location.";
                }

                MessageBox.Show(message, "CSV Auto-Save Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("Error showing CSV save information", ex, "Control.ToolStripMenuItemSaveToCSV_Click");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToolStripMenuItemShow3DOrientation_Click(object sender, EventArgs e)
        {
            if (!ToolStripMenuItemShow3DOrientation.Checked)
            {
                ToolStripMenuItemShow3DOrientation.Checked = true;
                Orientation3DForm = new Orientation3D();
                Orientation3DForm.setControl(this);
                Orientation3DForm.Show();
                Is3DCubeOpen = true;
            }
            else
            {
                ToolStripMenuItemShow3DOrientation.Checked = false;
                if (Orientation3DForm != null)
                {
                    try
                    {
                        Orientation3DForm.Close();
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError("Error closing 3D Orientation form", ex, "Control.ToolStripMenuItemShow3DOrientation_Click");
                    }
                }
                Is3DCubeOpen = false;
            }
        }

        private void ToolStripMenuItemOpenDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var dataFolder = Path.Combine(path, "Shimmer_GSR");

                // Create directory if it doesn't exist
                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                    ErrorLogger.LogInfo($"Created data folder: {dataFolder}", "Control.ToolStripMenuItemOpenDataFolder_Click");
                }

                // Open folder in Explorer
                System.Diagnostics.Process.Start("explorer.exe", dataFolder);
                ErrorLogger.LogInfo($"Opened data folder: {dataFolder}", "Control.ToolStripMenuItemOpenDataFolder_Click");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("Error opening data folder", ex, "Control.ToolStripMenuItemOpenDataFolder_Click");
                MessageBox.Show($"Error opening data folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToolStripMenuItemOpenLogFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string logFolder = Path.GetTempPath();

                // Open folder in Explorer
                System.Diagnostics.Process.Start("explorer.exe", logFolder);
                ErrorLogger.LogInfo($"Opened log folder: {logFolder}", "Control.ToolStripMenuItemOpenLogFolder_Click");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("Error opening log folder", ex, "Control.ToolStripMenuItemOpenLogFolder_Click");
                MessageBox.Show($"Error opening log folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bluetoothSetupHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string helpMessage = "═══════════════════════════════════════\n" +
                                "   BLUETOOTH PAIRING INSTRUCTIONS\n" +
                                "═══════════════════════════════════════\n\n" +
                                "🔑 SHIMMER PAIRING PIN: 1234\n" +
                                "   (You will need this when pairing)\n\n" +
                                "───────────────────────────────────────\n\n" +
                                "If you see \"No Valid Ports\", follow these steps:\n\n" +
                                "1. Turn ON your Shimmer v3 device\n" +
                                "   • Blue LED should blink when powered on\n\n" +
                                "2. Open Windows Settings\n" +
                                "   • Press Win+I or go to Start → Settings\n\n" +
                                "3. Go to: Devices → Bluetooth & other devices\n\n" +
                                "4. Click \"Add Bluetooth or other device\"\n\n" +
                                "5. Select \"Bluetooth\"\n\n" +
                                "6. Wait for your Shimmer to appear\n" +
                                "   • It will show as \"Shimmer3-XXXX\" or similar\n\n" +
                                "7. Click on your Shimmer device to pair\n\n" +
                                "8. ⚠️ ENTER THE PIN: 1234 when prompted\n\n" +
                                "9. Wait for \"Connected\" status\n\n" +
                                "10. Windows will assign a COM port automatically\n\n" +
                                "11. Return to ShimmerCapture and click RELOAD\n\n" +
                                "───────────────────────────────────────\n\n" +
                                "✓ Your device should now appear in the COM port dropdown!\n\n" +
                                "Still having issues?\n" +
                                "• Check error logs: Tools → Open Log Folder\n" +
                                "• Make sure device is powered on\n" +
                                "• Try removing and re-pairing the device";

            MessageBox.Show(helpMessage, "Bluetooth Setup Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetupGraph1(int numberOfTraces, String[] signalNames)
        {
            if (numberOfTraces > maxNumberOfTracesToPlot)
            {
                numberOfTraces = maxNumberOfTracesToPlot;
            }
            if (NumberOfTracesCountGraph1 == 0)
            {
                DataListGraph1.Clear();
                CurvesListGraph1.Clear();
                MyPaneGraph1.CurveList.Clear();
            }

            for (int j = 0; j < numberOfTraces; j++)
            {
                DataListGraph1.Add(new RollingPointPairList(1000));
                CurvesListGraph1.Add(MyPaneGraph1.AddCurve(signalNames[j], DataListGraph1[j], TraceColours[j], SymbolType.None));
                CurvesListGraph1[j].Line.Width = 1.5F;
                NumberOfTracesCountGraph1++;
                if (NumberOfTracesCountGraph1 >= maxNumberOfTracesToPlot)
                {
                    return;
                }
            }
            ZedGraphControl1.AxisChange();
        }

        public void SetupGraph2(int numberOfTraces, String[] signalNames)
        {
            if (numberOfTraces > maxNumberOfTracesToPlot)
            {
                numberOfTraces = maxNumberOfTracesToPlot;
            }
            if (NumberOfTracesCountGraph2 == 0)
            {
                DataListGraph2.Clear();
                CurvesListGraph2.Clear();
                MyPaneGraph2.CurveList.Clear();
            }

            for (int j = 0; j < numberOfTraces; j++)
            {
                DataListGraph2.Add(new RollingPointPairList(1000));
                CurvesListGraph2.Add(MyPaneGraph2.AddCurve(signalNames[j], DataListGraph1[j], TraceColours[j], SymbolType.None));
                CurvesListGraph2[j].Line.Width = 1.5F;
                NumberOfTracesCountGraph2++;
                if (NumberOfTracesCountGraph2 >= maxNumberOfTracesToPlot)
                {
                    return;
                }
            }
            ZedGraphControl2.AxisChange();
        }

        public void SetupGraph3(int numberOfTraces, String[] signalNames)
        {
            if (numberOfTraces > maxNumberOfTracesToPlot)
            {
                numberOfTraces = maxNumberOfTracesToPlot;
            }
            if (NumberOfTracesCountGraph3 == 0)
            {
                DataListGraph3.Clear();
                CurvesListGraph3.Clear();
                MyPaneGraph3.CurveList.Clear();
            }

            for (int j = 0; j < numberOfTraces; j++)
            {
                DataListGraph3.Add(new RollingPointPairList(1000));
                CurvesListGraph3.Add(MyPaneGraph3.AddCurve(signalNames[j], DataListGraph1[j], TraceColours[j], SymbolType.None));
                CurvesListGraph3[j].Line.Width = 1.5F;
                NumberOfTracesCountGraph3++;
                if (NumberOfTracesCountGraph3 >= maxNumberOfTracesToPlot)
                {
                    return;
                }
            }
            ZedGraphControl3.AxisChange();
        }

        private void MyContextMenuBuilder(ZedGraphControl control, ContextMenuStrip menuStrip)
        {
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if ((string)item.Tag == "show_val")
                {
                    //menuStrip.Items.Remove(item);
                    item.Enabled = false;
                    break;
                }
            }
        }

        public void DrawingData(string[] signalName, string[] formats, double[] data)
        {
            if (MyPaneGraph1.CurveList == null || MyPaneGraph2.CurveList == null || MyPaneGraph3.CurveList == null)
                return;

            if ((MyPaneGraph1.CurveList.Count <= 0) || (MyPaneGraph2.CurveList.Count <= 0) || (MyPaneGraph3.CurveList.Count <= 0))
                return;

            for (int i = 0; i < MyPaneGraph1.CurveList.Count; i++)
            {
                MyPaneGraph1.CurveList[i].IsVisible = false;
                MyPaneGraph1.CurveList[i].Label.IsVisible = false;
                MyPaneGraph2.CurveList[i].IsVisible = false;
                MyPaneGraph2.CurveList[i].Label.IsVisible = false;
                MyPaneGraph3.CurveList[i].IsVisible = false;
                MyPaneGraph3.CurveList[i].Label.IsVisible = false;
                LineItem curve1 = MyPaneGraph1.CurveList[i] as LineItem;
                LineItem curve2 = MyPaneGraph2.CurveList[i] as LineItem;
                LineItem curve3 = MyPaneGraph3.CurveList[i] as LineItem;
                if (curve1 == null || curve2 == null || curve3 == null)
                    return;
                if (DataListGraph1[i] == null || DataListGraph2[i] == null || DataListGraph3[i] == null)
                    return;
            }

            for (int j = 0; j < MyPaneGraph1.CurveList.Count; j++)
            {
                DataListGraph1[j].Add(CountXAxisDataPoints, data[j]);
                DataListGraph2[j].Add(CountXAxisDataPoints, data[j]);
                DataListGraph3[j].Add(CountXAxisDataPoints, data[j]);
                //System.Console.WriteLine("j: " + j + "" + "CountXAxisDataPoints: " + CountXAxisDataPoints + "" + "data[j]: " + data[j]);
            }

            for (int k = 0; k < SelectedSignalNameGroup1.Count; k++)
            {
                int index = MyPaneGraph1.CurveList.IndexOf(SelectedSignalNameGroup1[k]);
                if (index != -1)
                {
                    MyPaneGraph1.CurveList[index].IsVisible = true;
                    MyPaneGraph1.CurveList[index].Label.IsVisible = true;
                }
            }

            if (IsGraph2Visible)
            {
                for (int k = 0; k < SelectedSignalNameGroup2.Count; k++)
                {
                    int index = MyPaneGraph2.CurveList.IndexOf(SelectedSignalNameGroup2[k]);
                    if (index != -1)
                    {
                        MyPaneGraph2.CurveList[index].IsVisible = true;
                        MyPaneGraph2.CurveList[index].Label.IsVisible = true;
                    }
                }
            }

            if (IsGraph3Visible)
            {
                for (int k = 0; k < SelectedSignalNameGroup3.Count; k++)
                {
                    int index = MyPaneGraph3.CurveList.IndexOf(SelectedSignalNameGroup3[k]);
                    if (index != -1)
                    {
                        MyPaneGraph3.CurveList[index].IsVisible = true;
                        MyPaneGraph3.CurveList[index].Label.IsVisible = true;
                    }
                }
            }

            Scale xScale1 = MyPaneGraph1.XAxis.Scale;
            xScale1.Max = CountXAxisDataPoints;
            xScale1.Min = CountXAxisDataPoints - 1000;

            Scale xScale2 = MyPaneGraph2.XAxis.Scale;
            xScale2.Max = CountXAxisDataPoints;
            xScale2.Min = CountXAxisDataPoints - 1000;

            Scale xScale3 = MyPaneGraph3.XAxis.Scale;
            xScale3.Max = CountXAxisDataPoints;
            xScale3.Min = CountXAxisDataPoints - 1000;

            //Invalidate() is called to update the data displayed on the graph. This produces a lag. 
            //Only update every 'factor' number of samples
            if (ShimmerDevice.GetSamplingRate() > 1000)
            {
                int factor = 50;
                if (IsGraph2Visible && IsGraph3Visible)
                {
                    factor = 60;
                }
                else if (IsGraph2Visible && (!IsGraph3Visible))
                {
                    factor = 80;
                }

                if (usingLinux)
                {
                    System.Console.WriteLine("Using Linux");
                    factor = 500;
                }

                if (CountXAxisDataPoints % factor == 0)
                {
                    ZedGraphControl1.AxisChange();
                    ZedGraphControl1.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl1, ZedGraphControl1.ContextMenuStrip);

                    ZedGraphControl2.AxisChange();
                    ZedGraphControl2.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl2, ZedGraphControl2.ContextMenuStrip);

                    ZedGraphControl3.AxisChange();
                    ZedGraphControl3.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl3, ZedGraphControl3.ContextMenuStrip);
                }
            }
            else if (ShimmerDevice.GetSamplingRate() > 500)
            {
                int factor = 100;
                if (IsGraph2Visible && IsGraph3Visible)
                {
                    factor = 200;
                }
                else if (IsGraph2Visible && (!IsGraph3Visible))
                {
                    factor = 200;
                }
                if (usingLinux)
                {
                    System.Console.WriteLine("Using Linux");
                    factor = 250;
                }
                if (CountXAxisDataPoints % factor == 0)
                {
                    ZedGraphControl1.AxisChange();
                    ZedGraphControl1.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl1, ZedGraphControl1.ContextMenuStrip);

                    ZedGraphControl2.AxisChange();
                    ZedGraphControl2.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl2, ZedGraphControl2.ContextMenuStrip);

                    ZedGraphControl3.AxisChange();
                    ZedGraphControl3.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl3, ZedGraphControl3.ContextMenuStrip);
                }
            }
            else if (ShimmerDevice.GetSamplingRate() > 200)
            {
                int factor = 40;
                if (IsGraph2Visible && IsGraph3Visible)
                {
                    factor = 40;
                }
                else if (IsGraph2Visible && (!IsGraph3Visible))
                {
                    factor = 80;
                }
                if (usingLinux)
                {
                    System.Console.WriteLine("Using Linux");
                    factor = 100;
                }
                if (CountXAxisDataPoints % factor == 0)
                {
                    ZedGraphControl1.AxisChange();
                    ZedGraphControl1.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl1, ZedGraphControl1.ContextMenuStrip);

                    ZedGraphControl2.AxisChange();
                    ZedGraphControl2.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl2, ZedGraphControl2.ContextMenuStrip);

                    ZedGraphControl3.AxisChange();
                    ZedGraphControl3.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl3, ZedGraphControl3.ContextMenuStrip);
                }
            }
            else
            {
                int factor = 10;
                if (IsGraph2Visible && IsGraph3Visible)
                {
                    factor = 20;
                }
                else if (IsGraph2Visible && (!IsGraph3Visible))
                {
                    factor = 20;
                }
                if (usingLinux)
                {
                    System.Console.WriteLine("Using Linux");
                    factor = 50;
                }
                if (CountXAxisDataPoints % factor == 0)
                {
                    ZedGraphControl1.AxisChange();
                    ZedGraphControl1.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl1, ZedGraphControl1.ContextMenuStrip);

                    ZedGraphControl2.AxisChange();
                    ZedGraphControl2.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl2, ZedGraphControl2.ContextMenuStrip);

                    ZedGraphControl3.AxisChange();
                    ZedGraphControl3.Invalidate();
                    MyContextMenuBuilder(ZedGraphControl3, ZedGraphControl3.ContextMenuStrip);
                }
            }
            CountXAxisDataPoints++;
        }

        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            textBoxShimmerState.Text = value;
        }

        public string GetLoggingFormat()
        {
            return Delimeter;
        }

        public void SetLoggingFormat(string format)
        {
            Delimeter = format;
        }

        private void buttonAddGraph_Click(object sender, EventArgs e)
        {
            if (IsGraph2Visible && (!IsGraph3Visible))
            {
                IsGraph3Visible = true;
                ZedGraphControl3.Visible = true;
                // Signal selection will be handled by SignalSelectionDialog

                if (!usingLinux)
                {
                    int w = this.Size.Width;
                    if (this.Size.Height < 1200)
                    {
                        this.Size = new System.Drawing.Size(w, 1200);
                    }
                }
            }
            else if (!IsGraph2Visible)
            {
                IsGraph2Visible = true;
                ZedGraphControl2.Visible = true;
                // Signal selection will be handled by SignalSelectionDialog

                if (!usingLinux)
                {
                    int w = this.Size.Width;
                    if (this.Size.Height < 797)
                    {
                        this.Size = new System.Drawing.Size(w, 797);
                    }
                }
            }
        }

        private void buttonRemoveGraph_Click(object sender, EventArgs e)
        {
            if (IsGraph3Visible)
            {
                ZedGraphControl3.Visible = false;
                IsGraph3Visible = false;
                SelectedSignalNameGroup3.Clear();
            }
            else if (IsGraph2Visible)
            {
                ZedGraphControl2.Visible = false;
                IsGraph2Visible = false;
                SelectedSignalNameGroup2.Clear();
            }
        }

        bool isConnected = false;
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            RemoveAllTextBox();
            if (!isConnected)
            {
                Connect();
            }
            else
            {
                Stop();
                Disconnect();
            }
        }

        private void SetConnectDisconnect(bool connected)
        {
            isConnected = connected;
            if(connected)
            {
                buttonConnect.Text = "Disconnect";
                comboBoxComPorts.Enabled = false;
                buttonReload.Enabled = false;
            }
            else
            {
                buttonConnect.Text = "Connect";
                comboBoxComPorts.Enabled = true;
                buttonReload.Enabled = true;
            }
        }

        public void Connect()
        {
            ComPort = comboBoxComPorts.Text;
            if(!ComPort.Contains("No Valid")) { 
                ShimmerDevice = new ShimmerLogAndStreamSystemSerialPort("Shimmer", ComPort);
                ShimmerDevice.UICallback += this.HandleEvent;

                //for Shimmer and ShimmerSDBT
                ShimmerDevice.SetShimmerAddress(ComPort);

                //for Shimmer32Feet and ShimmerSDBT32Feet
                //ShimmerBluetooth.SetAddress("00066666940E");
                bool connect = true; // check to connect one at a time

                if (ShimmerDevice.GetState() != ShimmerBluetooth.SHIMMER_STATE_CONNECTED)
                {
                    if (connect)
                    {
                        ShimmerDevice.StartConnectThread();
                        connect = false;
                    }
                }
            }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            
            Stop();
            Disconnect();
            RemoveAllTextBox();
        }

        public void Disconnect()
        {
            if (WriteToFile != null)
            {
                WriteToFile.CloseFile();
                ToolStripMenuItemSaveToCSV.Checked = false;
            }

            labelPRR.Visible = false;
            if (ShimmerDevice != null)
            {
                if (ShimmerDevice.GetState() == (int)ShimmerBluetooth.SHIMMER_STATE_STREAMING)
                {
                    if (ShimmerDevice.GetFirmwareIdentifier() == 3)
                    {

                    }
                    else
                    {
                        ShimmerDevice.StopStreaming();
                        udpListener.StopListener();
                    }
                }
                ShimmerDevice.Disconnect();
                if (Orientation3DForm != null)
                {
                    Orientation3DForm.Close();
                }
            }
            ShimmerIdSetup.Clear();
            StreamingSignalNamesRaw.Clear();
            StreamingSignalNamesCal.Clear();

            // clear all signal selections
            SelectedSignalNameGroup1.Clear();
            SelectedSignalNameGroup2.Clear();
            SelectedSignalNameGroup3.Clear();
        }

        public void EnablePPGtoHR(bool enable)
        {
            EnablePPGtoHRConversion = enable;
        }

        public bool GetEnablePPGtoHR()
        {
            return EnablePPGtoHRConversion;
        }

        public void SetNumberOfBeatsToAve(int number)
        {
            NumberOfHeartBeatsToAverage = number;
        }

        public void SetNumberOfBeatsToAveECG(int number)
        {
            NumberOfHeartBeatsToAverageECG = number;
        }

        public int GetNumberOfBeatsToAve()
        {
            return NumberOfHeartBeatsToAverage;
        }

        public int GetNumberOfBeatsToAveECG()
        {
            return NumberOfHeartBeatsToAverageECG;
        }

        public void EnableECGtoHR(bool enable)
        {
            EnableECGtoHRConversion = enable;
        }

        public bool GetEnableECGtoHR()
        {
            return EnableECGtoHRConversion;
        }


        private void buttonStart_Click(object sender, EventArgs e)
        {
            textBoxSubj.Enabled = false;

            FirstTime = true;
            labelPRR.Visible = true;
            buttonStart_Click1();

            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var subFolderPath = Path.Combine(path, "Shimmer_GSR");

            //Create GSR directory in MyDocuments if it doesn't exist already
            if (!Directory.Exists(subFolderPath))
            {
                Directory.CreateDirectory(subFolderPath);
            }

            // Generate filename with timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = Path.Combine(subFolderPath, $"{textBoxSubj.Text}_{timestamp}.csv");

            try
            {
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                WriteToFile = new Logging(filename, ",", textBoxSubj.Text, version);
                ToolStripMenuItemSaveToCSV.Checked = true;
                ErrorLogger.LogInfo($"Auto-started CSV logging: {filename}", "Control.buttonStart_Click");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Failed to start CSV logging to {filename}", ex, "Control.buttonStart_Click");
                MessageBox.Show($"Warning: Failed to start CSV logging.\n{ex.Message}", "CSV Logging Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Create marker log file
            try
            {
                string markerFilename = Path.Combine(subFolderPath, $"{textBoxSubj.Text}_{timestamp}_markers.csv");
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string deviceInfo = ShimmerDevice != null ? ShimmerDevice.GetDeviceName() : "N/A";
                string deviceSerial = ShimmerDevice != null ? ShimmerDevice.GetShimmerAddress() : "N/A";

                markerLogger = new MarkerLogger(markerFilename, textBoxSubj.Text, deviceInfo, version, deviceSerial);
                ErrorLogger.LogInfo($"Marker log file created: {markerFilename}", "Control.buttonStart_Click");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Failed to create marker log file", ex, "Control.buttonStart_Click");
                // Non-critical error - continue without marker log
            }

            // UDP controls removed - settings now in dialog

            //timer1.Start();  // timer added from toolbox
            SendKeys.Send("{F7}");
            WriteToFile.WriteMarker(7);


        }

        // btsd changes1
        public void buttonStart_Click1()
        {

            SetupFilters();

            //ECG-HR Conversion
            if (EnableECGtoHRConversion)
            {
                ECGtoHR = new ECGToHR(ShimmerDevice.GetSamplingRate(), TrainingPeriodECG, NumberOfHeartBeatsToAverageECG);
            }

            CountXAxisDataPoints = 0;
            CountXAxisDataPoints++;

            Properties.Settings.Default.UDPport = udpPortNo.ToString();
            Properties.Settings.Default.COMport = comboBoxComPorts.SelectedText;
            Properties.Settings.Default.Subject = textBoxSubj.Text;
            Properties.Settings.Default.UDPenabled = udpMarkersEnabled;
            Properties.Settings.Default.Save();

            ShimmerDevice.StartStreamingandLog();

            // Auto-send marker 231 on recording start
            try
            {
                SendUDPMarker(231, "Recording start");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogWarning($"Failed to auto-send marker 231 on recording start: {ex.Message}", "Control.buttonStart_Click1");
            }

            if (udpMarkersEnabled)
            {
                udpListener = new UDPListener(udpPortNo);

                // Set marker logger for inbound markers
                if (markerLogger != null)
                {
                    udpListener.SetMarkerLogger(markerLogger);
                }

                udpListener.NewMessageReceived += delegate (object o, MyMessageArgs msgData)
                {
                    string timestamp =  DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + ".csv";

                    string s = o.ToString() + " " + msgData.data.ToString();
                    if (WriteToFile != null)
                    {
                        WriteToFile.WriteMarker(msgData.data[0]);
                        statusStrip1.Text = ("Marker received: "+timestamp+" : "+s);
                    }
                    else
                    {
                        statusStrip1.Text = ("Marker log file not created!!");
                    }

                };

                udpListener.StartListener(udpMsgLen);
            }
        }


        private void buttonStop_Click(object sender, EventArgs e)
        {
            RemoveAllTextBox();
            Stop();
        }

        private void Stop()
        {
            labelPRR.Visible = false;
            // UDP controls removed - settings now in dialog
            ShimmerDevice.StopStreaming();

            // Auto-send marker 232 on recording stop
            try
            {
                SendUDPMarker(232, "Recording stop");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogWarning($"Failed to auto-send marker 232 on recording stop: {ex.Message}", "Control.Stop");
            }

            udpListener.StopListener();
            buttonStop_Click1();

            SendKeys.Send("{F9}");
            if(WriteToFile!=null)
                WriteToFile.WriteMarker(9);
            //timer1.Stop();
            textBoxSubj.Enabled = true;

        }
        //private delegate void EnableButtonsCallback(int state);

        public void buttonStop_Click1()
        {
            RemoveAllTextBox();
            labelPRR.Visible = false;
            if (WriteToFile != null)
            {
                WriteToFile.CloseFile();
                ToolStripMenuItemSaveToCSV.Checked = false;
            }

            // Close marker log file
            if (markerLogger != null)
            {
                try
                {
                    markerLogger.Close();
                    markerLogger = null;
                    ErrorLogger.LogInfo("Marker log file closed", "Control.buttonStop_Click1");
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("Error closing marker log file", ex, "Control.buttonStop_Click1");
                }
            }

            ShimmerIdSetup.Clear();
            StreamingSignalNamesRaw.Clear();
            StreamingSignalNamesCal.Clear();
            NumberOfTracesCountGraph1 = 0;
            NumberOfTracesCountGraph2 = 0;
            NumberOfTracesCountGraph3 = 0;
            // EnableHPF_0_05HZ
            // EnableHPF_0_5HZ
            // EnableHPF_5HZ
            // EnableBSF_49_51HZ
            // EnableBSF_59_61HZ

            if (EnablePPGtoHRConversion)
            {
                if (LPF_PPG != null)
                {
                    LPF_PPG.resetBuffers();
                }
                if (HPF_PPG != null)
                {
                    HPF_PPG.resetBuffers();
                }
            }
            if (EnableHPF_0_05HZ || EnableHPF_0_5HZ || EnableHPF_5HZ)
            {
                try
                {
                    HPF_Exg1Ch1.resetBuffers();
                    HPF_Exg1Ch2.resetBuffers();
                }
                catch
                {
                }
            }
            if (EnableNQF)
            {
                try
                {
                    NQF_Exg1Ch1.resetBuffers();
                    NQF_Exg1Ch2.resetBuffers();
                }
                catch
                {
                }
            }
            if (EnableNQF)
            {
                try
                {
                    NQF_Exg2Ch1.resetBuffers();
                    NQF_Exg2Ch2.resetBuffers();
                }
                catch
                {
                }
            }
            if (EnableHPF_0_05HZ || EnableHPF_0_5HZ || EnableHPF_5HZ)
            {
                try
                {
                    HPF_Exg2Ch1.resetBuffers();
                    HPF_Exg2Ch2.resetBuffers();
                }
                catch
                {
                }
            }
            if (EnableBSF_49_51HZ || EnableBSF_59_61HZ)
            {
                try
                {
                    BSF_Exg1Ch1.resetBuffers();
                    BSF_Exg1Ch2.resetBuffers();
                }
                catch
                {
                }
            }
            if (EnableBSF_49_51HZ || EnableBSF_59_61HZ)
            {
                try
                {
                    BSF_Exg2Ch1.resetBuffers();
                    BSF_Exg2Ch2.resetBuffers();
                }
                catch
                {
                }
            }
            // clear all signal selections
            SelectedSignalNameGroup1.Clear();
            SelectedSignalNameGroup2.Clear();
            SelectedSignalNameGroup3.Clear();

        }

        private void EnableButtons(int state)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int>(EnableButtons), new object[] { state });
                return;
            }

            if (state == (int)ShimmerBluetooth.SHIMMER_STATE_CONNECTED)
            {
                buttonConnect.Enabled = true;
                SetConnectDisconnect(true);
                //checkBoxTSACheck.Visible = true;
                checkBoxTSACheck.Checked = ShimmerDevice.mEnableTimeStampAlignmentCheck;
                //buttonDisconnect.Enabled = true;
                if (ShimmerDevice.GetFirmwareIdentifier() == 3)
                {
                    buttonStreamandLog.Enabled = true;
                    buttonReadDirectory.Enabled = false;
                    buttonReadDirectory.Visible = false;
                    buttonStreamandLog.Visible = true;
                }
                else
                {
                    buttonStreamandLog.Visible = true;
                    buttonReadDirectory.Visible = false;
                }
                //buttonStream.Enabled = true;
                //buttonStop.Enabled = false;
                configureToolStripMenuItem.Enabled = true;
                buttonStop_Click1();
            }
            // btsd changes3 start
            else if (state == (int)ShimmerBluetooth.SHIMMER_STATE_CONNECTING)
            {
                buttonConnect.Enabled = false;
                //buttonDisconnect.Enabled = false;
                buttonStreamandLog.Enabled = false;
                buttonReadDirectory.Enabled = false;
                SetConnectDisconnect(true);
                buttonConnect.Text = "Connecting...";
                //buttonStream.Enabled = false;
                //buttonStop.Enabled = false;
                configureToolStripMenuItem.Enabled = false;
                ToolStripMenuItemShow3DOrientation.Enabled = false;
            }
            // btsd changes3 end
            else if (state == (int)ShimmerBluetooth.SHIMMER_STATE_NONE)
            {
                buttonConnect.Enabled = true;
                //buttonDisconnect.Enabled = false;
                buttonStreamandLog.Enabled = false;
                buttonReadDirectory.Enabled = false;
                SetConnectDisconnect(false);
                //buttonStream.Enabled = false;
                //buttonStop.Enabled = false;
                configureToolStripMenuItem.Enabled = false;
                ToolStripMenuItemShow3DOrientation.Enabled = false;
            }
            else if (state == (int)ShimmerBluetooth.SHIMMER_STATE_STREAMING)
            {
                buttonConnect.Enabled = true;
                //buttonDisconnect.Enabled = true;
                SetConnectDisconnect(true);
                buttonStreamandLog.Enabled = false;
                //buttonStream.Enabled = false;
                //buttonStop.Enabled = true;
                // btsd changes1
                configureToolStripMenuItem.Enabled = true;
                labelPRR.Visible = true;
            }

            Boolean leadOffDetectionEnabled = getLeadOffDetectionEnabled(); // method returns true if ECG lead off detection is enabled

            labelExGLeadOffDetection.Visible = leadOffDetectionEnabled;
            labelLeadOffStatus1.Visible = leadOffDetectionEnabled;
            labelLeadOffStatus2.Visible = leadOffDetectionEnabled;
            labelLeadOffStatus3.Visible = leadOffDetectionEnabled;
            labelLeadOffStatus4.Visible = leadOffDetectionEnabled;
            labelLeadOffStatus5.Visible = leadOffDetectionEnabled;
            textBoxLeadOffStatus1.Visible = leadOffDetectionEnabled;
            textBoxLeadOffStatus2.Visible = leadOffDetectionEnabled;
            textBoxLeadOffStatus3.Visible = leadOffDetectionEnabled;
            textBoxLeadOffStatus4.Visible = leadOffDetectionEnabled;
            textBoxLeadOffStatus5.Visible = leadOffDetectionEnabled;

        }

        private Boolean getLeadOffDetectionEnabled() // method returns true if ECG lead off detection is enabled
        {

            if (((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0
                                    && (ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0) ||
                                    (((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0
                                    && (ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0)))
            {

                if ((ShimmerDevice.GetState() == (int)ShimmerBluetooth.SHIMMER_STATE_STREAMING) && (ShimmerDevice.GetShimmerVersion() == (int)ShimmerBluetooth.ShimmerVersion.SHIMMER3) && (ShimmerDevice.IsDefaultECGConfigurationEnabled()) && (((ShimmerDevice.GetEXG1RegisterByte(1) & 0x40) != 0) && ((ShimmerDevice.GetEXG2RegisterByte(1) & 0x40) != 0)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void HandleEvent(object sender, EventArgs args)
        {
            CustomEventArgs eventArgs = (CustomEventArgs)args;
            int indicator = eventArgs.getIndicator();

            switch (indicator)
            {
                case (int)ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_STATE_CHANGE:

                    System.Diagnostics.Debug.Write(((ShimmerBluetooth)sender).GetDeviceName() + " State = " + ((ShimmerBluetooth)sender).GetStateString() + System.Environment.NewLine);
                    int state = (int)eventArgs.getObject();
                    if (state == (int)ShimmerBluetooth.SHIMMER_STATE_CONNECTED)
                    {
                        AppendTextBox("Connected");
                        ChangeStatusLabel("Connected to " + ShimmerDevice.GetShimmerAddress() + ". Firmware Version: " + ShimmerDevice.GetFirmwareVersionFullName());
                        EnableButtons((int)ShimmerBluetooth.SHIMMER_STATE_CONNECTED);
                        //buttonStop_Click1();
                    }
                    else if (state == (int)ShimmerBluetooth.SHIMMER_STATE_CONNECTING)
                    {
                        AppendTextBox("Connecting");
                        ChangeStatusLabel("Connecting");
                        // btsd changes3 start
                        EnableButtons((int)ShimmerBluetooth.SHIMMER_STATE_CONNECTING);
                        // btsd changes3 end
                    }
                    else if (state == (int)ShimmerBluetooth.SHIMMER_STATE_NONE)
                    {
                        AppendTextBox("Disconnected");
                        ChangeStatusLabel("Disconnected");
                        EnableButtons((int)ShimmerBluetooth.SHIMMER_STATE_NONE);
                    }
                    else if (state == (int)ShimmerBluetooth.SHIMMER_STATE_STREAMING)
                    {
                        AppendTextBox("Streaming");
                        ChangeStatusLabel("Streaming");
                        EnableButtons((int)ShimmerBluetooth.SHIMMER_STATE_STREAMING);
                        FirstTime = true;
                    }
                    break;
                case (int)ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_NOTIFICATION_MESSAGE:
                    string message = (string)eventArgs.getObject();
                    System.Diagnostics.Debug.Write(((ShimmerBluetooth)sender).GetDeviceName() + message + System.Environment.NewLine);
                    //Message BOX
                    int minorIdentifier = eventArgs.getMinorIndication();
                    if (minorIdentifier == (int)ShimmerLogAndStreamSystemSerialPort.ShimmerSDBTMinorIdentifier.MSG_WARNING)
                    {
                        MessageBox.Show(message, Control.ApplicationName,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (minorIdentifier == (int)ShimmerLogAndStreamSystemSerialPort.ShimmerSDBTMinorIdentifier.MSG_EXTRA_REMOVABLE_DEVICES_DETECTED)
                    {
                        MessageBox.Show(message, "Message");
                        FolderBrowserDialog fbd = new FolderBrowserDialog();
                        DialogResult result = fbd.ShowDialog();
                        ShimmerDevice.SetDrivePath(fbd.SelectedPath);
                    }
                    else if (minorIdentifier == (int)ShimmerLogAndStreamSystemSerialPort.ShimmerSDBTMinorIdentifier.MSG_ERROR)
                    {
                        MessageBox.Show(message, Control.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (message.Equals("Connection lost"))
                    {
                        MessageBox.Show("Connection with device lost while streaming", Control.ApplicationName,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        ChangeStatusLabel(message);
                    }

                    break;
                case (int)ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_DATA_PACKET:
                    // this is essential to ensure the object is not a reference
                    ObjectCluster objectCluster = new ObjectCluster((ObjectCluster)eventArgs.getObject());
                    List<String> names = objectCluster.GetNames();
                    List<String> formats = objectCluster.GetFormats();
                    List<String> units = objectCluster.GetUnits();
                    List<Double> data = objectCluster.GetData();



                    //if (EnablePPGtoHRConversion)
                    //{
                    //    int index = objectCluster.GetIndex(PPGSignalName, "CAL");
                    //    int indexts = objectCluster.GetIndex("Timestamp", "CAL");
                    //    if (index != -1)
                    //    {
                    //        double dataFilteredLP = LPF_PPG.filterData(data[index]);
                    //        double dataFilteredHP = HPF_PPG.filterData(dataFilteredLP);
                    //        double[] dataTS = new double[] { data[indexts] };
                    //        int heartRate = (int)Math.Round(PPGtoHeartRateCalculation.ppgToHrConversion(dataFilteredHP, dataTS[0]));
                    //        names.Add("Heart Rate PPG");
                    //        formats.Add("");
                    //        units.Add("Beats/min");
                    //        data.Add(heartRate);
                    //    }
                    //}


                    ////3D Orientation - Cube
                    //if (ShimmerDevice.Is3DOrientationEnabled() && Is3DCubeOpen)
                    //{
                    //    double[,] rotMatrix = new double[3, 3];
                    //    double theta = (objectCluster.GetData("Axis Angle A", "CAL")).GetData();
                    //    double Rx = (objectCluster.GetData("Axis Angle X", "CAL")).GetData();
                    //    double Ry = (objectCluster.GetData("Axis Angle Y", "CAL")).GetData();
                    //    double Rz = (objectCluster.GetData("Axis Angle Z", "CAL")).GetData();
                    //    double q1 = (objectCluster.GetData("Quaternion 0", "CAL")).GetData();
                    //    double q2 = (objectCluster.GetData("Quaternion 1", "CAL")).GetData();
                    //    double q3 = (objectCluster.GetData("Quaternion 2", "CAL")).GetData();
                    //    double q4 = (objectCluster.GetData("Quaternion 3", "CAL")).GetData();

                    //    //convert to a rotation matrix
                    //    rotMatrix[0, 0] = 2 * q1 * q1 - 1 + 2 * q2 * q2;
                    //    rotMatrix[0, 1] = 2 * (q2 * q3 - q1 * q4);
                    //    rotMatrix[0, 2] = 2 * (q2 * q4 + q1 * q3);
                    //    rotMatrix[1, 0] = 2 * (q2 * q3 + q1 * q4);
                    //    rotMatrix[1, 1] = 2 * q1 * q1 - 1 + 2 * q3 * q3;
                    //    rotMatrix[1, 2] = 2 * (q3 * q4 - q1 * q2);
                    //    rotMatrix[2, 0] = 2 * (q2 * q4 - q1 * q3);
                    //    rotMatrix[2, 1] = 2 * (q3 * q4 + q1 * q2);
                    //    rotMatrix[2, 2] = 2 * q1 * q1 - 1 + 2 * q4 * q4;

                    //    // set function
                    //    if (SetOrientation)
                    //    {
                    //        SetOriMatrix = matrixinverse3x3(rotMatrix);
                    //        SetOrientation = false;
                    //    }

                    //    rotMatrix = matrixmultiplication(SetOriMatrix, rotMatrix);
                    //    AxisAngle aa = new AxisAngle(new Matrix3d(rotMatrix[0, 0], rotMatrix[0, 1], rotMatrix[0, 2], rotMatrix[1, 0], rotMatrix[1, 1], rotMatrix[1, 2], rotMatrix[2, 0], rotMatrix[2, 1], rotMatrix[2, 2]));
                    //    try
                    //    {
                    //        Orientation3DForm.setAxisAngle(aa.angle * 180 / Math.PI, aa.x, aa.y, aa.z);
                    //    }
                    //    catch (System.NullReferenceException)
                    //    {

                    //    }
                    //}

                    ////ExG filtering
                    //if (ShimmerDevice.GetShimmerVersion() == (int)ShimmerBluetooth.ShimmerVersion.SHIMMER3)
                    //{
                    //    if (EnableHPF_0_05HZ || EnableHPF_0_5HZ || EnableHPF_5HZ)
                    //    {
                    //        int[] index1 = new int[2];
                    //        int[] index2 = new int[2];
                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("ECG LL-RA", "CAL");
                    //                    double[] filteredData1 = HPF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("ECG LA-RA", "CAL");
                    //                    double[] filteredData2 = HPF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EMG CH1", "CAL");
                    //                    double[] filteredData1 = HPF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EMG CH2", "CAL");
                    //                    double[] filteredData2 = HPF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EXG1 CH1", "CAL");
                    //                    double[] filteredData1 = HPF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EXG1 CH2", "CAL");
                    //                    double[] filteredData2 = HPF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }
                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = HPF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("ECG Vx-RL", "CAL");
                    //                    double[] filteredData2 = HPF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = HPF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2", "CAL");
                    //                    double[] filteredData2 = HPF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = HPF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2", "CAL");
                    //                    double[] filteredData2 = HPF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }

                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("ECG LL-RA", "CAL");
                    //                    double[] filteredData1 = HPF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("ECG LA-RA", "CAL");
                    //                    double[] filteredData2 = HPF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EMG CH1", "CAL");
                    //                    double[] filteredData1 = HPF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EMG CH2", "CAL");
                    //                    double[] filteredData2 = HPF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EXG1 CH1 16Bit", "CAL");
                    //                    double[] filteredData1 = HPF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EXG1 CH2 16Bit", "CAL");
                    //                    double[] filteredData2 = HPF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }
                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = HPF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("ECG Vx-RL", "CAL");
                    //                    double[] filteredData2 = HPF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = HPF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2", "CAL");
                    //                    double[] filteredData2 = HPF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {

                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1 16Bit", "CAL");
                    //                    double[] filteredData1 = HPF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2 16Bit", "CAL");
                    //                    double[] filteredData2 = HPF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }
                    //    }
                    //    if (EnableBSF_49_51HZ || EnableBSF_59_61HZ)
                    //    {
                    //        int[] index1 = new int[4];
                    //        int[] index2 = new int[4];
                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("ECG LL-RA", "CAL");
                    //                    double[] filteredData1 = BSF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("ECG LA-RA", "CAL");
                    //                    double[] filteredData2 = BSF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EMG CH1", "CAL");
                    //                    double[] filteredData1 = BSF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EMG CH2", "CAL");
                    //                    double[] filteredData2 = BSF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EXG1 CH1", "CAL");
                    //                    double[] filteredData1 = BSF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EXG1 CH2", "CAL");
                    //                    double[] filteredData2 = BSF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }
                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = BSF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("ECG Vx-RL", "CAL");
                    //                    double[] filteredData2 = BSF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = BSF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2", "CAL");
                    //                    double[] filteredData2 = BSF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = BSF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2", "CAL");
                    //                    double[] filteredData2 = BSF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }

                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("ECG LL-RA", "CAL");
                    //                    double[] filteredData1 = BSF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("ECG LA-RA", "CAL");
                    //                    double[] filteredData2 = BSF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EMG CH1", "CAL");
                    //                    double[] filteredData1 = BSF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EMG CH2", "CAL");
                    //                    double[] filteredData2 = BSF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EXG1 CH1 16Bit", "CAL");
                    //                    double[] filteredData1 = BSF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EXG1 CH2 16Bit", "CAL");
                    //                    double[] filteredData2 = BSF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }
                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = BSF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("ECG Vx-RL", "CAL");
                    //                    double[] filteredData2 = BSF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = BSF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2", "CAL");
                    //                    double[] filteredData2 = BSF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1 16Bit", "CAL");
                    //                    double[] filteredData1 = BSF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2 16Bit", "CAL");
                    //                    double[] filteredData2 = BSF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }
                    //    }
                    //    if (EnableNQF)
                    //    {
                    //        int[] index1 = new int[4];
                    //        int[] index2 = new int[4];
                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("ECG LL-RA", "CAL");
                    //                    double[] filteredData1 = NQF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("ECG LA-RA", "CAL");
                    //                    double[] filteredData2 = NQF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EMG CH1", "CAL");
                    //                    double[] filteredData1 = NQF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EMG CH2", "CAL");
                    //                    double[] filteredData2 = NQF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EXG1 CH1", "CAL");
                    //                    double[] filteredData1 = NQF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EXG1 CH2", "CAL");
                    //                    double[] filteredData2 = NQF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }
                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = NQF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("ECG Vx-RL", "CAL");
                    //                    double[] filteredData2 = NQF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = NQF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2", "CAL");
                    //                    double[] filteredData2 = NQF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = NQF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2", "CAL");
                    //                    double[] filteredData2 = NQF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }

                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("ECG LL-RA", "CAL");
                    //                    double[] filteredData1 = NQF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("ECG LA-RA", "CAL");
                    //                    double[] filteredData2 = NQF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EMG CH1", "CAL");
                    //                    double[] filteredData1 = NQF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EMG CH2", "CAL");
                    //                    double[] filteredData2 = NQF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index1[0] = objectCluster.GetIndex("EXG1 CH1 16Bit", "CAL");
                    //                    double[] filteredData1 = NQF_Exg1Ch1.filterData(new double[] { data[index1[0]] });
                    //                    data[index1[0]] = filteredData1[0];
                    //                    index1[1] = objectCluster.GetIndex("EXG1 CH2 16Bit", "CAL");
                    //                    double[] filteredData2 = NQF_Exg1Ch2.filterData(new double[] { data[index1[1]] });
                    //                    data[index1[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }
                    //        if ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0)
                    //        {
                    //            if (ShimmerDevice.IsDefaultECGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = NQF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("ECG Vx-RL", "CAL");
                    //                    double[] filteredData2 = NQF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else if (ShimmerDevice.IsDefaultEMGConfigurationEnabled())
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1", "CAL");
                    //                    double[] filteredData1 = NQF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2", "CAL");
                    //                    double[] filteredData2 = NQF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    index2[0] = objectCluster.GetIndex("EXG2 CH1 16Bit", "CAL");
                    //                    double[] filteredData1 = NQF_Exg2Ch1.filterData(new double[] { data[index2[0]] });
                    //                    data[index2[0]] = filteredData1[0];
                    //                    index2[1] = objectCluster.GetIndex("EXG2 CH2 16Bit", "CAL");
                    //                    double[] filteredData2 = NQF_Exg2Ch2.filterData(new double[] { data[index2[1]] });
                    //                    data[index2[1]] = filteredData2[0];
                    //                }
                    //                catch
                    //                {
                    //                }
                    //            }
                    //        }
                    //    }
                    //}


                    //if (EnableECGtoHRConversion)
                    //{
                    //    //ECG-HR Conversion
                    //    int index = -1;
                    //    if (ShimmerDevice.GetShimmerVersion() == (int)ShimmerBluetooth.ShimmerVersion.SHIMMER3)
                    //    {
                    //        index = objectCluster.GetIndex(ECGSignalName, "RAW");
                    //    }
                    //    else
                    //    {
                    //        index = objectCluster.GetIndex(ECGSignalName, "RAW");
                    //    }
                    //    if (index != -1)
                    //    {
                    //        int hr = -1;
                    //        double ecgData = -1;
                    //        ecgData = data[index];
                    //        double calTimestamp = objectCluster.GetData("Timestamp", "CAL").GetData();
                    //        hr = (int)ECGtoHR.ECGToHRConversion(ecgData, calTimestamp);
                    //        names.Add("Heart Rate ECG");
                    //        formats.Add("");
                    //        units.Add("Beats/min");
                    //        data.Add(hr);
                    //    }
                    //}
                    ///*
                    //if (UserControlExgConfig.comboBoxLeadOffDetection != null) // method to extract ExG status channels if ExG lead off detection is enabled
                    //{
                    //}
                    //*/
                    //if (((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0
                    //    && (ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0) ||
                    //    (((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0
                    //    && (ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0)))
                    //{

                    //    if ((ShimmerDevice.GetShimmerVersion() == (int)ShimmerBluetooth.ShimmerVersion.SHIMMER3) && (ShimmerDevice.IsDefaultECGConfigurationEnabled()) && (((ShimmerDevice.GetEXG1RegisterByte(1) & 0x40) != 0) && ((ShimmerDevice.GetEXG2RegisterByte(1) & 0x40) != 0)))
                    //    {
                    //        try
                    //        {
                    //            int ExG1statusIndex = objectCluster.GetIndex("EXG1 Sta", "RAW");
                    //            int ExG2statusIndex = objectCluster.GetIndex("EXG2 Sta", "RAW");
                    //            double ExG1status = objectCluster.GetData("EXG1 Sta", "RAW").GetData();
                    //            double ExG2status = objectCluster.GetData("EXG2 Sta", "RAW").GetData();
                    //            if (ExGLeadOffCounter < ExGLeadOffCounterSize) // call updateExGLeadOff code once every second
                    //            {
                    //                ExGLeadOffCounter++;
                    //            }
                    //            else
                    //            {
                    //                ExGLeadOffCounter = 0;
                    //                updateExGLeadOffTextBoxes(ExG1status, ExG2status);
                    //            }
                    //        }
                    //        catch
                    //        {
                    //        }
                    //    }
                    //}

                    ////Set up checkboxes and graph first time
                    ///*
                    //if (!ShimmerIdSetup.Contains(objectCluster.GetShimmerID()))
                    //{
                    //    List<String> signalNamesandFormats = new List<String>();
                    //    List<String> signalNamesandFormatsRaw = new List<String>();
                    //    List<String> signalNamesandFormatsCal = new List<String>();
                    //    for (int i = 0; i < names.Count; i++)
                    //    {
                    //        //RAW and CAL signals separated to allow for checkbox set up - RAW on LHS, CAL on RHS.
                    //        if (formats[i].Equals("RAW"))
                    //        {
                    //            signalNamesandFormatsRaw.Add(names[i] + " " + formats[i]);
                    //            StreamingSignalNamesRaw.Add(names[i] + " " + formats[i]);
                    //        }
                    //        else
                    //        {
                    //            signalNamesandFormatsCal.Add(names[i] + " " + formats[i]);
                    //            StreamingSignalNamesCal.Add(names[i] + " " + formats[i]);
                    //        }
                    //        signalNamesandFormats.Add(names[i] + " " + formats[i]);
                    //    }
                    //    SetupCheckboxesGroup1(objectCluster.GetShimmerID(), signalNamesandFormatsRaw.ToArray(), signalNamesandFormatsCal.ToArray());
                    //    SetupCheckboxesGroup2(objectCluster.GetShimmerID(), signalNamesandFormatsRaw.ToArray(), signalNamesandFormatsCal.ToArray());
                    //    SetupCheckboxesGroup3(objectCluster.GetShimmerID(), signalNamesandFormatsRaw.ToArray(), signalNamesandFormatsCal.ToArray());
                    //    SetupGraph1(data.Count, signalNamesandFormats.ToArray());
                    //    SetupGraph2(data.Count, signalNamesandFormats.ToArray());
                    //    SetupGraph3(data.Count, signalNamesandFormats.ToArray());
                    //    ShimmerIdSetup.Add(objectCluster.GetShimmerID());
                    //}
                    

                    if (FirstTime)
                    {
                        List<String> fnames = new List<String>();
                        int icount = 0;
                        foreach (String s in names)
                        {
                            fnames.Add(s + " " + formats[icount] + " (" + units[icount] + ")");
                            icount++;
                        }
                        ShowChannelLabels(fnames);
                        ShowChannelTextBoxes(fnames.Count);

                        //checkboxes

                        List<String> signalNamesandFormats = new List<String>();
                        List<String> signalNamesandFormatsRaw = new List<String>();
                        List<String> signalNamesandFormatsCal = new List<String>();
                        for (int i = 0; i < names.Count; i++)
                        {
                            //RAW and CAL signals separated to allow for checkbox set up - RAW on LHS, CAL on RHS.
                            if (formats[i].Equals("RAW"))
                            {
                                signalNamesandFormatsRaw.Add(names[i] + " " + formats[i]);
                                StreamingSignalNamesRaw.Add(names[i] + " " + formats[i]);
                            }
                            else
                            {
                                signalNamesandFormatsCal.Add(names[i] + " " + formats[i]);
                                StreamingSignalNamesCal.Add(names[i] + " " + formats[i]);
                            }
                            signalNamesandFormats.Add(names[i] + " " + formats[i]);
                        }
                        // Setup graphs
                        SetupGraph1(data.Count, signalNamesandFormats.ToArray());
                        SetupGraph2(data.Count, signalNamesandFormats.ToArray());
                        SetupGraph3(data.Count, signalNamesandFormats.ToArray());
                        ShimmerIdSetup.Add(objectCluster.GetShimmerID());

                        // Load saved signal selections (best-effort)
                        LoadSavedSignalSelections(signalNamesandFormats);

                        FirstTime = false;
                    }


                    
                    ShowTBcount++;
                    if (ShowTBcount % Math.Truncate(ShimmerDevice.GetSamplingRate() / 5) == 0)
                    {
                        UpdateChannelTextBoxes(data);
                    }

                    //Write to file
                    if (WriteToFile != null && ToolStripMenuItemSaveToCSV.Checked != false)
                    {
                        WriteToFile.WriteData(objectCluster);
                        //ToolStripMenuItemSaveToCSV.Checked = false
                    }
                    else
                    {
                        //Plot
                        DrawingData(names.ToArray(), formats.ToArray(), data.ToArray());
                    }

                    break;
                case (int)ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_PACKET_RECEPTION_RATE:
                    double prr = (double)eventArgs.getObject();
                    count++;
                    if (count % Math.Truncate(ShimmerDevice.GetSamplingRate()) == 0)
                    {
                        SetLabelText("Packet Reception Rate: " + Math.Truncate(prr).ToString() + "%");
                    }
                    break;
            }
        }

        delegate void SetTextCallback(string text);

        private void SetButtonText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.buttonStreamandLog.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetButtonText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.buttonStreamandLog.Text = text;
            }
        }

        delegate void SetLabelCallback(string text);

        private void SetLabelText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.labelPRR.InvokeRequired)
            {
                SetLabelCallback d = new SetLabelCallback(SetLabelText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.labelPRR.Text = text;
            }
        }

        public void setTheOrientation()
        {
            SetOrientation = true;
        }

        internal void resetTheOrientation()
        {
            SetOriMatrix = new double[,] { { 1, 0, 0 }, { 0, -1, 0 }, { 0, 0, -1 } };
        }

        private double[,] matrixmultiplication(double[,] a, double[,] b)
        {
            int aRows = a.GetLength(0),
                aColumns = a.GetLength(1),
                bRows = b.GetLength(0),
                bColumns = b.GetLength(1);
            double[,] resultant = new double[aRows, bColumns];

            for (int i = 0; i < aRows; i++)
            { // aRow
                for (int j = 0; j < bColumns; j++)
                { // bColumn
                    for (int k = 0; k < aColumns; k++)
                    { // aColumn
                        resultant[i, j] += a[i, k] * b[k, j];
                    }
                }
            }

            return resultant;
        }

        private double[,] matrixinverse3x3(double[,] data)
        {
            double a, b, c, d, e, f, g, h, i;
            a = data[0, 0];
            b = data[0, 1];
            c = data[0, 2];
            d = data[1, 0];
            e = data[1, 1];
            f = data[1, 2];
            g = data[2, 0];
            h = data[2, 1];
            i = data[2, 2];
            //
            double deter = a * e * i + b * f * g + c * d * h - c * e * g - b * d * i - a * f * h;
            double[,] answer = new double[3, 3];
            answer[0, 0] = (1 / deter) * (e * i - f * h);
            answer[0, 1] = (1 / deter) * (c * h - b * i);
            answer[0, 2] = (1 / deter) * (b * f - c * e);
            answer[1, 0] = (1 / deter) * (f * g - d * i);
            answer[1, 1] = (1 / deter) * (a * i - c * g);
            answer[1, 2] = (1 / deter) * (c * d - a * f);
            answer[2, 0] = (1 / deter) * (d * h - e * g);
            answer[2, 1] = (1 / deter) * (g * b - a * h);
            answer[2, 2] = (1 / deter) * (a * e - b * d);
            return answer;
        }

        private void SetupFilters()
        {
            //Create NQ Filters
            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0)) && EnableNQF)
            {
                double cutoff = ShimmerDevice.GetSamplingRate() / 2;
                NQF_Exg1Ch1 = new Filter(Filter.LOW_PASS, ShimmerDevice.GetSamplingRate(), new double[] { cutoff });
                NQF_Exg1Ch2 = new Filter(Filter.LOW_PASS, ShimmerDevice.GetSamplingRate(), new double[] { cutoff });

            }

            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0)) && EnableNQF)
            {
                double cutoff = ShimmerDevice.GetSamplingRate() / 2;
                NQF_Exg2Ch1 = new Filter(Filter.LOW_PASS, ShimmerDevice.GetSamplingRate(), new double[] { cutoff });
                NQF_Exg2Ch2 = new Filter(Filter.LOW_PASS, ShimmerDevice.GetSamplingRate(), new double[] { cutoff });

            }

            //Create High Pass Filters for EXG

            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0)) && EnableHPF_0_05HZ)
            {
                HPF_Exg1Ch1 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 0.05 });
                HPF_Exg1Ch2 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 0.05 });
            }

            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0)) && EnableHPF_0_05HZ)
            {
                HPF_Exg2Ch1 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 0.05 });
                HPF_Exg2Ch2 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 0.05 });
            }
            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0)) && EnableHPF_0_5HZ)
            {
                HPF_Exg1Ch1 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 0.5 });
                HPF_Exg1Ch2 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 0.5 });
            }
            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0)) && EnableHPF_0_5HZ)
            {
                HPF_Exg2Ch1 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 0.5 });
                HPF_Exg2Ch2 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 0.5 });
            }
            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0)) && EnableHPF_5HZ)
            {
                HPF_Exg1Ch1 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 5 });
                HPF_Exg1Ch2 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 5 });
            }
            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0)) && EnableHPF_5HZ)
            {
                HPF_Exg2Ch1 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 5 });
                HPF_Exg2Ch2 = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 5 });
            }

            //Create Band Stop Filters for EXG
            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0)) && EnableBSF_49_51HZ)
            {
                BSF_Exg1Ch1 = new Filter(Filter.BAND_STOP, ShimmerDevice.GetSamplingRate(), new double[] { 49, 51 });
                BSF_Exg1Ch2 = new Filter(Filter.BAND_STOP, ShimmerDevice.GetSamplingRate(), new double[] { 49, 51 });
            }
            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0)) && EnableBSF_49_51HZ)
            {
                BSF_Exg2Ch1 = new Filter(Filter.BAND_STOP, ShimmerDevice.GetSamplingRate(), new double[] { 49, 51 });
                BSF_Exg2Ch2 = new Filter(Filter.BAND_STOP, ShimmerDevice.GetSamplingRate(), new double[] { 49, 51 });
            }
            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG1_16BIT) > 0)) && EnableBSF_59_61HZ)
            {
                BSF_Exg1Ch1 = new Filter(Filter.BAND_STOP, ShimmerDevice.GetSamplingRate(), new double[] { 59, 61 });
                BSF_Exg1Ch2 = new Filter(Filter.BAND_STOP, ShimmerDevice.GetSamplingRate(), new double[] { 59, 61 });
            }
            if ((((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_24BIT) > 0) || ((ShimmerDevice.GetEnabledSensors() & (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_EXG2_16BIT) > 0)) && EnableBSF_59_61HZ)
            {
                BSF_Exg2Ch1 = new Filter(Filter.BAND_STOP, ShimmerDevice.GetSamplingRate(), new double[] { 59, 61 });
                BSF_Exg2Ch2 = new Filter(Filter.BAND_STOP, ShimmerDevice.GetSamplingRate(), new double[] { 59, 61 });
            }

            //PPG-HR Conversion
            if (EnablePPGtoHRConversion)
            {
                PPGtoHeartRateCalculation = new PPGToHRAlgorithm(ShimmerDevice.GetSamplingRate(), NumberOfHeartBeatsToAverage, TrainingPeriodPPG);
                LPF_PPG = new Filter(Filter.LOW_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 5 });
                HPF_PPG = new Filter(Filter.HIGH_PASS, ShimmerDevice.GetSamplingRate(), new double[] { 0.5 });
            }

        }

        private void buttonStream_Click(object sender, EventArgs e)
        {
            //int[] a = new int [1];
            //a[2] = 3;
            FirstTime = true;
            labelPRR.Visible = true;

            SetupFilters();

            //ECG-HR Conversion
            if (EnableECGtoHRConversion)
            {
                ECGtoHR = new ECGToHR(ShimmerDevice.GetSamplingRate(), TrainingPeriodECG, NumberOfHeartBeatsToAverageECG);
            }
            ExGLeadOffCounter = 0;
            ExGLeadOffCounterSize = (int)ShimmerDevice.GetSamplingRate();
            ShimmerIdSetup.Clear();
            StreamingSignalNamesRaw.Clear();
            StreamingSignalNamesCal.Clear();
            NumberOfTracesCountGraph1 = 0;
            NumberOfTracesCountGraph2 = 0;
            NumberOfTracesCountGraph3 = 0;
            CountXAxisDataPoints = 0;
            CountXAxisDataPoints++;
            ShimmerDevice.StartStreaming();

            udpListener = new UDPListener(udpPortNo);
            udpListener.StartListener(udpMsgLen);

        }

        private int getNumPorts()
        {
            String[] names = SerialPort.GetPortNames();
            return names.Length;
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            string curComPort = comboBoxComPorts.SelectedText;
            comboBoxComPorts.Items.Clear();
            String[] names = SerialPort.GetPortNames();
            int i = 0;
            int numPorts = names.Length;
            bool COMfound = false;
            foreach (String s in names)
            {
                comboBoxComPorts.Items.Add(s);
                if(s==curComPort)
                {
                    comboBoxComPorts.SelectedIndex = i;
                    COMfound = true;
                }
                i++;
                

            }
            if (!COMfound&&i>0)
            {
                comboBoxComPorts.SelectedIndex = i-1;
                buttonConnect.Enabled = true;
            }
            else if(i==0||numPorts==0)
            {
                comboBoxComPorts.Items.Clear();
                comboBoxComPorts.Text = "No Valid Ports";
                buttonConnect.Enabled = false;
            }

            
        }

        private void labelPRR_Click(object sender, EventArgs e)
        {

        }

        private void ZedGraphControl1_Load(object sender, EventArgs e)
        {

        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormUpdateCheck ImportForm = new FormUpdateCheck();
            ImportForm.ShowDialog(this);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout AboutForm = new FormAbout();
            AboutForm.ShowDialog(this);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            FormUpdateCheck ImportForm = new FormUpdateCheck();
            ImportForm.ShowDialog(this);
        }

        private void buttonReadDirectory_Click(object sender, EventArgs e)
        {
            ShimmerDevice.ReadDirectory();
        }

        private void comboBoxComPorts_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Console.WriteLine("Set Blink LED");
            if (ShimmerDevice.GetBlinkLED() == 0)
            {
                ShimmerDevice.WriteBlinkLED(2);
            }
            else if (ShimmerDevice.GetBlinkLED() == 2)
            {
                ShimmerDevice.WriteBlinkLED(0);
            }
        }

        private void checkBoxTSACheck_CheckedChanged(object sender, EventArgs e)
        {
            ShimmerDevice.mEnableTimeStampAlignmentCheck = checkBoxTSACheck.Checked;

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            PPGtoHeartRateCalculation.resetParameters();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            //MessageBox.Show("Timer ticked");
            //SendKeys.Send("{F8}");
            WriteToFile.WriteMarker(8);
        }

        private void labelComPort_Click(object sender, EventArgs e)
        {

        }

        // Removed - textBox_udpPort no longer exists, use UDP Settings Dialog instead
        //private void textBox_udpPort_TextChanged(object sender, EventArgs e)
        //{
        //    uint outPort;
        //    uint.TryParse(textBox_udpPort.Text,out outPort);
        //    if (outPort > 80 && outPort < 65500)
        //        udpPortNo = (int)outPort;
        //    else
        //    {
        //        textBox_udpPort.Text = outPort.ToString();
        //    }
        //}

        bool udpMarkersEnabled = true;

        // Removed - checkBox91 no longer exists, use UDP Settings Dialog instead
        //private void checkBox91_CheckedChanged(object sender, EventArgs e)
        //{
        //    udpMarkersEnabled = checkBox91.Checked;
        //    textBox_udpPort.Enabled = udpMarkersEnabled;
        //}

        #region Manual Marker Functionality

        // InitializeManualMarkerControls() - REMOVED
        // Controls are now defined in Control.Designer.cs (groupBoxManualMarker and children)
        // No longer need to dynamically create controls at runtime

        private void LoadManualMarkerSettings()
        {
            try
            {
                textBoxMarkerIP.Text = Properties.Settings.Default.ManualMarkerIP;
                textBoxMarkerPort.Text = Properties.Settings.Default.ManualMarkerPort;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("Error loading manual marker settings", ex, "Control.LoadManualMarkerSettings");
                // Use defaults if loading fails
                textBoxMarkerIP.Text = "127.0.0.1";
                textBoxMarkerPort.Text = "5501";
            }

            // Update status display after loading settings
            UpdateUDPStatusDisplay();
        }

        private void SaveManualMarkerSettings()
        {
            try
            {
                Properties.Settings.Default.ManualMarkerIP = textBoxMarkerIP.Text;
                Properties.Settings.Default.ManualMarkerPort = textBoxMarkerPort.Text;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("Error saving manual marker settings", ex, "Control.SaveManualMarkerSettings");
            }
        }

        private void SendUDPMarker(int markerValue, string description)
        {
            string ip = textBoxMarkerIP.Text;
            string portText = textBoxMarkerPort.Text;

            try
            {
                // Parse port number
                if (!int.TryParse(portText, out int port))
                {
                    ErrorLogger.LogError($"Invalid port number: {portText}", null, "Control.SendUDPMarker");
                    MessageBox.Show("Invalid port number. Please enter a valid port (e.g., 5501).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create UDP client and send marker
                using (var udpClient = new System.Net.Sockets.UdpClient())
                {
                    var endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port);
                    byte[] data = BitConverter.GetBytes(markerValue);

                    // Send in big-endian format (consistent with udp_to_key.py)
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(data);
                    }

                    udpClient.Send(data, data.Length, endpoint);

                    // Log the send
                    ErrorLogger.LogInfo($"UDP marker sent: {description} (value={markerValue}) to {ip}:{port}", "Control.SendUDPMarker");

                    // Log outbound marker to marker log
                    if (markerLogger != null)
                    {
                        try
                        {
                            markerLogger.LogOutboundMarker(markerValue);
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Error logging outbound marker", ex, "Control.SendUDPMarker");
                        }
                    }

                    // Also write to CSV file if logging is active
                    if (WriteToFile != null)
                    {
                        try
                        {
                            WriteToFile.WriteMarker(markerValue);
                            ErrorLogger.LogInfo($"Marker {markerValue} written to CSV file", "Control.SendUDPMarker");
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Error writing marker to CSV file", ex, "Control.SendUDPMarker");
                        }
                    }
                }

                // Save settings after successful send
                SaveManualMarkerSettings();
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                ErrorLogger.LogError($"Socket error sending UDP marker to {ip}:{portText}", ex, "Control.SendUDPMarker");
                MessageBox.Show($"Failed to send marker: {ex.Message}", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FormatException ex)
            {
                ErrorLogger.LogError($"Invalid IP address format: {ip}", ex, "Control.SendUDPMarker");
                MessageBox.Show("Invalid IP address format. Please enter a valid IP (e.g., 127.0.0.1).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Unexpected error sending UDP marker", ex, "Control.SendUDPMarker");
                MessageBox.Show($"Failed to send marker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonSendMarker_Click(object sender, EventArgs e)
        {
            SendUDPMarker(230, "Manual marker");
        }

        /// <summary>
        /// Event handler for UDP messages received
        /// The UdpListener handles writing to file, this is for any additional processing
        /// </summary>
        private void UDP_NewMessageReceived(object sender, MyMessageArgs e)
        {
            try
            {
                // The UdpListener already writes the marker to file via SetWriteToFile
                // This handler is for any additional UI updates or processing
                System.Diagnostics.Debug.WriteLine($"UDP marker received: {BitConverter.ToString(e.data)}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing UDP message: {ex.Message}");
            }
        }

        /// <summary>
        /// Restarts the UDP listener gracefully with a new port
        /// </summary>
        private void RestartUDPListener(int newPort)
        {
            try
            {
                // Stop existing listener
                if (udpListener != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Stopping UDP listener on port {udpPortNo}");
                    udpListener.StopListener();
                    udpListener.Dispose();
                    udpListener = null;
                }

                // Update port
                udpPortNo = newPort;

                // Start new listener if markers are enabled
                if (udpMarkersEnabled)
                {
                    System.Diagnostics.Debug.WriteLine($"Starting UDP listener on port {udpPortNo}");
                    udpListener = new UDPListener(udpPortNo);
                    udpListener.SetWriteToFile(WriteToFile);
                    udpListener.NewMessageReceived += new EventHandler<MyMessageArgs>(UDP_NewMessageReceived);
                    udpListener.StartListener(4);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restarting UDP listener: {ex.Message}");
                MessageBox.Show($"Failed to restart UDP listener on port {newPort}: {ex.Message}\n\nPlease try a different port.",
                    "UDP Listener Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Updates the UDP status display label
        /// </summary>
        private void UpdateUDPStatusDisplay()
        {
            if (labelUDPStatus != null)
            {
                if (udpMarkersEnabled)
                {
                    string outIP = textBoxMarkerIP != null ? textBoxMarkerIP.Text : "127.0.0.1";
                    string outPort = textBoxMarkerPort != null ? textBoxMarkerPort.Text : "5501";
                    labelUDPStatus.Text = $"UDP Markers: Enabled\n" +
                                         $"Inbound: Port {udpPortNo}\n" +
                                         $"Outbound: {outIP}:{outPort}";
                    labelUDPStatus.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    labelUDPStatus.Text = "UDP Markers: Disabled\n(Click to configure)";
                    labelUDPStatus.ForeColor = System.Drawing.Color.Gray;
                }
            }
        }

        /// <summary>
        /// Opens the UDP marker settings dialog
        /// </summary>
        private void OpenUDPMarkerSettings()
        {
            try
            {
                // Get current settings
                string currentIP = textBoxMarkerIP != null ? textBoxMarkerIP.Text : "127.0.0.1";
                string currentPortText = textBoxMarkerPort != null ? textBoxMarkerPort.Text : "5501";
                int.TryParse(currentPortText, out int currentOutboundPort);
                if (currentOutboundPort == 0) currentOutboundPort = 5501;

                // Open settings dialog
                var dialog = new UdpMarkerSettingsDialog(
                    udpMarkersEnabled,
                    udpPortNo,
                    currentIP,
                    currentOutboundPort);

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Update enabled state
                    bool wasEnabled = udpMarkersEnabled;
                    udpMarkersEnabled = dialog.MarkersEnabled;

                    // Update outbound settings
                    if (textBoxMarkerIP != null)
                    {
                        textBoxMarkerIP.Text = dialog.OutboundIP;
                    }
                    if (textBoxMarkerPort != null)
                    {
                        textBoxMarkerPort.Text = dialog.OutboundPort.ToString();
                    }

                    // Restart listener if port changed or enable state changed
                    if (dialog.InboundPort != udpPortNo || wasEnabled != udpMarkersEnabled)
                    {
                        RestartUDPListener(dialog.InboundPort);
                    }

                    // Save settings
                    Properties.Settings.Default.UDPenabled = udpMarkersEnabled;
                    Properties.Settings.Default.UDPport = dialog.InboundPort.ToString();
                    Properties.Settings.Default.ManualMarkerIP = dialog.OutboundIP;
                    Properties.Settings.Default.ManualMarkerPort = dialog.OutboundPort.ToString();
                    Properties.Settings.Default.Save();

                    // Update status display
                    UpdateUDPStatusDisplay();

                    System.Diagnostics.Debug.WriteLine($"UDP settings updated: Enabled={udpMarkersEnabled}, InPort={dialog.InboundPort}, OutIP={dialog.OutboundIP}, OutPort={dialog.OutboundPort}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening UDP settings dialog: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void button_ConfigureSignals_Click(object sender, EventArgs e)
        {
            OpenSignalSelectionDialog();
        }

        private void configureSignalDisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSignalSelectionDialog();
        }

        private void configureUDPMarkersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenUDPMarkerSettings();
        }

        private void labelExGLeadOffDetection_Click(object sender, EventArgs e)
        {

        }

        private void labelLeadOffStatus4_Click(object sender, EventArgs e)
        {

        }

        private void labelLeadOffStatus3_Click(object sender, EventArgs e)
        {

        }

        private void labelLeadOffStatus5_Click(object sender, EventArgs e)
        {

        }

        private void labelLeadOffStatus2_Click(object sender, EventArgs e)
        {

        }

        private void textBoxLeadOffStatus5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxLeadOffStatus4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxLeadOffStatus2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxLeadOffStatus3_TextChanged(object sender, EventArgs e)
        {

        }

        private void labelLeadOffStatus1_Click(object sender, EventArgs e)
        {

        }
    }


    // ExceptionReporter Class
    internal class ExceptionEventHandler
    {
        private static readonly string ApplicationName = Application.ProductName.ToString().Replace("_", " ");
        //    private string versionNumber = Application.ProductVersion.ToString().Substring(0, Application.ProductVersion.ToString().LastIndexOf(".")).ToLower();
        private string versionNumber = Application.ProductVersion.ToString();

        public void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //ReportCrash(e.ExceptionObject as Exception);
            Environment.Exit(0);
        }

        public void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //ReportCrash(e.Exception);
            Environment.Exit(0);
        }

        private static void ReportCrash(Exception exception)
        {

        }
    }


}
