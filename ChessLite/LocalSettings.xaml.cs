using ChessLite.Network;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChessLite
{
    public partial class LocalSettings : UserControl
    {
        private IPAddress IPAddress;
        private int Port;
        private TextBox addressTextBox;
        private Label addressLabel;
        public object Timer { get; private set; }

        public LocalSettings()
        {
            InitializeComponent();
            addressLabel = new Label { Content = "Введите адрес: " };
            addressTextBox = new TextBox
            {
                Margin = new Thickness(10, 0, 10, 0)
            };
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Элементы управления
            var writeStackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            
            var portLabel = new Label { Content = "Введите порт: " };
            var portTextBox = new TextBox
            {
                Margin = new Thickness(10, 0, 10, 0)
            };
            var okButton = new Button
            {
                Content = (sender as Button).Content,
                IsEnabled = false
            };
            var backButton = new Button { Content = "Назад" };
            void textBox_TextChanged(object obj, TextChangedEventArgs args)
            {
                int port = -1;
                bool portIsRight = int.TryParse(portTextBox.Text, out port) && port <= 65535 && port >= 0;
                bool addressIsRight = !((sender as Button).Content.ToString() == "Создать") ? IPAddress.TryParse(addressTextBox.Text, out IPAddress) : true;
                okButton.IsEnabled = addressIsRight &&
                                     portIsRight;
            }

            //События
            addressTextBox.TextChanged += textBox_TextChanged;
            portTextBox.TextChanged += textBox_TextChanged;
            okButton.Click += (s, arg) =>
            {
                Port = Convert.ToInt32(portTextBox.Text);
                if (sender == createButton)
                    CreateButton();
                else
                    ConnectButton();
            };
            backButton.Click += (s, arg) =>
            {
                IPAddress = null;
                Content = stackPanel;
            };

            //Добавление
            if (!((sender as Button).Content.ToString() == "Создать"))
            {
                writeStackPanel.Children.Add(addressLabel);
                writeStackPanel.Children.Add(addressTextBox);
            }
            writeStackPanel.Children.Add(portLabel);
            writeStackPanel.Children.Add(portTextBox);
            writeStackPanel.Children.Add(okButton);
            writeStackPanel.Children.Add(backButton);

            Content = writeStackPanel;
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as NamesWindow).Content = (Parent as NamesWindow).typeStackPanel;
        }


        private void CreateButton()
        {
            var waitTextBlock = new TextBlock
            {
                Text = "Ожидание подключения\nдругого игрока...",
                FontSize = 24,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            ((Parent as NamesWindow).Owner as MainWindow).IamServerAsync(IPAddress.ToString(),Port, textBox.Text,this);
            Content = waitTextBlock;
        }

        private void ConnectButton()
        {
            ((Parent as NamesWindow).Owner as MainWindow).IamClientAsync(IPAddress.ToString(), Port, textBox.Text);
            (Parent as NamesWindow).Close();
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var isEnabled = textBox.Text != string.Empty;
            createButton.IsEnabled = isEnabled;
            connectButton.IsEnabled = isEnabled;
        }
    }
}
