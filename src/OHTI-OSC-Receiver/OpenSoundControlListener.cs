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
		private static OscReceiver m_Receiver;
		private static OscAddressManager m_Listener;
		private static Thread m_Thread;

		public delegate void HeadtrackingDataEventHandler(string name);
		public event HeadtrackingDataEventHandler HeadtrackingDataEvent;

		public OpenSoundControlListener(ILogger<OpenSoundControlListener> logger)
        {
            _logger = logger;
        }

		public void Connect()
		{
			// if there is already an instace dispose of it
			if (m_Receiver != null)
			{
				// disable the timer
				//m_MessageCheckTimer.Enabled = false;

				// dispose of the reciever
				_logger.LogInformation("Disconnecting");
				m_Receiver.Dispose();
				m_Receiver = null;
			}

			// get the ip address from the address box 
			string addressString = "localhost";
			int addressPort = 77;

			IPAddress ipAddress;

			// parse the ip address
			if (addressString.Trim().Equals("Any", StringComparison.InvariantCultureIgnoreCase) == true)
			{
				ipAddress = IPAddress.Any;
			}
			else if (IPAddress.TryParse(addressString, out ipAddress) == false)
			{
				_logger.LogInformation(String.Format("Invalid IP address, {0}", addressString));

				return;
			}

			// create the reciever instance
			m_Receiver = new OscReceiver(ipAddress, (int)addressPort);

			// tell the user
			_logger.LogInformation(String.Format("Listening on: {0}:{1}", ipAddress, (int)addressPort));

			// manager
			m_Listener = new OscAddressManager();

			m_Listener.Attach("/testA", TestMethodA);

			m_Thread = new Thread(new ThreadStart(ListenLoop));

			try
			{
				// connect to the socket 
				m_Receiver.Connect();

				m_Thread.Start();
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception while connecting");
				_logger.LogError(ex.Message);

				m_Receiver.Dispose();
				m_Receiver = null;

				return;
			}

			// enable the timer
			//m_MessageCheckTimer.Enabled = true;
		}


		private void TestMethodA(OscMessage message)
		{
			Console.WriteLine("Test method A called!: " + message[0].ToString());
		}

		private void ListenLoop()
		{
			try
			{
				while (m_Receiver != null && m_Receiver.State != OscSocketState.Closed)
				{
					// if we are in a state to recieve
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
								Console.WriteLine("Cannot invoke");
								Console.WriteLine(packet.ToString());
								break;
							case OscPacketInvokeAction.HasError:
								Console.WriteLine("Error reading osc packet, " + packet.Error);
								Console.WriteLine(packet.ErrorMessage);
								break;
							case OscPacketInvokeAction.Pospone:
								Console.WriteLine("Posponed bundle");
								Console.WriteLine(packet.ToString());
								break;
							default:
								break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				// if the socket was connected when this happens
				// then tell the user
				if (m_Receiver.State == OscSocketState.Connected)
				{
					Console.WriteLine("Exception in listen loop");
					Console.WriteLine(ex.Message);
				}
			}
		}

		void Dispose()
		{
			if (m_Receiver != null)
			{
				// dispose of the reciever
				m_Receiver.Dispose();
				m_Receiver = null;
			}
		}
	}
}
