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
                _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}, Current headtracking data: {_headtrackerState}");
                await Task.Delay(10000, stoppingToken);
            }
        }

        private void ReceivedHeadtrackingEventHandler(string address, float w, float x, float y, float z)
        {
            // Store a local copy
            _headtrackerState.Save(address, w, x, y, z);

            // Send out to the websocket
            _websocketHub.Clients.All.HeadtrackerEvent(address, w, x, y, z);
        }
    }

    public class HeadtrackerFormat
    {
        public string Address { get; set; }
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public void Save(string address, float w, float x, float y, float z)
        {
            Address = address;
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override string ToString()
        {
            return $"Data: {Address}, W: {W}, X: {X}, Y: {Y}, Z: {Z}";
        }
    }
}
