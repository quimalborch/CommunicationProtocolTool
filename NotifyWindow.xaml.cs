using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CommunicationProtocol
{
    /// <summary>
    /// Interaction logic for NotifyWindow.xaml
    /// </summary>
    public partial class NotifyWindow : Window
    {
        public MainWindow MainWindow { get; set; }
        public NotifyWindow(MainWindow argMainWindow, string Message, string Tittle = "", bool Error = false)
        {
            MainWindow = argMainWindow;
            InitializeComponent();
            //MainWindow.IsEnabled = false;

            if (Error)
            {
                BorderNotify.BorderBrush = new SolidColorBrush(Colors.Red);
            }

            if (!string.IsNullOrEmpty(Tittle))
            {
                TextBlockTittle.Content = Tittle;
            }

            TextBlockNotify.Text = Message;
        }

        private void ButtonCloseNotify_Click(RoutedEventArgs e)
        {

        }

        private void ButtonCloseNotify_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //MainWindow.IsEnabled = true;
        }

        private void BottomContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void TextBlockTittle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
