# ShimmerAPI_Tool

A Windows application for interfacing with Shimmer v3 wearable sensors. Based on the official [Shimmer C# SDK](https://github.com/ShimmerEngineering/Shimmer-C-API).

**Version**: 1.0.0
**Framework**: .NET Framework 4.6.1
**Platform**: Windows (x86)

## What's New in v1.0.0

### 🎉 Major Stability Improvements
- **Fixed crashes** during long recording sessions (30min-2hrs+)
- **Thread-safe file writing** prevents data corruption
- **Proper resource cleanup** with IDisposable pattern implementation
- **Comprehensive error logging** for better troubleshooting
- **Memory optimization** with immediate stream flushing

### 📁 Auto-Save CSV Files
- **CSV files automatically save** when you start recording - no manual action needed!
- Save location: `Documents\Shimmer_GSR\` folder
- Filename format: `SubjectName_yyyyMMdd_HHmmss.csv`
- Automatic directory creation and file cleanup

### 📂 Quick Folder Access
New menu items in Tools menu:
- **Open Data Folder** - Instantly access your saved CSV files
- **Open Log Folder** - View error logs and troubleshooting info
- **CSV Auto-Save Info** - Check save location and filename format

### 🎯 Manual UDP Markers
- Send custom UDP markers with a button press (marker 230)
- **Auto-send markers** when recording starts (231) and stops (232)
- Configurable IP address and port (saved between sessions)
- All markers logged to both CSV file and error log

## Features

- 📡 Connect to Shimmer v3 devices via Bluetooth or Serial
- ⚙️ Configure sensor parameters (sampling rate, enabled sensors, ranges)
- 📊 Real-time data streaming and visualization
- 💾 Automatic CSV logging with timestamps
- 🔔 UDP marker support for event synchronization
- 📈 Real-time graphing with ZedGraph
- 🎮 3D orientation visualization (OpenGL)

## Getting Started

### First Time Setup: Pairing Your Shimmer Device

> 💡 **Quick Help**: In the app, go to **Help → Bluetooth Setup Help** for step-by-step instructions!

> 🔑 **IMPORTANT**: The Shimmer Bluetooth pairing PIN is **1234** (you'll need this during pairing)

**For Bluetooth Connection:**
1. Turn on your Shimmer v3 device (blue LED should blink)
2. Open Windows **Settings** → **Devices** → **Bluetooth & other devices**
3. Click **"Add Bluetooth or other device"** → **Bluetooth**
4. Wait for your Shimmer device to appear (e.g., "Shimmer3-XXXX")
5. Click on your Shimmer device to pair
6. **Enter the pairing PIN: 1234** when prompted
7. Wait for "Connected" status
8. Windows will automatically assign a COM port (e.g., COM5)
9. Return to ShimmerCapture and click the **Reload** button

**For USB/Serial Connection:**
- Simply plug in the USB-to-serial adapter
- Windows will automatically detect the COM port

### Using the Application

1. Launch ShimmerCapture
2. Click the **Reload** button to refresh COM ports
3. Select your Shimmer device from the COM port dropdown
   - If you see **"No Valid Ports"**: Your device isn't paired or connected yet (see setup above)
4. Click **Connect** - wait for connection confirmation
5. Configure your sensors in the Configuration menu if needed
6. Enter a subject name
7. Click **Start** - CSV logging begins automatically!
8. Click **Stop** when done - file automatically closes

Your data is saved to: `Documents\Shimmer_GSR\SubjectName_TIMESTAMP.csv`

## UDP Marker Integration

The application includes UDP marker support for event synchronization:

- **Incoming markers**: Listens on port 5501 (configurable)
- **Outgoing markers**: Configurable IP/port via GUI
- **Automatic markers**: 231 (start) and 232 (stop) sent automatically
- **Manual marker**: Send marker 230 via button press

### Using with udp_to_key.py
The included Python script converts UDP markers to keyboard inputs:
```bash
python udp_to_key.py
```
- Receives markers on port 5501
- Relays to ports 5504 and 5508
- Maps markers to keyboard keys (0-9 → number keys, F2-F10, etc.)

## Building from Source

### Requirements
- Visual Studio 2019 or later
- .NET Framework 4.6.1 SDK
- Windows OS

### Dependencies
- System.IO.Ports (5.0.0) - Serial port communication
- InTheHand.Net.Personal (3.5.605.0) - Bluetooth support
- ZedGraph (5.1.5) - Graphing library
- OpenTK (1.1.0) - 3D visualization

### Build Steps
1. Clone this repository
2. Open `ShimmerCapture.sln` in Visual Studio
3. Restore NuGet packages
4. Build solution (F6)
5. Run ShimmerCapture project

## Troubleshooting

### View Error Logs
- Tools → Open Log Folder
- Logs are stored in your system temp directory
- Includes timestamps, stack traces, and memory usage info

### Common Issues

**"No Valid Ports" appearing:**
- Your Shimmer device is not paired with Windows
- Go to Help → Bluetooth Setup Help in the app for instructions
- Remember: **Pairing PIN is 1234**
- After pairing, click the **Reload** button in ShimmerCapture

**Port access denied:**
- Close other applications using the COM port
- Unpair and re-pair the device if the issue persists

**Device not found:**
- Check Bluetooth pairing and COM port assignment
- Make sure the Shimmer device is powered on (blue LED blinking)
- Try removing the device from Windows and pairing again

**Cannot pair - PIN rejected:**
- The correct PIN is **1234** (not 0000 or other common PINs)
- Make sure you're entering exactly: 1-2-3-4

**CSV not saving:**
- Check `Documents\Shimmer_GSR\` folder permissions
- Make sure you have write access to your Documents folder

## File Structure

```
ShimmerAPI_Tool/
├── ShimmerAPI/              # Core sensor communication library
├── ShimmerCapture_source/   # GUI application
├── udp_to_key.py           # UDP marker relay script
├── stable/                  # Binary dependencies
└── packages/                # NuGet packages
```

## License

BSD 3-Clause License - Copyright © 2022 Shimmer Research, Ltd. / Drexel University

## Acknowledgments

Based on the official Shimmer C# SDK. Modified and enhanced for Drexel University research projects.

## Support

For issues and bug reports, check the error logs in Tools → Open Log Folder and include relevant log excerpts when reporting problems.
