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
                //float test = (float)-0.7896778;
                //float test2 = (float)0.6676576;
                //ReceivedHeadtrackingEventHandler("yeah", _headtrackerState.W, test, test2,
                //    _headtrackerState.Z);
                await Task.Delay(10000, stoppingToken);
            }
        }

        private void ReceivedHeadtrackingEventHandler(string address, float w, float x, float y, float z)
        {
            // Store a local copy
            _headtrackerState.Save(address, w, x, y, z);

            // Send out to the websocket
            _websocketHub.Clients.All.HeadtrackerEvent(address, w, x, y, z);

            _websocketHub.Clients.All.HeadtrackerEulerEvent(address, _headtrackerState.Euler[0], _headtrackerState.Euler[1], _headtrackerState.Euler[2]);
        }
    }

    public class HeadtrackerFormat
    {
        public string Address { get; set; }
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public double Roll { get; set; }

        public float[] Euler { get; set; } = new float[3];

        public void Save(string address, float w, float x, float y, float z)
        {
            Address = address;
            W = w;
            X = x;
            Y = y;
            Z = z;

            ToEuler();
        }

        public void ToEuler()
        {
            // roll (x-axis rotation)
            var sinr_cosp = 2 * (W * X + Y * Z);
            var cosr_cosp = 1 - 2 * (X * X + Y * Y);
            Roll = (Math.Atan2(sinr_cosp, cosr_cosp));

            // pitch (y-axis rotation)
            var sinp = 2 * (W * Y - Z * X);
            if (Math.Abs(sinp) >= 1) {
                Pitch = ((Math.PI / 2) * Math.Sign(sinp)); // use 90 degrees if out of range, copysign = sinp + or - applied to Math.PI/2
            } else {
                Pitch = (Math.Asin(sinp));
            }

            // yaw (z-axis rotation)
            var siny_cosp = 2 * (W * Z + X * Y);
            var cosy_cosp = 1 - 2 * (Y * Y + Z * Z);
            Yaw = (Math.Atan2(siny_cosp, cosy_cosp));

            Euler[0] = (float)Math.Round(RadiansToDegree(Yaw) * 100) / 100;
            Euler[1] = (float)Math.Round(RadiansToDegree(Pitch) * 100) / 100;
            Euler[2] = (float)Math.Round(RadiansToDegree(Roll) * 100) / 100;
        }

        public double RadiansToDegree(double radian)
        {
            return (radian * (180 / Math.PI));
        }

        public override string ToString()
        {
            return $"Data: {Address}, W: {W}, X: {X}, Y: {Y}, Z: {Z}";
        }
    }
}
