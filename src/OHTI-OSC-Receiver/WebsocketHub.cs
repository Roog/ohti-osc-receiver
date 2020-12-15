
using System;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OHTI_OSC_Receiver
{
    public class WebsocketHub : Hub<IWebsocketHub>
    {
        //public delegate void CommandEventHandler(string name);
        //public event CommandEventHandler CommandEvent;

        //private static VirtualGpioClientWorker virtualGpioClientWorker => VirtualGpioClientWorker.SingleInstance;

        public override async Task OnConnectedAsync()
        {
            //await Clients.All.SystemStatus("Connected...");
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return Task.CompletedTask;
        }
    }

    public interface IWebsocketHub
    {
        //Task SystemStatus(string message);
        Task HeadtrackerEvent(string address, float w, float x, float y, float z);
        Task HeadtrackerEulerEvent(string address, float yaw, float pitch, float roll);
    }
}
