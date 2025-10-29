using System;
using System.IO.Ports;

namespace ShimmerAPI
{

    public class ShimmerLogAndStreamSystemSerialPort : ShimmerLogAndStream, IDisposable
    {
        protected String ComPort;
        public System.IO.Ports.SerialPort SerialPort = new System.IO.Ports.SerialPort();
        private bool disposed = false;

        public ShimmerLogAndStreamSystemSerialPort(String devID, String bComPort)
            : base(devID)
        {
            ComPort = bComPort;
        }

        public ShimmerLogAndStreamSystemSerialPort(String devName, String bComPort, double samplingRate, int setEnabledSensors, byte[] exg1configuration, byte[] exg2configuration)
            : base(devName,samplingRate, setEnabledSensors, exg1configuration, exg2configuration)
        {
            ComPort = bComPort;
        }

        public ShimmerLogAndStreamSystemSerialPort(String devName, String bComPort, double samplingRate, int accelRange, int gsrRange, int setEnabledSensors, bool enableLowPowerAccel, bool enableLowPowerGyro, bool enableLowPowerMag, int gyroRange, int magRange, byte[] exg1configuration, byte[] exg2configuration, bool internalexppower)
            :base(devName, samplingRate, accelRange, gsrRange, setEnabledSensors, enableLowPowerAccel, enableLowPowerGyro, enableLowPowerMag, gyroRange, magRange, exg1configuration, exg2configuration, internalexppower)
        {
            ComPort = bComPort;
        }

        public ShimmerLogAndStreamSystemSerialPort(String devID, String bComPort, double samplingRate, int AccelRange, int GyroRange, int gsrRange, int setEnabledSensors)
            : base(devID, samplingRate, AccelRange, GyroRange, gsrRange, setEnabledSensors)
        {
            ComPort = bComPort;
        }

        protected override bool IsConnectionOpen()
        {
            //SerialPort.PortName = ComPort;
            return SerialPort.IsOpen;
        }

        protected override void CloseConnection()
        {
            SerialPort.Close();
        }
        protected override void FlushConnection()
        {
            SerialPort.DiscardInBuffer();
            SerialPort.DiscardOutBuffer();
        }
        protected override void FlushInputConnection()
        {
            try
            {
                SerialPort.ReadExisting();
                SerialPort.DiscardInBuffer();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogWarning($"Error flushing serial port input for {ComPort}: {ex.Message}", "ShimmerLogAndStreamSystemSerialPort.FlushInputConnection");
                // Continue - non-critical error
            }

        }
        protected override void WriteBytes(byte[] b, int index, int length)
        {
            if (GetState() != SHIMMER_STATE_NONE)
            {
                try
                {
                    SerialPort.Write(b, index, length);
                }
                catch (TimeoutException ex)
                {
                    ErrorLogger.LogWarning($"Write timeout on port {ComPort}: {ex.Message}", "ShimmerLogAndStreamSystemSerialPort.WriteBytes");
                    CustomEventArgs newEventArgs = new CustomEventArgs((int)ShimmerIdentifier.MSG_IDENTIFIER_NOTIFICATION_MESSAGE, "Connection lost - Write timeout");
                    OnNewEvent(newEventArgs);
                    Disconnect();
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError($"Write error on port {ComPort}, disconnecting", ex, "ShimmerLogAndStreamSystemSerialPort.WriteBytes");
                    CustomEventArgs newEventArgs = new CustomEventArgs((int)ShimmerIdentifier.MSG_IDENTIFIER_NOTIFICATION_MESSAGE, "Connection lost");
                    OnNewEvent(newEventArgs);
                    Disconnect();
                }
            }
        }

        protected override int ReadByte()
        {   
            if (GetState() != SHIMMER_STATE_NONE)
            {
                return SerialPort.ReadByte();
            }
            throw new InvalidOperationException();
        }

        protected override void OpenConnection()
        {
            try
            {
                SerialPort.BaudRate = 115200;
                SerialPort.PortName = ComPort;
                SerialPort.ReadTimeout = this.ReadTimeout;
                SerialPort.WriteTimeout = this.WriteTimeout;
                SetState(SHIMMER_STATE_CONNECTING);

                SerialPort.Open();
                ErrorLogger.LogInfo($"Serial port opened: {ComPort}", "ShimmerLogAndStreamSystemSerialPort.OpenConnection");

                SerialPort.DiscardInBuffer();
                SerialPort.DiscardOutBuffer();
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorLogger.LogError($"Port {ComPort} access denied - may be in use by another application", ex, "ShimmerLogAndStreamSystemSerialPort.OpenConnection");
                SetState(SHIMMER_STATE_NONE);
                throw;
            }
            catch (System.IO.IOException ex)
            {
                ErrorLogger.LogError($"I/O error opening port {ComPort} - device may be disconnected", ex, "ShimmerLogAndStreamSystemSerialPort.OpenConnection");
                SetState(SHIMMER_STATE_NONE);
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Failed to open serial port {ComPort}", ex, "ShimmerLogAndStreamSystemSerialPort.OpenConnection");
                SetState(SHIMMER_STATE_NONE);
                throw;
            }
        }

        public override string GetShimmerAddress()
        {
            return ComPort;
        }

        public override void SetShimmerAddress(string address)
        {
            ComPort = address;
        }

        public new void Dispose()
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
                    if (SerialPort != null)
                    {
                        if (SerialPort.IsOpen)
                        {
                            SerialPort.Close();
                        }
                        SerialPort.Dispose();
                        ErrorLogger.LogInfo($"Serial port disposed: {ComPort}", "ShimmerLogAndStreamSystemSerialPort.Dispose");
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("Error disposing serial port", ex, "ShimmerLogAndStreamSystemSerialPort.Dispose");
                }
            }

            disposed = true;
        }

        ~ShimmerLogAndStreamSystemSerialPort()
        {
            Dispose(false);
        }
    }
}
