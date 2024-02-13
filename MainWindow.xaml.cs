﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
using System.Deployment.Application;
using System.Threading;


namespace CommunicationProtocol
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TcpClientClass tcpServer;
        public bool tcpClientActive;
        public MainWindow ActualInstance;
        public RootSessions ListSessions;
        UdpClientClass udpClient = new UdpClientClass();
        Thread ThreadConnectionClientTCP;
        Thread LoopConnections;

        #region Class Session
        public class Answer
        {
            public string AnswerTextBox { get; set; }
        }

        public class Commands
        {
            public string SelectedCommand { get; set; }
            public string ActualText { get; set; }
        }

        public class Connection
        {
            public string IP { get; set; }
            public string PORT { get; set; }
            public string ENCODING { get; set; }
            public string Protocol { get; set; }
        }

        public class RootSessions
        {
            public List<Session> Sessions { get; set; }
        }

        public class Session
        {
            public string NameSession { get; set; }
            public Connection Connection { get; set; }
            public Commands Commands { get; set; }
            public Answer Answer { get; set; }
        }
        #endregion

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
            StartLocalSessions();
            LoadListCommandsXML();

            LabelVersionCommunicationProtocol.Content = string.Format("Version: {0}", Assembly.GetExecutingAssembly().GetName().Version);
        }

        private void LoadListCommandsXML()
        {
            try
            {
                //de la carpeta raiz del proyecto en la carpeta commands, haz una lista de los nombres de archivos.
                string[] files = Directory.GetFiles(@"commands", "*.xml");

                List<string> ListCommands = new List<string>();
                foreach (string file in files)
                {
                       ListCommands.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                }

                ListBoxCommands.ItemsSource = ListCommands;
            }
            catch (Exception)
            {

                throw;
            }
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
                if (!tcpClientActive)
                {
                    string inputIP = InputIPConnection.Text;
                    string inputPort = InputPORTConnection.Text;

                    string ControllerMessageValidConnections = String.Empty;
                    if (IsValidConnections(inputIP, inputPort, out ControllerMessageValidConnections))
                    {
                        if (ThreadConnectionClientTCP != null && ThreadConnectionClientTCP.IsAlive)
                        {
                            ThreadConnectionClientTCP.Abort();
                        }

                        //make a thread
                        ThreadConnectionClientTCP = new Thread(() => StartConnectionTCPClientThread(inputIP, inputPort));
                        ThreadConnectionClientTCP.Start();
                    }
                    else
                    {
                        MessageBox.Show(ControllerMessageValidConnections, "Communication Protocol Tool");
                    }
                }
                else
                {
                    tcpServer.Stop();
                    ButtonConnectConnection.Content = "Connect";

                    ListComboProtocols.IsEnabled = true;
                    ButtonSendDataToSocket.IsEnabled = false;
                    ButtonLoopContinuousConnections.IsEnabled = false;

                    InputIPConnection.IsEnabled = true;
                    InputPORTConnection.IsEnabled = true;
                    //ListComboEncodings.IsEnabled = true;
                    ButtonLoadSession.IsEnabled = true;
                    TextStatusConnection.Content = "Disconnected";

                    BorderTextStatusConnection.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    tcpClientActive = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }

        }

        private void AttemptToConnectTCPAnimation()
        {
            //make a loop for 1 second
            while (true)
            {
                TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = "Connecting.");
                Thread.Sleep(250);
                TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = "Connecting..");
                Thread.Sleep(250);
                TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = "Connecting...");
                Thread.Sleep(250);
                TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = "Connecting....");
                Thread.Sleep(250);
            }
        }

        private void StartConnectionTCPClientThread(string inputIP, string inputPort)
        {
            try
            {
                tcpServer = new TcpClientClass();

                //ListComboProtocols The calling thread cannot access this object because a different thread owns it
                InputIPConnection.Dispatcher.Invoke(() => InputIPConnection.IsEnabled = false);
                InputPORTConnection.Dispatcher.Invoke(() => InputPORTConnection.IsEnabled = false);
                ListComboProtocols.Dispatcher.Invoke(() => ListComboProtocols.IsEnabled = false);
                ButtonConnectConnection.Dispatcher.Invoke(() => ButtonConnectConnection.IsEnabled = false);
                //ListComboEncodings.Dispatcher.Invoke(() => ListComboEncodings.IsEnabled = false);

                Thread ThreadAttemptToConnectTCPAnimation = new Thread(() => AttemptToConnectTCPAnimation());
                ThreadAttemptToConnectTCPAnimation.Start();

                if (tcpServer.Start(inputIP, Int32.Parse(inputPort), GetActualEncoding(), this))
                {
                    if (ThreadAttemptToConnectTCPAnimation != null && ThreadAttemptToConnectTCPAnimation.IsAlive)
                    {
                        ThreadAttemptToConnectTCPAnimation.Abort();
                    }

                    tcpClientActive = true;

                    ButtonConnectConnection.Dispatcher.Invoke(() => ButtonConnectConnection.Content = "🛑 Disconnect");
                    TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = "Connected");

                    ButtonLoadSession.Dispatcher.Invoke(() => ButtonLoadSession.IsEnabled = false);
                    ButtonConnectConnection.Dispatcher.Invoke(() => ButtonConnectConnection.IsEnabled = true);
                    ButtonSendDataToSocket.Dispatcher.Invoke(() => ButtonSendDataToSocket.IsEnabled = true);
                    ButtonLoopContinuousConnections.Dispatcher.Invoke(() => ButtonLoopContinuousConnections.IsEnabled = true);

                    //rgba 230, 255, 0, 30%
                    BorderTextStatusConnection.Dispatcher.Invoke(() => BorderTextStatusConnection.Background = new SolidColorBrush(Color.FromArgb(50, 230, 250, 0)));

                    tcpServer.MessageReceived += TcpClient_MessageReceived;

                }
                else
                {
                    if (ThreadAttemptToConnectTCPAnimation != null && ThreadAttemptToConnectTCPAnimation.IsAlive)
                    {
                        ThreadAttemptToConnectTCPAnimation.Abort();
                    }

                    ListComboProtocols.Dispatcher.Invoke(() => ListComboProtocols.IsEnabled = true);
                    ButtonConnectConnection.Dispatcher.Invoke(() => ButtonConnectConnection.IsEnabled = true);
                    //ListComboEncodings.Dispatcher.Invoke(() => ListComboEncodings.IsEnabled = true);
                    InputIPConnection.Dispatcher.Invoke(() => InputIPConnection.IsEnabled = true);
                    ButtonLoadSession.Dispatcher.Invoke(() => ButtonLoadSession.IsEnabled = true);
                    InputPORTConnection.Dispatcher.Invoke(() => InputPORTConnection.IsEnabled = true);
                    TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = "⚠ Disconnected");


                    BorderTextStatusConnection.Dispatcher.Invoke(() => BorderTextStatusConnection.Background = new SolidColorBrush(Color.FromArgb(100 ,200, 108, 108)));
                }
            }
            catch (Exception)
            {

                throw;
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


        public Encoding GetActualEncoding()
        {
            try
            {
                //ListComboEncodings.Dispatcher.Invoke(() => ListComboEncodings.IsEnabled = false);
                object SelectedItem_ListComboEncodings = null;
                ListComboEncodings.Dispatcher.Invoke(() => SelectedItem_ListComboEncodings = ListComboEncodings.SelectedItem);

                if (SelectedItem_ListComboEncodings != null)
                {
                    string SelectedEncoding = SelectedItem_ListComboEncodings.ToString();

                    EncodingInfo Encoding = Encodings.FirstOrDefault(x => x.Name == SelectedEncoding);
                    return Encoding.Code;
                }

                return Encoding.ASCII;
            }
            catch (Exception)
            {
                return Encoding.ASCII;
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
                    }
                    else
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
                        ButtonLoopContinuousConnections.IsEnabled = false;

                        break;
                    case "UDP":
                        BorderTextStatusConnection.Visibility = Visibility.Hidden;
                        ButtonConnectConnection.Visibility = Visibility.Hidden;
                        ButtonSendDataToSocket.IsEnabled = true;
                        ButtonLoopContinuousConnections.IsEnabled = true;

                        break;
                    default:
                        Console.WriteLine("Looking forward to the Weekend.");

                        break;
                }

            }
        }

        private void ButtonCloseApplication_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //close ThreadConnectionClientTCP
                if (ThreadConnectionClientTCP != null && ThreadConnectionClientTCP.IsAlive)
                {
                    ThreadConnectionClientTCP.Abort();
                }

                if (tcpServer != null)
                {
                    tcpServer.Stop();
                }

                Application.Current.Shutdown();
            }
            catch (Exception)
            {
                Application.Current.Shutdown();
            }

        }

        private void ButtonSendDataToSocket_Click(object sender, RoutedEventArgs e)
        {
            //check if the list of protocols is tcp or udp or empty
            if (ListComboProtocols.SelectedItem == null)
            {
                return;
            }

            if (ListComboProtocols.SelectedItem.ToString() == "TCP")
            {
                if (tcpClientActive)
                {
                    tcpServer.SendMessage(TextBoxContentCommands.Text);
                }
            }
            
            if (ListComboProtocols.SelectedItem.ToString() == "UDP")
            {
                udpClient.SendMessage(InputIPConnection.Text, Int32.Parse(InputPORTConnection.Text), GetActualEncoding(), TextBoxContentCommands.Text, this);
            }
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


        #region Manage Sessions
        private void StartLocalSessions()
        {
            try
            {
                string filePath = @"C:\UBS\CommunicationProtocolTool.conf";
                if (!File.Exists(filePath))
                {
                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(filePath)))
                    {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                    }

                    if (!File.Exists(filePath))
                    {
                        File.Create(filePath).Close();
                        File.WriteAllText(filePath, "{}");
                    }
                }

                string fileText = File.ReadAllText(filePath);
                RootSessions ListRootSessions = JsonConvert.DeserializeObject<RootSessions>(fileText);

                ListSessions = ListRootSessions;
                UpdateListSessions();
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void UpdateSessionSave(string SessionName)
        {
            try
            {
                //get session named with the SessionName
                Session Session = ListSessions.Sessions.FirstOrDefault(x => x.NameSession == SessionName);
                if (Session == null)
                {
                    return;
                }

                Session.Connection.IP = InputIPConnection.Text;
                Session.Connection.PORT = InputPORTConnection.Text;


                string Encoding = String.Empty;
                string Protocol = String.Empty;
                if (ListComboEncodings.SelectedItem != null)
                {
                    Session.Connection.ENCODING = ListComboEncodings.SelectedItem.ToString();
                }

                if (ListComboProtocols.SelectedItem != null)
                {
                    Session.Connection.Protocol = ListComboProtocols.SelectedItem.ToString();
                }

                Session.Commands.ActualText = TextBoxContentCommands.Text;
                Session.Answer.AnswerTextBox = TextBoxRecivedInformation.Text;

                SaveSessionsInConfFile();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        private void ButtonSaveSession_Click(object sender, RoutedEventArgs e)
        {
            ListBoxSessions.SelectedItem = null;

            string NewSessionName = TextBoxNameSavedSession.Text;

            if (NewSessionName == String.Empty)
            {
                if (ListBoxSessions.SelectedItem == null)
                {
                    return;
                }

                NewSessionName = ListBoxSessions.SelectedItem.ToString();
            }

            if (ListSessions.Sessions != null)
            {
                if (ListSessions.Sessions.Any(x => x.NameSession == NewSessionName))
                {
                    UpdateSessionSave(NewSessionName);
                    return;
                }
            }

            string Encoding = String.Empty;
            string Protocol = String.Empty;
            if (ListComboEncodings.SelectedItem != null)
            {
                Encoding = ListComboEncodings.SelectedItem.ToString();
            }

            if (ListComboProtocols.SelectedItem != null)
            {
                Protocol = ListComboProtocols.SelectedItem.ToString();
            }

            Session Session = (new Session
            {
                NameSession = NewSessionName,
                Answer = new Answer
                {
                    AnswerTextBox = TextBoxRecivedInformation.Text
                },
                Commands = new Commands
                {
                    SelectedCommand = "",
                    ActualText = TextBoxContentCommands.Text
                },
                Connection = new Connection
                {
                    IP = InputIPConnection.Text,
                    PORT = InputPORTConnection.Text,
                    ENCODING = Encoding,
                    Protocol = Protocol
                }
            });
            
            if (ListSessions == null)
            {
                ListSessions = new RootSessions();
            }

            if (ListSessions.Sessions == null)
            {
                ListSessions.Sessions = new List<Session>();
            }

            ListSessions.Sessions.Add(Session);

            UpdateListSessions();
            SaveSessionsInConfFile();
        }

        private void SaveSessionsInConfFile()
        {
            try
            {
                //pass the ListSessions to json
                string json = JsonConvert.SerializeObject(ListSessions);

                string filePath = @"C:\UBS\CommunicationProtocolTool.conf";
                if (!File.Exists(filePath))
                {
                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(filePath)))
                    {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                    }

                    if (!File.Exists(filePath))
                    {
                        File.Create(filePath).Close();
                        File.WriteAllText(filePath, "{}");
                    }
                }

                File.WriteAllText(filePath, json);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void UpdateListSessions()
        {
            try
            {
                List<string> ListNameSessions = new List<string>();

                if (ListSessions.Sessions != null)
                {
                    for (global::System.Int32 i = 0; i < ListSessions.Sessions.Count; i++)
                    {
                        ListNameSessions.Add(ListSessions.Sessions[i].NameSession);
                    }

                    ListBoxSessions.ItemsSource = ListNameSessions;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        private void ButtonDeleteSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListBoxSessions.SelectedItem == null)
                {
                    return;
                }

                string SelectedSession = ListBoxSessions.SelectedItem.ToString();

                ListSessions.Sessions.RemoveAll(x => x.NameSession == SelectedSession);
                UpdateListSessions();
                SaveSessionsInConfFile();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ButtonLoadSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string SelectedSession = String.Empty;
                if (!GetSelectedSession(out SelectedSession))
                {
                    return;
                }

                Session Session = ListSessions.Sessions.FirstOrDefault(x => x.NameSession == SelectedSession);
                if (Session == null)
                {
                    return;
                }

                TextBoxNameSavedSession.Text = Session.NameSession;
                TextBoxContentCommands.Text = Session.Commands.ActualText;
                TextBoxRecivedInformation.Text = Session.Answer.AnswerTextBox;
                InputIPConnection.Text = Session.Connection.IP;
                InputPORTConnection.Text = Session.Connection.PORT;

                if (ListComboEncodings.Items.Contains(Session.Connection.ENCODING))
                {
                    ListComboEncodings.SelectedItem = Session.Connection.ENCODING;
                } else
                {
                    ListComboEncodings.SelectedItem = null;
                }

                if (ListComboProtocols.Items.Contains(Session.Connection.Protocol))
                {
                    ListComboProtocols.SelectedItem = Session.Connection.Protocol;
                } else
                {
                    ListComboProtocols.SelectedItem = null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool GetSelectedSession(out string NameSelectedSession)
        {
            NameSelectedSession = String.Empty;
            try
            {
                if (ListBoxSessions.SelectedItem == null)
                {
                    return false;
                }

                NameSelectedSession = ListBoxSessions.SelectedItem.ToString();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void TextBoxNameSavedSession_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ListBoxSessions.SelectedItem = null;
            }
        }

        private void TextBoxNameSavedSession_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private int IndexTextBoxContinuous;
        private void ButtonUpNumberContinuous_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Int32.TryParse(TextBoxContinuous.Text, out int Continuous))
                {
                    Continuous++;
                    IndexTextBoxContinuous = Continuous;
                    TextBoxContinuous.Text = Continuous.ToString();
                } else
                {
                    IndexTextBoxContinuous = IndexTextBoxContinuous + 1;
                    TextBoxContinuous.Text = IndexTextBoxContinuous.ToString();
                }
            }
            catch (Exception)
            {
            }
        }

        private void ButtonDownNumberContinuous_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Int32.TryParse(TextBoxContinuous.Text, out int Continuous))
                {
                    Continuous--;
                    if (Continuous < 1)
                    {
                        Continuous = 1;
                    }

                    IndexTextBoxContinuous = Continuous;
                    TextBoxContinuous.Text = Continuous.ToString();
                }
                else
                {
                    if (IndexTextBoxContinuous < 2)
                    {
                        IndexTextBoxContinuous = 2;
                    }

                    IndexTextBoxContinuous = IndexTextBoxContinuous - 1;
                    TextBoxContinuous.Text = IndexTextBoxContinuous.ToString();
                }
            }
            catch (Exception)
            {
            }
        }

        private void ButtonLoopContinuousConnections_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoopConnections != null && LoopConnections.IsAlive)
                {
                    LoopConnections.Abort();
                    ButtonLoopContinuousConnections.Content = "Continuous";

                    if (ListComboProtocols.SelectedItem.ToString() == "UDP")
                    {
                        ListComboProtocols.IsEnabled = true;
                        //ListComboEncodings.IsEnabled = true;
                        InputIPConnection.IsEnabled = true;
                        InputPORTConnection.IsEnabled = true;
                        InputIPConnection.IsEnabled = true;
                        InputPORTConnection.IsEnabled = true;
                    }

                    TextBoxContentCommands.IsEnabled = true;
                    ButtonSendDataToSocket.IsEnabled = true;
                    TextBoxContinuous.IsEnabled = true;
                    ButtonUpNumberContinuous.IsEnabled = true;
                    ButtonDownNumberContinuous.IsEnabled = true;

                } else
                {
                    if (ListComboProtocols.SelectedItem.ToString() == "UDP")
                    {
                        ListComboProtocols.IsEnabled = false;
                        //ListComboEncodings.IsEnabled = false;
                        InputIPConnection.IsEnabled = false;
                        InputPORTConnection.IsEnabled = false;
                    }

                    TextBoxContentCommands.IsEnabled = false;
                    ButtonSendDataToSocket.IsEnabled = false;
                    TextBoxContinuous.IsEnabled = false;
                    ButtonUpNumberContinuous.IsEnabled = false;
                    ButtonDownNumberContinuous.IsEnabled = false;

                    LoopConnections = new Thread(() => LoopConnectionsThread());
                    LoopConnections.Start();

                }

            }
            catch (Exception)
            {
                TextBoxContentCommands.IsEnabled = true;
            }
        }

        private void LoopConnectionsThread()
        {
            try
            {
                ButtonLoopContinuousConnections.Dispatcher.Invoke(() => ButtonLoopContinuousConnections.Content = "🛑 Continuous");

                while (true)
                {
                    if (tcpClientActive)
                    {
                        string ContentCommands = String.Empty;
                        TextBoxContentCommands.Dispatcher.Invoke(() => ContentCommands = TextBoxContentCommands.Text);

                        tcpServer.SendMessage(ContentCommands);
                    } else
                    {

                        string ContentCommands = String.Empty;
                        TextBoxContentCommands.Dispatcher.Invoke(() => ContentCommands = TextBoxContentCommands.Text);

                        string _InputIPConnection = String.Empty;
                        string _InputPORTConnection = String.Empty;
                        InputIPConnection.Dispatcher.Invoke(() => _InputIPConnection = this.InputIPConnection.Text);
                        InputPORTConnection.Dispatcher.Invoke(() => _InputPORTConnection = this.InputPORTConnection.Text);

                        udpClient.SendMessage(_InputIPConnection, Int32.Parse(_InputPORTConnection), GetActualEncoding(), ContentCommands, this);
                    }

                    int TimeSleep = 1;
                    TextBoxContinuous.Dispatcher.Invoke(() => TimeSleep = Int32.Parse(TextBoxContinuous.Text));
                    Thread.Sleep(TimeSleep * 1000);
                }

            }
            catch (Exception)
            {

            }
        }

        private void TextBoxSearchBoxListCommands_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string Text = TextBoxSearchBoxListCommands.Text;

                string[] files = Directory.GetFiles(@"commands", "*.xml");

                List<string> ListCommands = new List<string>();
                foreach (string file in files)
                {
                    ListCommands.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                }

                ListBoxCommands.ItemsSource = ListCommands;

                if (Text == String.Empty)
                {
                    ListBoxCommands.ItemsSource = ListCommands;
                    return;
                }

                List<string> ListCommandsFiltered = new List<string>();
                foreach (string Command in ListCommands)
                {
                    if ((Command.ToUpper()).Contains(Text.ToUpper()))
                    {
                        ListCommandsFiltered.Add(Command);
                    }
                }

                ListBoxCommands.ItemsSource = ListCommandsFiltered;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        private void ListBoxCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ListBoxCommands.SelectedItem == null)
                {
                    return;
                }

                string SelectedCommand = ListBoxCommands.SelectedItem.ToString();
                string filePath = @"commands\" + SelectedCommand + ".xml";

                if (!File.Exists(filePath))
                {
                    return;
                }

                string fileText = File.ReadAllText(filePath);
                TextBoxContentCommands.Text = fileText;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
