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
        /// Server Hostname or IP address
        /// </summary>
        public string Hostname { get; set; } = "localhost";

        /// <summary>
        /// Server Network port
        /// </summary>
        public int Port { get; set; } = 9000;
    }

    public class UDPBroadcastReceiver: IDisposable
    {
        private readonly ILogger<UDPBroadcastReceiver> _logger;
        protected CancellationTokenSource stoppingTokenSource;

        public bool IsClientConnected { get; set; }
        protected UdpClientOptions options;
        protected UdpClient udpClient;

        private IPEndPoint from; // RemoteIpEndPoint

        public delegate void HeadtrackingDataEventHandler(string address, float w, float x, float y, float z);
        public event HeadtrackingDataEventHandler HeadtrackingDataEvent;

        public UDPBroadcastReceiver(ILogger<UDPBroadcastReceiver> logger)
        {
            stoppingTokenSource = new CancellationTokenSource();
            _logger = logger;
            from = new IPEndPoint(0, 0);
        }

        private async Task InitiateUdpClient()
        {
            Console.WriteLine("Initiating UDP Client");

            IsClientConnected = false;
            // Initiate UDP Client
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }
            
            while (udpClient == null && stoppingTokenSource.Token.IsCancellationRequested == false)
            {
                try
                {
                    Console.WriteLine($"UdpClient on {IPAddress.Any}:{options.Port}");
                    udpClient = new UdpClient();
                    udpClient.ExclusiveAddressUse = false;
                    udpClient.EnableBroadcast = true;
                    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, options.Port));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not connect {ex.Message}");
                    IsClientConnected = false;
                    udpClient?.Close();
                    udpClient = null;
                    await Task.Delay(2000, stoppingTokenSource.Token);
                }
            }

            IsClientConnected = true;
        }

        public async void StartService(UdpClientOptions udpClientOptions)
        {
            Console.WriteLine("Starting service");
            options = udpClientOptions;

            while (stoppingTokenSource.Token.IsCancellationRequested == false)
            {
                if (IsClientConnected == false)
                {
                    Console.WriteLine("Client is disconnected");
                    await InitiateUdpClient();
                    var mess = Encoding.UTF8.GetBytes("");
                    udpClient.Send(mess, mess.Length, "255.255.255.255", 9000);

                    // setup first async event
                    AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
                    udpClient.BeginReceive(callBack, null);
                }

                Console.WriteLine("Keep alive package sent");
                var data = Encoding.UTF8.GetBytes("");
                udpClient.Send(data, data.Length, "255.255.255.255", 9000);

                await Task.Delay(3000, stoppingTokenSource.Token);
            }
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
                    HeadtrackingDataEvent?.Invoke(actual.Address, (float)actual[0], (float)actual[1], (float)actual[2], (float)actual[3]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing message {ex.Message}");
                }
            }

            AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
            udpClient.BeginReceive(callBack, null);
        }

        public void Dispose()
        {
            stoppingTokenSource.Cancel();
            if (udpClient != null)
            {
                udpClient.Dispose();
                udpClient = null;
            }

            IsClientConnected = false;
        }

        //SceneRotator/quaternions   ,ffff   = ?B>?????@<???
    }
}
