using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CommunicationProtocol
{
    internal class UdpClientClass
    {
        UdpClient udpClient;
        Thread thread;
        MainWindow mainWindow;
        public void SendMessage(string IP, int Port, Encoding encoding, string TextMessage, MainWindow _mainWindow)
        {
			try
			{
                mainWindow = _mainWindow;
                lock (this) // Use a lock for thread-safe access to udpClient
                {
                    if (udpClient != null)
                    {
                        Console.WriteLine("Socket already open. Closing existing socket...");
                        
                        if (thread != null && thread.IsAlive)
                        {
                            thread.Abort();
                        }

                        udpClient.Close();
                        udpClient = null; // Reset for reuse
                    }
                }

                thread = new Thread(() => Send(IP, Port, encoding, TextMessage));
                thread.Start();
            }
			catch (Exception)
			{

				throw;
			}
        }

        public void Send(string IP, int Port, Encoding encoding, string TextMessage)
        {
            try
            {
                // Ensure thread-safe access to udpClient
                lock (this)
                {
                    udpClient = new UdpClient(Port);
                    udpClient.Connect(IP, Port);
                }

                //udpClient = new UdpClient(Port);
                //udpClient.Connect(IP, Port);

                Byte[] sendBytes = encoding.GetBytes(TextMessage);
                udpClient.Send(sendBytes, sendBytes.Length);

                //IPEndPoint object will allow us to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = encoding.GetString(receiveBytes);

                // Uses the IPEndPoint object to determine which of these two hosts responded.
                //MessageBox.Show("This is the message you received " + returnData.ToString());
                //MessageBox.Show("This message was sent from " + RemoteIpEndPoint.Address.ToString() + " on their port number " + RemoteIpEndPoint.Port.ToString());

                //invoke to change text TextBoxRecivedInformation
                mainWindow.Dispatcher.Invoke(() => mainWindow.TextBoxRecivedInformation.Text = mainWindow.TextBoxRecivedInformation.Text + "[" + RemoteIpEndPoint.Address.ToString() + ":" + RemoteIpEndPoint.Port.ToString() + "]: " + returnData.ToString() + "\n");

                udpClient.Close();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
