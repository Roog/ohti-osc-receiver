
using System;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OHTI_OSC_Receiver
{
    public class WebsocketHub : Hub<IWebsocketHub>
    {
        public delegate void CommandEventHandler(string name);
        public event CommandEventHandler CommandEvent;

        //private static VirtualGpioClientWorker virtualGpioClientWorker => VirtualGpioClientWorker.SingleInstance;

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("user connected");
            await Clients.All.SystemStatus("Connected...");
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return Task.CompletedTask;
        }

        public void RequestInitialState()
        {
            //virtualGpioClientWorker.WebsocketInitialState();
        }
    }

    public interface IWebsocketHub
    {
        Task SystemStatus(string message);
        Task InitialEmberTree(Dictionary<string, ClientTreeParameterViewModel> obj);
        Task ChangesInEmberTree(string path, ClientTreeParameterViewModel obj);
        Task InitialEmberTreeMatrix(Dictionary<string, ClientMatrixViewModel> obj);
        Task ChangesInEmberTreeMatrix(string path, ClientMatrixSignalViewModel obj);
    }

    public class ClientTreeParameterViewModel
    {
        public string Type { get; set; }
        public dynamic Value { get; set; }
        public string NumericPath { get; set; }
        public bool IsWritable { get; set; }
    }

    public class ClientMatrixViewModel
    {
        public string Name { get; set; }
        public string NumericPath { get; set; }
        public List<ClientMatrixSignalViewModel> Targets { get; set; } = new List<ClientMatrixSignalViewModel>();
        public List<ClientMatrixSignalViewModel> Sources { get; set; } = new List<ClientMatrixSignalViewModel>();
    }

    public class ClientMatrixSignalViewModel
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string LabelPath { get; set; }
        public int[] ConnectedSources { get; set; }
    }
}