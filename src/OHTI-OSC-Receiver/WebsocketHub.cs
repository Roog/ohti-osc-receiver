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
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OHTI_OSC_Receiver
{
    public class WebsocketHub : Hub<IWebsocketHub>
    {
        //private static Worker worker => Worker.SingleInstance;
        private readonly ApplicationSettings _configuration;

        public WebsocketHub(IOptions<ApplicationSettings> configuration)
        {
            _configuration = configuration.Value;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.ApplicationSettings(_configuration);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return Task.CompletedTask;
        }
    }

    public interface IWebsocketHub
    {
        Task ApplicationSettings(ApplicationSettings settings);
        Task HeadtrackerEvent(string address, float w, float x, float y, float z);
        Task HeadtrackerEulerEvent(string address, float yaw, float pitch, float roll);
    }
}
