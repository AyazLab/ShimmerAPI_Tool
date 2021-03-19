using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;

using ShimmerAPI;
using ShimmerLibrary;

class UDPListener
{
    private int m_portToListen = 5501;
    private volatile bool listening;
    Thread m_ListeningThread;
    public event EventHandler<MyMessageArgs> NewMessageReceived;

    private UdpClient listener = null;

    private Logging writeToFile=null;

    //constructor
    public UDPListener()
    {
        this.listening = false;
        writeToFile = null;
    }

    public UDPListener(int m_portToListen)
    {
        this.m_portToListen = m_portToListen;
        this.listening = false;
        writeToFile = null;
    }

    public void SetWriteToFile(Logging _writeToFileRef)
    {
        writeToFile = _writeToFileRef;
    }

    public void StartListener(int exceptedMessageLength)
    {
        if (!this.listening)
        {
            m_ListeningThread = new Thread(ListenForUDPPackages);
            m_ListeningThread.IsBackground = true;
            this.listening = true;
            m_ListeningThread.Start();
        }
    }

    public void StopListener()
    {
        this.listening = false;
        listener.Close();   // forcibly end communication 
    }

    public void ListenForUDPPackages()
{
        listener = null;
    
    try
    {
        listener = new UdpClient(m_portToListen);
    }
    catch (SocketException)
    {
        //do nothing
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

                if(writeToFile!=null)
                    writeToFile.WriteMarker(bytes[0]);
                //raise event                        
                NewMessageReceived(this, new MyMessageArgs(bytes));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            listener.Close();
            Console.WriteLine("Done listening for UDP broadcast");
        }
    }
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