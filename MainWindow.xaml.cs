using System;
using System.Collections.Generic;
using System.Linq;
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
        public class EncodingInfo
        {
            public string Name { get; set; }
            public int Code { get; set; }

            public EncodingInfo(string nombre, int codigo)
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
            InitializeComponent();

            StartAllTypeEncodings();
            StartAllTypeProtocol();
            StartAllComponent();
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
                string inputIP = InputIPConnection.Text;
                string inputPort = InputPORTConnection.Text;

                string ControllerMessageValidConnections = String.Empty;
                if (IsValidConnections(inputIP, inputPort, out ControllerMessageValidConnections))
                {

                } else
                {
                    MessageBox.Show(ControllerMessageValidConnections, "Communication Protocol Tool");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }

        }

        #region StartTypeEncodings & Protocols
        private void StartAllTypeEncodings()
        {
            try
            {
                Encodings = new List<EncodingInfo>()
                {
                    new EncodingInfo("ASCII", 6),
                    new EncodingInfo("UTF-8", 8),
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
                int day = 4;
                switch (comboBox.SelectedItem.ToString())
                {
                    case "TCP":
                        BorderTextStatusConnection.Visibility = Visibility.Visible;
                        ButtonConnectConnection.Visibility = Visibility.Visible;

                        break;
                    case "UDP":
                        BorderTextStatusConnection.Visibility = Visibility.Hidden;
                        ButtonConnectConnection.Visibility = Visibility.Hidden;

                        break;
                    default:
                        Console.WriteLine("Looking forward to the Weekend.");

                        break;
                }

            }
        }
    }
}
 