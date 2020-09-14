using System;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;
using Rug.Osc.Core;

namespace OHTI_OSC_Receiver
{
    public class OpenSoundControlState
    {
        public string Id { get; set; }
        public IPAddress Hostname { get; set; }
        public int Port { get; set; }
        public bool Connected { get; set; } = false;

        public void SetConfig(string address, int port)
        {
            if (IPAddress.TryParse(address, out IPAddress ipAddress) == true)
            {
                Hostname = ipAddress;
            }
            Port = port;
        }

        public override string ToString()
        {
            return $"OSC Control State {Id} {Hostname?.AddressFamily}:{Port} connected: {Connected}";
        }
    }

    public class OpenSoundControlListener : IDisposable
    {
		private readonly ILogger<OpenSoundControlListener> _logger;
		private static OscReceiver m_Receiver;
		private static OscAddressManager m_Listener;
		private static Thread m_Thread;
        private static OpenSoundControlState _state = new OpenSoundControlState();

		public delegate void HeadtrackingDataEventHandler(string name);
		public event HeadtrackingDataEventHandler HeadtrackingDataEvent;

        public delegate void HeadtrackingDeviceEventHandler(OpenSoundControlState state);
        public event HeadtrackingDeviceEventHandler HeadtrackingDeviceEvent;

		public OpenSoundControlListener(ILogger<OpenSoundControlListener> logger)
        {
            _logger = logger;
        }

		public void Connect(string addressString = "239.255.255.255", int addressPort = 9000)
		{
            _logger.LogInformation($"Connecting OpenSoundControlListener to '{addressString}:{addressPort}'");
            _state.SetConfig(addressString, addressPort);

			// if there is already an instace dispose of it
			if (m_Receiver != null)
			{
				// dispose of the reciever
				_logger.LogInformation("Disconnecting");
				m_Receiver.Dispose();
				m_Receiver = null;
                _state.Connected = false;
			}

            IPAddress ipAddress;

            // parse the ip address
            //if (addressString.Trim().Equals("Any", StringComparison.InvariantCultureIgnoreCase) == true)
            //{
            //	ipAddress = IPAddress.Any;
            //}
            if (IPAddress.TryParse(addressString, out ipAddress) == false)
            {
                _logger.LogInformation($"Invalid IP address, {addressString}");
                return;
            }

            // create the receiver instance
            //m_Receiver = new OscReceiver(ipAddress, (int)addressPort);
            m_Receiver = new OscReceiver(IPAddress.Any, ipAddress, (int)addressPort);

            // tell the user
            _logger.LogInformation(String.Format("Listening on: {0}:{1}", addressString, (int)addressPort));

			// manager
			m_Listener = new OscAddressManager();

			m_Listener.Attach("/Scenerotator", TestMethodA);

			m_Thread = new Thread(new ThreadStart(ListenLoop));

			try
			{
				// connect to the socket 
				m_Receiver.Connect();

				m_Thread.Start();

                _state.Connected = true;
                HeadtrackingDeviceEvent?.Invoke(_state);
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception while connecting");
				_logger.LogError(ex.Message);

				m_Receiver.Dispose();
				m_Receiver = null;

                _state.Connected = false;
                HeadtrackingDeviceEvent?.Invoke(_state);

                return;
			}
		}

		private void TestMethodA(OscMessage message)
		{
			_logger.LogInformation("Test method A called!: " + message[0].ToString());
            _logger.LogInformation("Test method A called by " + message.Origin.ToString() + ": " + message[0].ToString());

            HeadtrackingDataEvent?.Invoke(message[0].ToString());
        }

		private void ListenLoop()
		{
			try
			{
				while (m_Receiver != null && m_Receiver.State != OscSocketState.Closed)
				{
					// if we are in a state to receive
					if (m_Receiver.State == OscSocketState.Connected)
					{
						// get the next message 
						// this will block until one arrives or the socket is closed
						OscPacket packet = m_Receiver.Receive();

						if (packet.Error == OscPacketError.None)
						{
							// write the message to the output
							_logger.LogInformation(packet.ToString());
						}
						else
						{
							_logger.LogError("Error reading packet, " + packet.Error);
							_logger.LogError(packet.ErrorMessage);
						}

						switch (m_Listener.ShouldInvoke(packet))
						{
							case OscPacketInvokeAction.Invoke:
								_logger.LogInformation("Received packet");
								m_Listener.Invoke(packet);
								break;
							case OscPacketInvokeAction.DontInvoke:
                                _logger.LogInformation("Cannot invoke");
                                _logger.LogInformation(packet.ToString());
								break;
							case OscPacketInvokeAction.HasError:
                                _logger.LogError("Error reading osc packet, " + packet.Error);
                                _logger.LogError(packet.ErrorMessage);
								break;
							case OscPacketInvokeAction.Pospone:
                                _logger.LogInformation("Posponed bundle");
                                _logger.LogInformation(packet.ToString());
								break;
							default:
								break;
						}
					}
				}
                _state.Connected = false;
                HeadtrackingDeviceEvent?.Invoke(_state);
            }
			catch (Exception ex)
			{
				// if the socket was connected when this happens
				// then tell the user
				if (m_Receiver?.State == OscSocketState.Connected)
				{
                    _logger.LogError("Exception in listen loop");
                    _logger.LogError(ex.Message);
				}

                _logger.LogError("Exception");
                _logger.LogError(ex.Message);
            }
		}

		public void Dispose()
		{
			if (m_Receiver != null)
			{
				// dispose of the reciever
				m_Receiver.Dispose();
				m_Receiver = null;
			}

            _state.Connected = false;
            HeadtrackingDeviceEvent?.Invoke(_state);
        }
	}
}
