using System.Net;

namespace OHTI_OSC_Receiver
{
    public class OpenSoundControlState
    {
        public string Id { get; set; }
        public IPAddress Hostname { get; set; }
        public int Port { get; set; }
        public bool UseUnicast { get; set; } = false;
        public bool Connected { get; set; } = false;

        public void SetConfig(string address, int port, bool useUnicast = false)
        {
            if (IPAddress.TryParse(address, out IPAddress ipAddress) == true)
            {
                Hostname = ipAddress;
            }
            Port = port;
            UseUnicast = useUnicast;
        }

        public override string ToString()
        {
            return $"OSC Control State {Id} {Hostname}:{Port} connected: {(Connected ? "yes" : "no")}, use unicast: {(UseUnicast ? "yes" : "no")}";
        }
    }
}