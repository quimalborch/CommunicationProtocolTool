using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UdpSocket
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public string ipAddress = "1.1.1.1";
    public int port = 0;

    public UdpSocket(string ipAddress, int port)
    {
        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

        this.ipAddress = ipAddress;
        this.port = port;
    }

    public event EventHandler<string> MessageReceived;

    public void SendData(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar datos: {ex.Message}");
        }
    }

    public string ReceiveData()
    {
        try
        {
            IPEndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] receivedData = udpClient.Receive(ref senderEndPoint);
            string message = Encoding.UTF8.GetString(receivedData);

            // Disparar el evento MessageReceived
            MessageReceived?.Invoke(this, message);

            // Devolver el mensaje recibido
            return message;
        }
        catch (SocketException ex)
        {
            if (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                // Manejar específicamente el caso de ConnectionReset
                Console.WriteLine("La conexión fue restablecida por el host remoto.");
            }
            else
            {
                Console.WriteLine($"Error al recibir datos: {ex.Message}");
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al recibir datos: {ex.Message}");
            return null;
        }
    }


    public void Close()
    {
        udpClient.Close();
    }
}
