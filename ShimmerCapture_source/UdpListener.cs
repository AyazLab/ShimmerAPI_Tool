using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;

using ShimmerAPI;
using ShimmerLibrary;

class UDPListener : IDisposable
{
    private int m_portToListen = 5501;
    private volatile bool listening;
    Thread m_ListeningThread;
    public event EventHandler<MyMessageArgs> NewMessageReceived;

    private UdpClient listener = null;

    private Logging writeToFile = null;
    private MarkerLogger markerLogger = null;
    private bool disposed = false;

    //constructor
    public UDPListener()
    {
        this.listening = false;
        writeToFile = null;
        markerLogger = null;
    }

    public UDPListener(int m_portToListen)
    {
        this.m_portToListen = m_portToListen;
        this.listening = false;
        writeToFile = null;
        markerLogger = null;
    }

    public void SetWriteToFile(Logging _writeToFileRef)
    {
        writeToFile = _writeToFileRef;
    }

    public void SetMarkerLogger(MarkerLogger _markerLogger)
    {
        markerLogger = _markerLogger;
    }

    public void StartListener(int exceptedMessageLength)
    {
        if (!this.listening)
        {
            ErrorLogger.LogInfo($"Starting UDP listener on port {m_portToListen}", "UDPListener.StartListener");
            m_ListeningThread = new Thread(ListenForUDPPackages);
            m_ListeningThread.IsBackground = true;
            this.listening = true;
            m_ListeningThread.Start();
        }
    }

    public void StopListener()
    {
        ErrorLogger.LogInfo($"Stopping UDP listener on port {m_portToListen}", "UDPListener.StopListener");
        this.listening = false;

        try
        {
            if (listener != null)
            {
                listener.Close();   // forcibly end communication
            }

            // Wait for thread to finish (up to 2 seconds)
            if (m_ListeningThread != null && m_ListeningThread.IsAlive)
            {
                if (!m_ListeningThread.Join(2000))
                {
                    ErrorLogger.LogWarning($"UDP listener thread did not terminate in time for port {m_portToListen}", "UDPListener.StopListener");
                }
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError($"Error stopping UDP listener on port {m_portToListen}", ex, "UDPListener.StopListener");
        }
    }

    public void ListenForUDPPackages()
    {
        listener = null;

        try
        {
            listener = new UdpClient(m_portToListen);
            ErrorLogger.LogInfo($"UDP client created for port {m_portToListen}", "UDPListener.ListenForUDPPackages");
        }
        catch (SocketException ex)
        {
            ErrorLogger.LogError($"Failed to create UDP client on port {m_portToListen} - port may be in use", ex, "UDPListener.ListenForUDPPackages");
            return;
        }

        if (listener != null)
        {
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, m_portToListen);

            try
            {
                while (this.listening)
                {
                    Console.WriteLine("Waiting for UDP broadcast to port " + m_portToListen);
                    byte[] bytes = listener.Receive(ref groupEP);

                    // Log inbound marker to marker log (before writing to main data file)
                    if (markerLogger != null)
                    {
                        try
                        {
                            markerLogger.LogInboundMarker(bytes[0]);
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Error logging inbound marker", ex, "UDPListener.ListenForUDPPackages");
                        }
                    }

                    if (writeToFile != null)
                    {
                        try
                        {
                            writeToFile.WriteMarker(bytes[0]);
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Error writing marker to file", ex, "UDPListener.ListenForUDPPackages");
                        }
                    }

                    // Raise event - check if there are subscribers first
                    if (NewMessageReceived != null)
                    {
                        try
                        {
                            NewMessageReceived(this, new MyMessageArgs(bytes));
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Error in UDP message event handler", ex, "UDPListener.ListenForUDPPackages");
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                if (this.listening)
                {
                    // Only log if we're still supposed to be listening (not a deliberate stop)
                    ErrorLogger.LogWarning($"Socket error while listening on port {m_portToListen}: {ex.Message}", "UDPListener.ListenForUDPPackages");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Unexpected error in UDP listener on port {m_portToListen}", ex, "UDPListener.ListenForUDPPackages");
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                try
                {
                    if (listener != null)
                    {
                        listener.Close();
                        listener.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogWarning($"Error disposing UDP client: {ex.Message}", "UDPListener.ListenForUDPPackages");
                }
                ErrorLogger.LogInfo($"UDP listener stopped for port {m_portToListen}", "UDPListener.ListenForUDPPackages");
                Console.WriteLine("Done listening for UDP broadcast");
            }
        }
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
            try
            {
                StopListener();

                if (listener != null)
                {
                    listener.Dispose();
                    listener = null;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("Error disposing UDPListener", ex, "UDPListener.Dispose");
            }
        }

        disposed = true;
    }

    ~UDPListener()
    {
        Dispose(false);
    }
}

public class MyMessageArgs : EventArgs
{
    public byte[] data { get; set; }

    public MyMessageArgs(byte[] newData)
    {
        data = newData;
    }
}