﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommunicationProtocol
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TcpClientClass tcpServer;
        public bool tcpServerActive;
        public MainWindow ActualInstance;

        public class EncodingInfo
        {
            public string Name { get; set; }
            public System.Text.Encoding Code { get; set; }

            public EncodingInfo(string nombre, System.Text.Encoding codigo)
            {
                Name = nombre;
                Code = codigo;
            }
        }

        public class ProtocolInfo
        {
            public string Name { get; set; }
            public int Code { get; set; }

            public ProtocolInfo(string nombre, int codigo)
            {
                Name = nombre;
                Code = codigo;
            }
        }

        public static List<EncodingInfo> Encodings { get; set; }
        public static List<ProtocolInfo> Protocols { get; set; }

        public MainWindow()
        {
            ActualInstance = this;
            InitializeComponent();

            StartAllTypeEncodings();
            StartAllTypeProtocol();
            StartAllComponent();

            LabelVersionCommunicationProtocol.Content = string.Format("Versión: {0}", Assembly.GetExecutingAssembly().GetName().Version);
        }

        

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void ButtonConnectConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!tcpServerActive)
                {
                    string inputIP = InputIPConnection.Text;
                    string inputPort = InputPORTConnection.Text;

                    string ControllerMessageValidConnections = String.Empty;
                    if (IsValidConnections(inputIP, inputPort, out ControllerMessageValidConnections))
                    {
                        tcpServer = new TcpClientClass();

                        ListComboProtocols.IsEnabled = false;

                        if (tcpServer.Start(inputIP, Int32.Parse(inputPort), GetActualEncoding()))
                        {
                            tcpServerActive = true;
                            ButtonConnectConnection.Content = "Disconnect";
                            ButtonSendDataToSocket.IsEnabled = true;
                            TextBoxContentCommands.IsEnabled = true;

                            tcpServer.MessageReceived += TcpClient_MessageReceived;

                        } else
                        {
                            ListComboProtocols.IsEnabled = true;
                        } 
                    }
                    else
                    {
                        MessageBox.Show(ControllerMessageValidConnections, "Communication Protocol Tool");
                    }
                } else
                {
                    tcpServer.Stop();
                    ButtonConnectConnection.Content = "Connect";

                    ListComboProtocols.IsEnabled = true;
                    TextBoxContentCommands.IsEnabled = false;
                    ButtonSendDataToSocket.IsEnabled = false;
                    tcpServerActive = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }

        }

        private void TcpClient_MessageReceived(object sender, TcpClientClass.MessageReceivedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() => ActualInstance.TextBoxRecivedInformation.Text = ActualInstance.TextBoxRecivedInformation.Text + e.Message);
                Dispatcher.Invoke(() => ActualInstance.TextBoxRecivedInformation.ScrollToEnd());
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately
            }
        }


        private Encoding GetActualEncoding()
        {
            try
            {
                return Encoding.ASCII;
            }
            catch (Exception)
            {
                return Encoding.ASCII;
                throw;
            }
        }

        #region StartTypeEncodings & Protocols
        private void StartAllTypeEncodings()
        {
            try
            {
                Encodings = new List<EncodingInfo>()
                {
                    new EncodingInfo("ASCII", System.Text.Encoding.ASCII),
                    new EncodingInfo("UTF-8", System.Text.Encoding.UTF8),
                    new EncodingInfo("UNICODE", System.Text.Encoding.Unicode),
                };


                List<string> ListComboBox = new List<string>();
                for (global::System.Int32 i = 0; i < Encodings.Count; i++)
                {
                    ListComboBox.Add(Encodings[i].Name);
                }

                ListComboEncodings.ItemsSource = ListComboBox;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void StartAllTypeProtocol()
        {
            try
            {
                Protocols = new List<ProtocolInfo>()
                {
                    new ProtocolInfo("TCP", 0),
                    new ProtocolInfo("UDP", 1),
                };

                List<string> ListComboBox = new List<string>();
                for (global::System.Int32 i = 0; i < Protocols.Count; i++)
                {
                    ListComboBox.Add(Protocols[i].Name);
                }

                ListComboProtocols.ItemsSource = ListComboBox;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region CheckValidConnections
        private bool IsValidConnections(string ipAddress, string inputPort, out string ControllerMessageValidConnections)
        {
            ControllerMessageValidConnections = String.Empty;
            List<string> MessageErrors = new List<string>();
            try
            {
                if ((IsValidIP(ipAddress) && IsValidPort(inputPort)))
                {
                    return true;
                }

                if (!IsValidIP(ipAddress))
                {
                    MessageErrors.Add("La IP seleccionada no está en un formato valido.");
                }

                if (!IsValidPort(inputPort))
                {
                    if (inputPort != String.Empty)
                    {
                        MessageErrors.Add("El puerto seleccionado no está en un formato valido.");
                    } else
                    {
                        MessageErrors.Add("El puerto no puede estar vacio.");
                    }
                }

                string StringMessageBox = "No se ha podido iniciar la conexión devido a los siguientes motivos: \n\n";

                for (Int32 i = 0; i < MessageErrors.Count; i++)
                {
                    StringMessageBox += ((i + 1) + ". " + MessageErrors[i] + "\n");
                }

                ControllerMessageValidConnections = StringMessageBox;
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private bool IsValidPort(string portString)
        {
            if (!int.TryParse(portString, out int port))
            {
                return false;
            }

            return port >= 10 && port <= 99999;
        }

        private bool IsValidIP(string ipAddress)
        {
            string pattern = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
            return Regex.IsMatch(ipAddress, pattern);
        }
        #endregion

        #region StartAllComponent
        private void StartAllComponent()
        {
            BorderTextStatusConnection.Visibility = Visibility.Hidden;
            ButtonConnectConnection.Visibility = Visibility.Hidden;
        }
        #endregion


        private void ListComboProtocolsChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                switch (comboBox.SelectedItem.ToString())
                {
                    case "TCP":
                        BorderTextStatusConnection.Visibility = Visibility.Visible;
                        ButtonConnectConnection.Visibility = Visibility.Visible;
                        ButtonSendDataToSocket.IsEnabled = false;

                        break;
                    case "UDP":
                        BorderTextStatusConnection.Visibility = Visibility.Hidden;
                        ButtonConnectConnection.Visibility = Visibility.Hidden;
                        ButtonSendDataToSocket.IsEnabled = true;

                        break;
                    default:
                        Console.WriteLine("Looking forward to the Weekend.");

                        break;
                }

            }
        }

        private void ButtonCloseApplication_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ButtonSendDataToSocket_Click(object sender, RoutedEventArgs e)
        {
            tcpServer.SendMessage(TextBoxContentCommands.Text);
        }

        private void TextBoxRecivedInformation_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBoxRecivedInformation_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBoxRecivedInformation.Text = String.Empty;
        }

        private void ButtonClearTerminal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TextBoxRecivedInformation.Text = String.Empty;
            }
        }
    }
}
 