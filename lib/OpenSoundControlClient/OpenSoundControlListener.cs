using System;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;
using Rug.Osc.Core;

namespace OHTI_OSC_Receiver
{
    public class OpenSoundControlListener : IDisposable
    {
		private readonly ILogger<OpenSoundControlListener> _logger;
        private static OpenSoundControlState _state = new OpenSoundControlState();
        private static OscReceiver m_Receiver;
		private static OscAddressManager m_Listener;
		private static Thread m_Thread;

        #region Properties
        public OpenSoundControlState State { get { return _state; } } 

		//public delegate void HeadtrackingDataEventHandler(string name);
		//public event HeadtrackingDataEventHandler HeadtrackingDataEvent;

        public delegate void HeadtrackingDeviceEventHandler(OpenSoundControlState state, string message = "");
        public event HeadtrackingDeviceEventHandler HeadtrackingDeviceEvent;
        #endregion

        public OpenSoundControlListener(ILogger<OpenSoundControlListener> logger)
        {
            _logger = logger;
        }

        public bool SaveConfiguration(string addressString = "239.255.255.255", int addressPort = 9000, bool useUnicast = false)
        {
            if (m_Receiver?.State == OscSocketState.Connected)
            {
                return false;
            }
            _state.SetConfig(addressString, addressPort, useUnicast);
            return true;
        }

        public void Disconnect()
        {
            if (m_Receiver != null)
            {
                // dispose of the reciever
                m_Receiver.Dispose();
                m_Receiver = null;
                _state.Connected = false;
                HeadtrackingDeviceEvent?.Invoke(_state, $"Disconnecting");
            }
            else
            {
                HeadtrackingDeviceEvent?.Invoke(_state);
            }
        }

        public void Connect()
		{
            _logger.LogInformation($"Connecting OpenSoundControlListener to '{_state}");

            // if there is already an instace dispose of it
            Disconnect();

            // check the ip address
            if (_state.Hostname == null)
            {
                _logger.LogInformation($"No hostname added");
                HeadtrackingDeviceEvent?.Invoke(_state, $"No hostname added");
                return;
            }

            // check the port
            if (_state.Port == 0)
            {
                _logger.LogInformation($"No port added");
                HeadtrackingDeviceEvent?.Invoke(_state, $"No port added");
                return;
            }

            // create the receiver instance
            if (_state.UseUnicast) {
                m_Receiver = new OscReceiver((int)_state.Port);
            } else {
                m_Receiver = new OscReceiver(IPAddress.Any, _state.Hostname, (int)_state.Port);
            }
            
            // tell the user
            _logger.LogInformation($"Listening on: {_state}");
            HeadtrackingDeviceEvent?.Invoke(_state, $"Listening");

            // manager
            m_Listener = new OscAddressManager();
			m_Listener.Attach("/scenerotator", TestMethodA);
            m_Listener.Attach("/scenerotator/quaternions", TestMethodA);

            m_Thread = new Thread(new ThreadStart(ListenLoop));

			try
			{
				// connect to the socket 
				m_Receiver.Connect();

				m_Thread.Start();

                _state.Connected = true;
                HeadtrackingDeviceEvent?.Invoke(_state, "Connected");
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception while connecting");
				_logger.LogError(ex.Message);

                _state.Connected = false;
                HeadtrackingDeviceEvent?.Invoke(_state, $"Error while connecting, {ex.Message}");

                m_Receiver.Dispose();
				m_Receiver = null;

                return;
			}
		}

		private void TestMethodA(OscMessage message)
		{
			_logger.LogInformation("Test method A called!: " + message[0].ToString());
            _logger.LogInformation("Test method A called by " + message.Origin.ToString() + ": " + message[0].ToString());

            //HeadtrackingDataEvent?.Invoke(message[0].ToString());
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
                            //HeadtrackingDataEvent?.Invoke(packet.ToString());
                        }
						else
						{
							_logger.LogError("Error reading packet, " + packet.Error);
							_logger.LogError(packet.ErrorMessage);
                            HeadtrackingDeviceEvent?.Invoke(_state, $"Error reading packet {packet.Error}");
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
                    HeadtrackingDeviceEvent?.Invoke(_state, $"Exception in listen loop, {ex.Message}");
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
            HeadtrackingDeviceEvent?.Invoke(_state, "Disposed");
        }
	}
}
