using System;
using System.IO.Ports;
using System.Text;

namespace USBComPortClient
{
    public class Class1
    {
        private static SerialPort _comport = new SerialPort();
        static bool _continue;
        static SerialPort _serialPort;

        // Get a list of serial port names.             
        //string[] ports = SerialPort.GetPortNames();
        //Console.WriteLine("The following serial ports were found:");
        //// Display each port name to the console.             
        //foreach (string port in ports)
        //{
        //    Console.WriteLine(port);
        //}
        //Console.WriteLine("Available Handshake options:");
        //foreach (string s in Enum.GetNames(typeof(Handshake)))
        //{
        //    Console.WriteLine("   {0}", s);
        //}

        //string name;
        //string message;
        //StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        //Thread readThread = new Thread(Read);

        //// Create a new SerialPort object with default settings.
        //_serialPort = new SerialPort();

        //// Allow the user to set the appropriate properties.
        //_serialPort.PortName = "COM7"; // SetPortName(_serialPort.PortName);
        //_serialPort.BaudRate = 9600; //SetPortBaudRate(_serialPort.BaudRate);
        //_serialPort.Parity = Parity.None; //SetPortParity(_serialPort.Parity);
        //_serialPort.DataBits = 8; //SetPortDataBits(_serialPort.DataBits);
        //_serialPort.StopBits = StopBits.One; //SetPortStopBits(_serialPort.StopBits);
        //_serialPort.Handshake = Handshake.None; // SetPortHandshake(_serialPort.Handshake);

        //// Set the read/write timeouts
        //_serialPort.ReadTimeout = 5000;
        //_serialPort.WriteTimeout = 5000;

        //_serialPort.Open();
        //_continue = true;
        //readThread.Start();

        ////Console.Write("Name: ");
        ////name = Console.ReadLine();

        ////Console.WriteLine("Type QUIT to exit");

        ////while (_continue)
        ////{
        ////    message = Console.ReadLine();

        ////    if (stringComparer.Equals("quit", message))
        ////    {
        ////        _continue = false;
        ////    }
        ////    else
        ////    {
        ////        _serialPort.WriteLine(
        ////            String.Format("<{0}>: {1}", name, message));
        ////    }
        ////}

        //readThread.Join();
        //_serialPort.Close();

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    byte[] data = new byte[10];
                    int bytesRead = _serialPort.Read(data, 0, data.Length);
                    string _message = Encoding.ASCII.GetString(data, 0, bytesRead);
                    Console.WriteLine("###RE");
                    //string _message = _serialPort.ReadLine();
                    Console.WriteLine(_message.ToString() + "--");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Err: {ex.Message}");
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
