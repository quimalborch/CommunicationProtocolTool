using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows;

namespace CommunicationProtocol
{
    public class TcpClient
    {
        System.Net.Sockets.TcpClient ClientTCP;
        System.Text.Encoding Encoding;

        public bool Start(string Ip, string Port, System.Text.Encoding encoding)
        {
            try
            {
                ClientTCP = new System.Net.Sockets.TcpClient(Ip, int.Parse(Port));
                Encoding = encoding;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                ClientTCP.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return false;
            }
        }

        public void SendMessage(string Message)
        {
            try
            {
                NetworkStream stream = ClientTCP.GetStream();
                byte[] data = Encoding.GetBytes(Message);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
