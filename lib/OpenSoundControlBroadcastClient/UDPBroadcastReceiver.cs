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

using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OscCore;

namespace OpenSoundControlBroadcastClient
{
    public class UdpBroadcastReceiver: IDisposable
    {
        private readonly ILogger<UdpBroadcastReceiver> _logger;

        protected CancellationTokenSource stoppingTokenSource;
        protected UdpClientOptions options;
        protected UdpClient udpClient;
        private IPEndPoint remoteIpEndpoint;

        private bool _connected;
        public bool IsClientConnected
        {
            get => _connected;
            private set
            {
                _connected = value;
                UdpClientConnectedEvent?.Invoke(value);
            }
        }

        public delegate void HeadtrackingDataEventHandler(string address, float w, float x, float y, float z);
        public event HeadtrackingDataEventHandler HeadtrackingDataEvent;

        public delegate void UdpClientConnectionEventHandler(bool isConnected);
        public event UdpClientConnectionEventHandler UdpClientConnectedEvent;

        public UdpBroadcastReceiver(ILogger<UdpBroadcastReceiver> logger)
        {
            _logger = logger;
            stoppingTokenSource = new CancellationTokenSource();
            remoteIpEndpoint = new IPEndPoint(0, 0);
        }

        private async Task InitiateUdpClient()
        {
            _logger.LogInformation("UdpClient initiating");

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
                    _logger.LogInformation($"UdpClient on {IPAddress.Any}:{options.Port}");
                    udpClient = new UdpClient();
                    udpClient.ExclusiveAddressUse = false;
                    udpClient.EnableBroadcast = true;
                    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, options.Port));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"UdpClient could not connect {ex.Message}");
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
            _logger.LogInformation("UdpClient starting service");
            options = udpClientOptions;

            while (stoppingTokenSource.Token.IsCancellationRequested == false)
            {
                if (IsClientConnected == false)
                {
                    _logger.LogWarning("UdpClient is disconnected");
                    await InitiateUdpClient();
                    SendHeartbeat();

                    // Setup first async event
                    AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
                    udpClient.BeginReceive(callBack, null);
                }

                SendHeartbeat();

                await Task.Delay(3000, stoppingTokenSource.Token);
            }
        }

        void SendHeartbeat()
        {
            try
            {
                var data = Encoding.UTF8.GetBytes("");
                udpClient.Send(data, data.Length, options.Hostname, options.Port);
                _logger.LogDebug("UdpClient keep alive package sent");
            }
            catch(Exception ex)
            {
                _logger.LogWarning($"UdpClient could not send keep alive package because: {ex.Message}");
            }
        }

        /// <summary>
        /// Receives data on this format <![CDATA[SceneRotator/quaternions   ,ffff   = ?B>?????@<???]]>
        /// </summary>
        /// <param name="result"></param>
        void ReceiveCallback(IAsyncResult result)
        {
            byte[] bytes = null;

            try
            {
                bytes = udpClient.EndReceive(result, ref remoteIpEndpoint);
            }
            catch (ObjectDisposedException e)
            {
                // Ignore if disposed. This happens when closing the listener
            }

            // Process bytes
            if (bytes != null && bytes.Length > 0)
            {
                //_logger.LogInformation(Encoding.UTF8.GetString(bytes));
                try
                {
                    OscMessage actual = OscMessage.Read(bytes, 0, bytes.Length);
                    HeadtrackingDataEvent?.Invoke(actual.Address, (float)actual[0], (float)actual[1], (float)actual[2], (float)actual[3]);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"UdpClient error parsing message {ex.Message}");
                }
            }

            try
            {
                AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
                udpClient.BeginReceive(callBack, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"UdpClient error: {ex.Message}");
            }
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
    }
}
