using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationProtocol
{
    public class TcpServer
    {
        TcpListener listener;
        public void SetupTCPServer()
        {
            IPAddress ipAddress = IPAddress.Any;
            int port = 8080;
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

            listener = new TcpListener(endPoint);
        }

        public void Start()
        {
            listener.Start();
        }
    }
}
