using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows;
using System.Net.Http;
using System.Threading;
using System.Windows.Controls;

namespace CommunicationProtocol
{
    public class TcpClientClass
    {
        private TcpClient client;
        private NetworkStream stream;
        private Encoding encoding;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public class MessageReceivedEventArgs : EventArgs
        {
            public string Message { get; }

            public MessageReceivedEventArgs(string message)
            {
                Message = message;
            }
        }
        
        public bool Start(string ip, int port, Encoding encoding)
        {
            try
            {
                this.client = new TcpClient(ip, port);
                this.stream = client.GetStream();
                this.encoding = encoding;

                void StartListenerTPCClient()
                {
                    while (client.Connected)
                    {
                        _ = ReceiveMessageAsync();

                        Thread.Sleep(1000);
                    }
                }

                Thread thread = new Thread(StartListenerTPCClient);
                thread.Start();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
                return false;
            }
        }

        public async Task ReceiveMessageAsync()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine("Server connection closed.");
                    return;
                }

                string message = encoding.GetString(buffer, 0, bytesRead);

                // Invocar el evento con el mensaje recibido
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data: {ex.Message}");
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                byte[] data = encoding.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public void Stop()
        {
            try
            {
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing socket: {ex.Message}");
            }
        }
    }
}
