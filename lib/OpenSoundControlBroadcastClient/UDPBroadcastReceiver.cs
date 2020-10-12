using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OscCore;

namespace OpenSoundControlBroadcastClient
{
    public class UdpClientOptions
    {
        /// <summary>
        /// ABB PLC Hostname or IP address
        /// </summary>
        public string Hostname { get; set; } = "localhost";

        /// <summary>
        /// ABB PLC Network port
        /// </summary>
        public int Port { get; set; } = 9000;
    }

    public class UDPBroadcastReceiver//: //INotifyPropertyChanged
    {
        private readonly ILogger<UDPBroadcastReceiver> _logger;
        protected CancellationToken stoppingToken;

        public bool IsClientConnected { get; set; }
        protected UdpClientOptions options;
        protected UdpClient udpClient;

        private IPEndPoint from; // RemoteIpEndPoint

        public delegate void HeadtrackingDataEventHandler(string address, float x, float y, float z, float theta);
        public event HeadtrackingDataEventHandler HeadtrackingDataEvent;

        public UDPBroadcastReceiver(ILogger<UDPBroadcastReceiver> logger)
        {
            this.stoppingToken = new CancellationToken();
            _logger = logger;
            Console.WriteLine("#################################### UDPBroadcastReceiver");
            from = new IPEndPoint(0, 0);
        }

        private async Task InitiateUdpClient()
        {
            IsClientConnected = false;
            // Initiate UDP Client
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }
            Console.WriteLine("#################################### InitiateUdpClient");
            while (udpClient == null)
            {
                try
                {
                    Console.WriteLine($"UdpClient on {IPAddress.Any}:{options.Port}");
                    udpClient = new UdpClient();
                    udpClient.ExclusiveAddressUse = false;
                    udpClient.EnableBroadcast = true;
                    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, options.Port));


                    //// https://darchuk.net/2019/01/04/c-setting-socket-keep-alive/


                    //// Get the size of the uint to use to back the byte array
                    //int size = Marshal.SizeOf((uint)0);

                    //// Create the byte array
                    //byte[] keepAlive = new byte[size * 3];

                    //// Pack the byte array:
                    //// Turn keepalive on
                    //Buffer.BlockCopy(BitConverter.GetBytes((uint)1), 0, keepAlive, 0, size);
                    //// Set amount of time without activity before sending a keepalive to 5 seconds
                    //Buffer.BlockCopy(BitConverter.GetBytes((uint)5000), 0, keepAlive, size, size);
                    //// Set keepalive interval to 5 seconds
                    //Buffer.BlockCopy(BitConverter.GetBytes((uint)5000), 0, keepAlive, size * 2, size);

                    //// Set the keep-alive settings on the underlying Socket
                    //udpClient.Client.IOControl(IOControlCode.KeepAliveValues, keepAlive, null);


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not connect {ex.Message}");
                    IsClientConnected = false;
                    udpClient?.Close();
                    udpClient = null;
                    await Task.Delay(2000, stoppingToken);
                }
            }

            IsClientConnected = true;
        }

        public async void StartService(UdpClientOptions udpClientOptions)
        {
            options = udpClientOptions;

            this.stoppingToken = new CancellationToken();
            
            while (this.stoppingToken.IsCancellationRequested == false)
            {
                if (IsClientConnected == false)
                {
                    Console.WriteLine("");
                    await InitiateUdpClient();
                    //var data = Encoding.UTF8.GetBytes("ABCD");
                    //udpClient.Send(data, data.Length, "255.255.255.255", 9000);

                    udpClient.Send(new byte[0], 0, "255.255.255.255", 9000);

                    // setup first async event
                    AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
                    udpClient.BeginReceive(callBack, null);
                }

                Console.WriteLine("ok");
                //var recvBuffer = udpClient.Receive(ref from);
                //Console.WriteLine(Encoding.UTF8.GetString(recvBuffer));
                udpClient.Send(new byte[0], 0, "255.255.255.255", 9000);

                await Task.Delay(3000, this.stoppingToken);
            }
            
            Console.WriteLine("####################################started service");
        }

        void ReceiveCallback(IAsyncResult result)
        {
            Byte[] bytes = null;

            try
            {
                bytes = udpClient.EndReceive(result, ref from);
            }
            catch (ObjectDisposedException e)
            {
                // Ignore if disposed. This happens when closing the listener
            }

            // Process bytes
            if (bytes != null && bytes.Length > 0)
            {
                //Console.WriteLine(Encoding.UTF8.GetString(bytes));

                try
                {
                    OscMessage actual = OscMessage.Read(bytes, 0, bytes.Length);

                    Console.WriteLine($"WOOP {actual}-- {actual.Address} .. {actual.Count} 00 {actual[1]}");
                    HeadtrackingDataEvent?.Invoke(actual.Address, Convert.ToSingle(actual[0]), Convert.ToSingle(actual[1]), Convert.ToSingle(actual[2]), Convert.ToSingle(actual[3]));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing message {ex.Message}");
                }
            }

            AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
            udpClient.BeginReceive(callBack, null);
        }

        //SceneRotator/quaternions   ,ffff   = ?B>?????@<???
    }
}
