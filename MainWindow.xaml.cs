using Newtonsoft.Json;
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
using System.Xml.Linq;
using System.Data.SqlTypes;
using System.Xml;
using System.Runtime.Remoting.Messaging;


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
        private UdpSocket udpSocket;
        private bool ContextMenuLanguageIsOpen;
        Translator translator = new Translator();
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

        public class Languages
        {
            public string Language { get; set; }
            public string Code { get; set; }

            public Languages(string language, string code)
            {
                Language = language;
                Code = code;
            }
        }

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
            StartAllTypeLanguages();
            StartAllTypeProtocol();
            StartAllComponent();
            StartLocalSessions();
            LoadListCommandsXML();

            TranslateAll();


            bool IsVersionPublished = TryGetEntryPointVersion(out string versionPublished);

            if (IsVersionPublished)
            {
                LabelVersionCommunicationProtocol.Content = string.Format("Version: {0}", versionPublished);
            } else
            {
                LabelVersionCommunicationProtocol.Content = string.Format("Version: {0}", Assembly.GetExecutingAssembly().GetName().Version);
            }

        }

        public void StartAllTypeLanguages()
        {
            try
            {
                List<Languages> ListLanguages = new List<Languages>()
                {
                    new Languages("English", "en-en"),
                    new Languages("Spanish", "es-es"),
                    new Languages("Portuguese", "pt-pt"),
                    new Languages("French", "fr-fr"),
                    new Languages("German", "de-de"),
                    new Languages("Italian", "it-it"),
                    new Languages("Russian", "ru-ru"),
                    new Languages("Chinese", "zh-cn"),
                    new Languages("Japanese", "ja-jp"),
                    new Languages("Korean", "ko-kr"),
                    new Languages("Arabic", "ar-sa"),
                    new Languages("Hindi", "hi-in"),
                    new Languages("Turkish", "tr-tr"),
                    new Languages("Dutch", "nl-nl"),
                    new Languages("Polish", "pl-pl"),
                    new Languages("Swedish", "sv-se"),
                    new Languages("Norwegian", "no-no"),
                    new Languages("Danish", "da-dk"),
                    new Languages("Finnish", "fi-fi"),
                    new Languages("Greek", "el-gr"),
                    new Languages("Czech", "cs-cz"),
                    new Languages("Hungarian", "hu-hu"),
                    new Languages("Romanian", "ro-ro"),
                    new Languages("Thai", "th-th"),
                    new Languages("Vietnamese", "vi-vn"),
                    new Languages("Indonesian", "id-id"),
                };

                List<string> ListComboBox = new List<string>();
                for (global::System.Int32 i = 0; i < ListLanguages.Count; i++)
                {
                    ListComboBox.Add(ListLanguages[i].Language);
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Error starting languages: " + ex.Message, "Communication Protocol Tool", true);
            }
        }

        public void ShowNotification(string Message, string Tittle = "", bool Error = false)
        {
            try
            {
                NotifyWindow notificationWindow = new NotifyWindow(this, Message, Tittle, Error);
                notificationWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error showing notification: " + ex.Message, "Communication Protocol Tool");
            }
        }

        private void LoadListCommandsXML()
        {
            try
            {
                string commandsFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands");
                if (Directory.Exists(commandsFolderPath))
                {
                    string[] files = Directory.GetFiles(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands"), "*.xml");
                    List<string> ListCommands = new List<string>();
                    foreach (string file in files)
                    {
                           ListCommands.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                    }

                    ListBoxCommands.ItemsSource = ListCommands;
                } else
                {
                    ShowNotification(translator.Translate("commandFolder404", translator.CurrentLanguage), "Communication Protocol Tool", true);
                }
            }
            catch (Exception)
            {

                
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
                        ShowNotification(ControllerMessageValidConnections, "Communication Protocol Tool", true);
                    }
                }
                else
                {
                    tcpServer.Stop();
                    ButtonConnectConnection.Content = "Connect";

                    ListComboProtocols.IsEnabled = true;
                    ButtonSendDataToSocket.IsEnabled = false;
                    ButtonLoopContinuousConnections.IsEnabled = false;
                    ButtonUpNumberContinuous.IsEnabled = false;
                    ButtonDownNumberContinuous.IsEnabled = false;
                    TextBoxContinuous.IsEnabled = false;

                    InputIPConnection.IsEnabled = true;
                    InputPORTConnection.IsEnabled = true;
                    //ListComboEncodings.IsEnabled = true;
                    ButtonLoadSession.IsEnabled = true;
                    TextStatusConnection.Content = translator.Translate("disconnected", translator.CurrentLanguage);

                    BorderTextStatusConnection.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    tcpClientActive = false;
                }
            }
            catch (Exception ex)
            {
                ShowNotification(translator.Translate("connection_failed", translator.CurrentLanguage) + ": " + ex.Message, "Communication Protocol Tool - Button Connect", true);
            }

        }

        private void AttemptToConnectTCPAnimation()
        { 
            try
            {
                while (true)
                {
                    TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = translator.Translate("connecting", translator.CurrentLanguage) + ".");
                    Thread.Sleep(250);                                                          
                    TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = translator.Translate("connecting", translator.CurrentLanguage) + "..");
                    Thread.Sleep(250);                                                          
                    TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = translator.Translate("connecting", translator.CurrentLanguage) + "...");
                    Thread.Sleep(250);                                                          
                    TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = translator.Translate("connecting", translator.CurrentLanguage) + "....");
                    Thread.Sleep(250);
                }
            }
            catch (Exception ex)
            {
                //ShowNotification("Connection failed: " + ex.Message, "Communication Protocol Tool - TCP Animation", true);
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

                    ButtonConnectConnection.Dispatcher.Invoke(() => ButtonConnectConnection.Content = translator.Translate("disconnect", translator.CurrentLanguage));
                    TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = translator.Translate("connected", translator.CurrentLanguage));

                    ButtonLoadSession.Dispatcher.Invoke(() => ButtonLoadSession.IsEnabled = false);
                    ButtonConnectConnection.Dispatcher.Invoke(() => ButtonConnectConnection.IsEnabled = true);
                    ButtonSendDataToSocket.Dispatcher.Invoke(() => ButtonSendDataToSocket.IsEnabled = true);
                    ButtonLoopContinuousConnections.Dispatcher.Invoke(() => ButtonLoopContinuousConnections.IsEnabled = true);
                    ButtonUpNumberContinuous.Dispatcher.Invoke(() => ButtonUpNumberContinuous.IsEnabled = true);
                    ButtonDownNumberContinuous.Dispatcher.Invoke(() => ButtonDownNumberContinuous.IsEnabled = true);
                    TextBoxContinuous.Dispatcher.Invoke(() => TextBoxContinuous.IsEnabled = true);

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
                    TextStatusConnection.Dispatcher.Invoke(() => TextStatusConnection.Content = translator.Translate("disconnected_warning", translator.CurrentLanguage));


                    BorderTextStatusConnection.Dispatcher.Invoke(() => BorderTextStatusConnection.Background = new SolidColorBrush(Color.FromArgb(100 ,200, 108, 108)));
                }
            }
            catch (Exception)
            {
                ShowNotification(translator.Translate("connection_failed", translator.CurrentLanguage), "Communication Protocol Tool - Client Thread", true);
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
                ShowNotification(translator.Translate("error_reciving_message", translator.CurrentLanguage) + ": " + ex.Message, "Communication Protocol Tool", true);
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
                    MessageErrors.Add(translator.Translate("error_ip_format", translator.CurrentLanguage));
                }

                if (!IsValidPort(inputPort))
                {
                    if (inputPort != String.Empty)
                    {
                        MessageErrors.Add(translator.Translate("error_port_format", translator.CurrentLanguage));
                    }
                    else
                    {
                        MessageErrors.Add(translator.Translate("error_port_empty", translator.CurrentLanguage));
                    }
                }

                string StringMessageBox = translator.Translate("error_input_connection", translator.CurrentLanguage) + ": \n";

                for (Int32 i = 0; i < MessageErrors.Count; i++)
                {
                    StringMessageBox += ((i + 1) + ". " + MessageErrors[i] + "\n");
                }

                ControllerMessageValidConnections = StringMessageBox;
                return false;
            }
            catch (Exception ex)
            {
                ControllerMessageValidConnections = String.Empty;
                return false;
            }
        }
        private bool IsValidPort(string portString)
        {
            try
            {
                if (!int.TryParse(portString, out int port))
                {
                    return false;
                }

                return port >= 10 && port <= 99999;
            }
            catch (Exception ex)
            {
                ShowNotification("Error for validating Port: " + ex.Message, "Communication Protocol Tool", true);
                return false;
            }
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
            try
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
                            ButtonUpNumberContinuous.IsEnabled = false;
                            ButtonDownNumberContinuous.IsEnabled = false;
                            TextBoxContinuous.IsEnabled = false;

                            break;
                        case "UDP":
                            BorderTextStatusConnection.Visibility = Visibility.Hidden;
                            ButtonConnectConnection.Visibility = Visibility.Hidden;
                            ButtonSendDataToSocket.IsEnabled = true;
                            ButtonLoopContinuousConnections.IsEnabled = true;
                            ButtonUpNumberContinuous.IsEnabled = true;
                            ButtonDownNumberContinuous.IsEnabled = true;
                            TextBoxContinuous.IsEnabled = true;

                            break;
                        default:
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                ShowNotification("Error changing protocol: " + ex.Message, "Communication Protocol Tool", true);
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
            catch (Exception ex)
            {
                ShowNotification("Error closing application: " + ex.Message, "Communication Protocol Tool", true);
                Application.Current.Shutdown();
            }

        }

        private void ButtonSendDataToSocket_Click(object sender, RoutedEventArgs e)
        {
            try
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
                    //udpClient.SendMessage(InputIPConnection.Text, Int32.Parse(InputPORTConnection.Text), GetActualEncoding(), TextBoxContentCommands.Text, this);
                    udpSocket = new UdpSocket(InputIPConnection.Text, Int32.Parse(InputPORTConnection.Text));
                    udpSocket.MessageReceived += UdpSocket_MessageReceived;

                    // Iniciar un hilo o temporizador para recibir mensajes automáticamente
                    // Puedes ajustar esto según tus necesidades
                    Task.Run(() => ReceiveMessagesAutomatically());

                    udpSocket.SendData(TextBoxContentCommands.Text);
                }
            }
            catch (Exception ex)
            {
                ShowNotification(translator.Translate("error_sending_data", translator.CurrentLanguage) + ": " + ex.Message, "Communication Protocol Tool", true);
            }
        }

        private void ReceiveMessagesAutomatically()
        {
            while (true)
            {
                // Llamar a ReceiveData para recibir mensajes automáticamente
                string mensajeRecibido = udpSocket.ReceiveData();
                if (mensajeRecibido != null)
                {
                    // Actualizar el TextBox con el mensaje recibido
                    this.Dispatcher.Invoke(() => this.TextBoxRecivedInformation.Text = this.TextBoxRecivedInformation.Text + mensajeRecibido.ToString() + "\n");
                }

                // Agregar un retraso o manejo según tus necesidades
                Thread.Sleep(1000);
            }
        }

        private void UdpSocket_MessageReceived(object sender, string message)
        {
            // Actualizar el TextBox con el mensaje recibido
            this.Dispatcher.Invoke(() => this.TextBoxRecivedInformation.Text = this.TextBoxRecivedInformation.Text + message.ToString() + "\n");
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

                //throw;
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
            catch (Exception ex)
            {
                ShowNotification("Error deleting session: " + ex.Message, "Communication Protocol Tool", true);
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
                ShowNotification("Error loading session", "Communication Protocol Tool", true);
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
            catch (Exception ex)
            {
                ShowNotification("Error changing continuous number: " + ex.Message, "Communication Protocol Tool", true);
            }
        }

        private void ButtonLoopContinuousConnections_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string inputIP = InputIPConnection.Text;
                string inputPort = InputPORTConnection.Text;
                string ControllerMessageValidConnections = String.Empty;

                if (!IsValidConnections(inputIP, inputPort, out ControllerMessageValidConnections))
                {
                    ShowNotification(ControllerMessageValidConnections, "Communication Protocol Tool", true);
                    return;
                }

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
                        ButtonLoadSession.IsEnabled = true;
                    }

                    TextBoxContentCommands.IsEnabled = true;
                    ButtonSendDataToSocket.IsEnabled = true;
                    TextBoxContinuous.IsEnabled = true;
                    ButtonUpNumberContinuous.IsEnabled = true;
                    ButtonDownNumberContinuous.IsEnabled = true;
                    ListBoxCommands.IsEnabled = true;

                } else
                {
                    if (ListComboProtocols.SelectedItem.ToString() == "UDP")
                    {
                        ListComboProtocols.IsEnabled = false;
                        //ListComboEncodings.IsEnabled = false;
                        InputIPConnection.IsEnabled = false;
                        InputPORTConnection.IsEnabled = false;
                        ButtonLoadSession.IsEnabled = false;
                    }

                    TextBoxContentCommands.IsEnabled = false;
                    ButtonSendDataToSocket.IsEnabled = false;
                    TextBoxContinuous.IsEnabled = false;
                    ButtonUpNumberContinuous.IsEnabled = false;
                    ButtonDownNumberContinuous.IsEnabled = false;
                    ListBoxCommands.IsEnabled = false;

                    LoopConnections = new Thread(() => LoopConnectionsThread());
                    LoopConnections.Start();

                }

            }
            catch (Exception)
            {
                TextBoxContentCommands.IsEnabled = true;
                ShowNotification("Error starting continuous connections", "Communication Protocol Tool", true);
            }
        }

        public void ConnectionTCPClientClosed()
        {
            try
            {
                ButtonConnectConnection.Content = translator.Translate("connect", translator.CurrentLanguage);

                ListComboProtocols.IsEnabled = true;
                ButtonSendDataToSocket.IsEnabled = false;
                ButtonLoopContinuousConnections.IsEnabled = false;
                ButtonUpNumberContinuous.IsEnabled = false;
                ButtonDownNumberContinuous.IsEnabled = false;
                TextBoxContinuous.IsEnabled = false;

                InputIPConnection.IsEnabled = true;
                InputPORTConnection.IsEnabled = true;
                //ListComboEncodings.IsEnabled = true;
                ButtonLoadSession.IsEnabled = true;
                TextStatusConnection.Content = translator.Translate("server_closed", translator.CurrentLanguage);

                BorderTextStatusConnection.Background = new SolidColorBrush(Color.FromArgb(100, 200, 108, 108));

                tcpClientActive = false;

                if (LoopConnections != null && LoopConnections.IsAlive)
                {
                    LoopConnections.Abort();
                    ButtonLoopContinuousConnections.Content = translator.Translate("continuous", translator.CurrentLanguage);

                    TextBoxContentCommands.IsEnabled = true;
                    ButtonSendDataToSocket.IsEnabled = true;
                    TextBoxContinuous.IsEnabled = true;
                    ButtonUpNumberContinuous.IsEnabled = true;
                    ButtonDownNumberContinuous.IsEnabled = true;
                    ListBoxCommands.IsEnabled = true;
                }   
            }
            catch (Exception ex)
            {
                ShowNotification("Error stoping resource connections", "Communication Protocol Tool", true);
            }
        }

        private void LoopConnectionsThread()
        {
            try
            {
                ButtonLoopContinuousConnections.Dispatcher.Invoke(() => ButtonLoopContinuousConnections.Content = translator.Translate("continuous_active", translator.CurrentLanguage));

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

                        //udpClient.SendMessage(_InputIPConnection, Int32.Parse(_InputPORTConnection), GetActualEncoding(), ContentCommands, this);
                        udpSocket = new UdpSocket(_InputIPConnection, Int32.Parse(_InputPORTConnection));
                        udpSocket.MessageReceived += UdpSocket_MessageReceived;

                        // Iniciar un hilo o temporizador para recibir mensajes automáticamente
                        // Puedes ajustar esto según tus necesidades
                        Task.Run(() => ReceiveMessagesAutomatically());

                        udpSocket.SendData(ContentCommands);
                    }

                    int TimeSleep = 1;
                    //TextBoxContinuous.Dispatcher.Invoke(() => TimeSleep = Int32.Parse(TextBoxContinuous.Text));
                    Thread.Sleep(TimeSleep * 1000);
                }

            }
            catch (Exception ex)
            {
                if (ex.Message != "Thread was being aborted.")
                {
                    this.ShowNotification("Error starting continuous connections: " + ex.Message, "Communication Protocol Tool", true);
                }
            }
        }

        static bool TryGetEntryPointVersion(out string version)
        {
            version = null;

            try
            {
                string fileName = "CommunicationProtocol.exe.manifest";
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                if (File.Exists(filePath))
                {
                    string fileText = File.ReadAllText(filePath);

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(fileText);

                    XmlNodeList assemblyIdentityNodes = xmlDoc.SelectNodes("//asmv1:assemblyIdentity", GetNamespaceManager(xmlDoc));

                    if (assemblyIdentityNodes[0].Attributes["version"].Value != null)
                    {
                        version = assemblyIdentityNodes[0].Attributes["version"].Value;
                    }

                    return version != null;
                }
                else
                {
                    Console.WriteLine($"El archivo {fileName} no se encuentra en la carpeta raíz del programa.");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Se produjo un error al intentar cargar el archivo.");
                return false;
            }
        }

        private static XmlNamespaceManager GetNamespaceManager(XmlDocument xmlDoc)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("asmv1", "urn:schemas-microsoft-com:asm.v1");
            return namespaceManager;
        }

        private void TextBoxSearchBoxListCommands_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {

                string commandsFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands");
                if (Directory.Exists(commandsFolderPath))
                {
                    string Text = TextBoxSearchBoxListCommands.Text;
                    string[] files = Directory.GetFiles(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands"), "*.xml");

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
                } else
                {
                    ShowNotification(translator.Translate("commandFolder404", translator.CurrentLanguage), "Communication Protocol Tool", true);
                }

            }
            catch (Exception ex)
            {
                ShowNotification("Error searching commands: " + ex.Message, "Communication Protocol Tool", true);
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

                string commandsFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands");
                if (Directory.Exists(commandsFolderPath))
                {
                    string SelectedCommand = ListBoxCommands.SelectedItem.ToString();
                    string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands").ToString() + "\\" + SelectedCommand + ".xml";

                    if (!File.Exists(filePath))
                    {
                        return;
                    }

                    string fileText = File.ReadAllText(filePath);
                    TextBoxContentCommands.Text = fileText;
                } else
                {
                    ShowNotification(translator.Translate("commandFolder404", translator.CurrentLanguage), "Communication Protocol Tool", true);
                }

            }
            catch (Exception ex)
            {
                ShowNotification("Error selecting command: " + ex.Message, "Communication Protocol Tool", true);
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNotification("Info button comming soon...");
        }

        public void TranslateAll()
        {
            try
            {
                LabelTittleAnswer.Content = translator.Translate("answer", translator.CurrentLanguage);
                LabelCommands.Content = translator.Translate("commands", translator.CurrentLanguage);
                LabelConnection.Content = translator.Translate("connection", translator.CurrentLanguage);
                GroupBoxConnection.Header = translator.Translate("connection", translator.CurrentLanguage);
                GroupBoxProtocol.Header = translator.Translate("protocol", translator.CurrentLanguage);
                ButtonLoopContinuousConnections.Content = translator.Translate("continuous", translator.CurrentLanguage);
                LabelSavedSessions.Content = translator.Translate("saved_sessions", translator.CurrentLanguage);
                ButtonLoadSession.Content = translator.Translate("loadSession", translator.CurrentLanguage);
                ButtonSaveSession.Content = translator.Translate("saveSession", translator.CurrentLanguage);
                ButtonDeleteSession.Content = translator.Translate("deleteSession", translator.CurrentLanguage);
                GroupBoxSessions.Header = translator.Translate("sessionsExplain", translator.CurrentLanguage);
                LabelSessions.Content = translator.Translate("sessions", translator.CurrentLanguage);
                ButtonConnectConnection.Content = translator.Translate("connect", translator.CurrentLanguage);
                LabelConnectionPort.Content = translator.Translate("port", translator.CurrentLanguage);
                LabelConnectionIP.Content = translator.Translate("ip", translator.CurrentLanguage);
                ButtonSendDataToSocket.Content = translator.Translate("send", translator.CurrentLanguage);
                LabelProtocol.Content = translator.Translate("protocol", translator.CurrentLanguage);
                GroupBoxEncoding.Header = translator.Translate("encoding", translator.CurrentLanguage);
                LabelTimeSecs.Content = translator.Translate("LabelTimeSecs", translator.CurrentLanguage);

                translator.ChangeIconLanguage(this);
            }
            catch (Exception ex)
            {
                ShowNotification("Error translating UI: " + ex.Message, "Communication Protocol Tool", true);
            }
        }

        private void ButtonLanguagContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ContextMenuLanguageIsOpen)
            {
                LanguageContextMenu.Visibility = Visibility.Hidden;
                ContextMenuLanguageIsOpen = false;
            } else
            {
                LanguageContextMenu.Visibility = Visibility.Visible;
                ContextMenuLanguageIsOpen = true;
            }
        }

        private void ChangeLanguageUI_spanish(object sender, RoutedEventArgs e)
        {
            translator.ChangeLanguage(this, "es-es");
        }

        private void ChangeLanguageUI_english(object sender, RoutedEventArgs e)
        {
            translator.ChangeLanguage(this, "en-en");
        }

        private void ChangeLanguageUI_italiano(object sender, RoutedEventArgs e)
        {
            translator.ChangeLanguage(this, "it-it");
        }

        private void ChangeLanguageUI_french(object sender, RoutedEventArgs e)
        {
            translator.ChangeLanguage(this, "fr-fr");
        }

        private void ChangeLanguageUI_norwegian(object sender, RoutedEventArgs e)
        {
            translator.ChangeLanguage(this, "no-no");
        }
    }
}
