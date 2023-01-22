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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OHTI_OSC_Receiver.Models;
using OpenSoundControlBroadcastClient;

namespace OHTI_OSC_Receiver
{
    public class Worker : BackgroundService
    {
        public static Worker SingleInstance { get; private set; }

        private readonly ILogger<Worker> _logger;
        private readonly ApplicationSettings _configuration;
        private readonly IHubContext<WebsocketHub, IWebsocketHub> _websocketHub;
        private readonly UdpBroadcastReceiver _oscUdpBroadcastReceiver;
        private readonly HeadtrackerFormat _headtrackerData = new HeadtrackerFormat();
        private readonly HeadtrackerClientState _headtrackerClientState = new HeadtrackerClientState();

        public Worker(
            ILogger<Worker> logger,
            IOptions<ApplicationSettings> configuration,
            IHubContext<WebsocketHub, IWebsocketHub> websocketHub,
            UdpBroadcastReceiver oscBroadcastReceiver)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _websocketHub = websocketHub;
            _oscUdpBroadcastReceiver = oscBroadcastReceiver;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SingleInstance = this;

            // Initiate service
            _oscUdpBroadcastReceiver.StartService(new UdpClientOptions
            {
                Hostname = _configuration.Receiver.Hostname,
                Port = _configuration.Receiver.Port
            });

            // Set-up listeners
            _oscUdpBroadcastReceiver.HeadtrackingDataEvent += ReceivedHeadtrackingEventHandler;
            _oscUdpBroadcastReceiver.UdpClientConnectedEvent += ReceivedUdpClientConnectedEventHandler;

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}, current headtracking data: {_headtrackerData}");
                await SendApplicationStateAsync();
                await Task.Delay(2000, stoppingToken);
            }
        }

        public async Task SendApplicationStateAsync()
        {
            _headtrackerClientState.ReceiverIsConnected = _oscUdpBroadcastReceiver.IsClientConnected;

            await _websocketHub.Clients.All.ApplicationState(_headtrackerClientState);
        }

        private void ReceivedUdpClientConnectedEventHandler(bool isConnected)
        {
            // Store some application state
            _headtrackerClientState.ReceiverIsConnected = isConnected;
            _websocketHub.Clients.All.ApplicationState(_headtrackerClientState);
        }

        private void ReceivedHeadtrackingEventHandler(string address, float w, float x, float y, float z)
        {
            // Store a local copy
            _headtrackerData.Save(address, w, x, y, z);

            // Store some application state
            _headtrackerClientState.LastReceivedData = DateTime.Now;

            // Send out to the websocket
            _websocketHub.Clients.All.HeadtrackerEvent(address, w, x, y, z);

            _websocketHub.Clients.All.HeadtrackerEulerEvent(address, _headtrackerData.Euler[0], _headtrackerData.Euler[1], _headtrackerData.Euler[2]);
        }
    }
}
