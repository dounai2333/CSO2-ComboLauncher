using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace CSO2_ComboLauncher
{
    public class Server
    {
        private string name;
        public string Name { get { return name; } }
        public string IpAddress;
        private string ping;
        public string Ping { get { return ping; } }
        public static Dictionary<string,Server> Servers = new Dictionary<string,Server>();

        public Server(string name, string ipaddr)
        {
            this.name = name;
            IpAddress = ipaddr;
            Servers.Add(name,this);

            try
            {
                var ping = new Ping().Send(ipaddr, 1500);
                this.ping = ping.Status == IPStatus.Success ? ping.RoundtripTime.ToString() + "ms" : "Failed";
            }
            catch
            {
                ping = "Failed";
            }
        }
    }
}