#region copyright
/*
 * Open Headtracker Initiative OSC Websocket Gateway
 *
 * Copyright (c) 2021 Bo-Erik Sandholm & Roger Sandholm, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion copyright

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
