using System;
using System.IO.Ports;
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

        private static SerialPort _comport = new SerialPort();

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

            _oscListener.Connect();

            //// Get a list of serial port names.             
            //string[] ports = SerialPort.GetPortNames();
            //Console.WriteLine("The following serial ports were found:");
            //// Display each port name to the console.             
            //foreach (string port in ports)
            //{
            //    Console.WriteLine(port);
            //}


            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(20000, stoppingToken);
            }
        }

	}
}
