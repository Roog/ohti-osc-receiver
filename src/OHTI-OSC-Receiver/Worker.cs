using System;
using System.IO.Ports;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSoundControlBroadcastClient;
using Rug.Osc.Core;

namespace OHTI_OSC_Receiver
{
    public class Worker : BackgroundService, IDisposable
	{
        private readonly ILogger<Worker> _logger;
        private readonly ApplicationSettings _configuration;
        private readonly IHubContext<WebsocketHub, IWebsocketHub> _websocketHub;
        private readonly OpenSoundControlListener _oscListener;

        public Worker(ILogger<Worker> logger,
            IOptions<ApplicationSettings> configuration,
            IHubContext<WebsocketHub, IWebsocketHub> websocketHub,
            OpenSoundControlListener openSoundControlListener,
            UDPBroadcastReceiver oscBroadcastReceiver)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _websocketHub = websocketHub;
            _oscListener = openSoundControlListener;
            oscBroadcastReceiver.StartService(new UdpClientOptions());

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _oscListener.SaveConfiguration("239.255.255.255", 9000);
            _oscListener.Connect();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(20000, stoppingToken);
            }
        }
    }
}
