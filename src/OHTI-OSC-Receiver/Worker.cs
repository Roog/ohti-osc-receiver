using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            OpenSoundControlListener openSoundControlListener)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _websocketHub = websocketHub;
            _oscListener = openSoundControlListener;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {



            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

	}
}
