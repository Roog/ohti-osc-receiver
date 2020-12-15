using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSoundControlBroadcastClient;

namespace OHTI_OSC_Receiver
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ApplicationSettings _configuration;
        private readonly IHubContext<WebsocketHub, IWebsocketHub> _websocketHub;
        private readonly UDPBroadcastReceiver _oscUdpBroadcastReceiver;

        private HeadtrackerFormat _headtrackerState = new HeadtrackerFormat();

        public Worker(ILogger<Worker> logger,
            IOptions<ApplicationSettings> configuration,
            IHubContext<WebsocketHub, IWebsocketHub> websocketHub,
            UDPBroadcastReceiver oscBroadcastReceiver)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _websocketHub = websocketHub;
            _oscUdpBroadcastReceiver = oscBroadcastReceiver;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initiate service
            _oscUdpBroadcastReceiver.StartService(new UdpClientOptions());

            // Set-up listeners
            _oscUdpBroadcastReceiver.HeadtrackingDataEvent += ReceivedHeadtrackingEventHandler;

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}, v0.2, Current headtracking data: {_headtrackerState}");
                float test = (float)-0.7896778;
                float test2 = (float)0.6676576;
                ReceivedHeadtrackingEventHandler("yeah", _headtrackerState.W, test, test2,
                    _headtrackerState.Z);
                await Task.Delay(10000, stoppingToken);
            }
        }

        private void ReceivedHeadtrackingEventHandler(string address, float w, float x, float y, float z)
        {
            // Store a local copy
            _headtrackerState.Save(address, w, x, y, z);

            // Send out to the websocket
            _websocketHub.Clients.All.HeadtrackerEvent(address, w, x, y, z);

            //_websocketHub.Clients.All.HeadtrackerEulerEvent(address, _headtrackerState.Yaw, _headtrackerState.Pitch, _headtrackerState.Roll);
        }
    }

    public class HeadtrackerFormat
    {
        public string Address { get; set; }
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }

        public void Save(string address, float w, float x, float y, float z)
        {
            Address = address;
            W = w;
            X = x;
            Y = y;
            Z = z;

            double sqw = W * W;
            double sqx = X * X;
            double sqy = Y * Y;
            double sqz = Z * Z;

            Yaw = (float)Math.Atan2(2f * X * W + 2f * Y * Z, 1 - 2f * (sqz + sqw));     // Yaw = X
            Pitch = (float)Math.Asin(2f * (X * Z - W * Y));                             // Pitch = Y
            Roll = (float)Math.Atan2(2f * X * Y + 2f * Z * W, 1 - 2f * (sqy + sqz));      // Roll = Z
        }

        public float[] GetEuler()
        {
            float[] euler = new float[3];
            double sqw = W * W;
            double sqx = X * X;
            double sqy = Y * Y;
            double sqz = Z * Z;

            Yaw = (float)Math.Atan2(2f * X * W + 2f * Y * Z, 1 - 2f * (sqz + sqw));     // Yaw = Y
            Pitch = (float)Math.Asin(2f * (X * Z - W * Y));                             // Pitch = X
            Roll = (float)Math.Atan2(2f * X * Y + 2f * Z * W, 1 - 2f * (sqy + sqz));      // Roll = Z

            euler[0] = Yaw;
            euler[1] = Pitch;
            euler[2] = Roll;
            return euler;
        }

        public override string ToString()
        {
            return $"Data: {Address}, W: {W}, X: {X}, Y: {Y}, Z: {Z}";
        }
    }
}
