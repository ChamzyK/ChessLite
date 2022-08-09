using ChessLite.Network;
using System.Windows;
using System.Windows.Controls;

namespace ChessLite
{
    //Никогда не повторять, опасно для жизни!!! Этот код писался по приколу и для прикола
    public partial class NamesWindow : Window
    {
        public GameType GameType { get; protected set; }
        internal StackPanel typeStackPanel;
        public NamesWindow()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Window));

            MouseDown += (s, e) =>
            {
                DragMove();
            };

            //Элементы управления
            typeStackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            var localButton = new Button()
            {
                Content = "По сети"
            };
            var singleButton = new Button()
            {
                Content = "На одном компьютере"
            };
            var cancelButton = new Button()
            {
                Content = "Отмена"
            };
            typeStackPanel.Children.Add(localButton);
            typeStackPanel.Children.Add(singleButton);
            typeStackPanel.Children.Add(cancelButton);

            //События
            cancelButton.Click += (s, e) =>
            {
                this.Close();
            };
            localButton.Click += (s, e) =>
            {
                GameType = GameType.Local;
                Content = new LocalSettings();
            };
            singleButton.Click += (s, e) =>
            {
                GameType = GameType.Single;
                Content = NameStackPanel;
            };
            Content = typeStackPanel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow).GameType = GameType.Single;
            (Owner as MainWindow).StartNewGame(Name1_txtBlock.Text, Name2_txtBlock.Text);
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Content = typeStackPanel;
        }

        private void txtBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            btn.IsEnabled = Name1_txtBlock.Text != string.Empty && Name2_txtBlock.Text != string.Empty;
        }
    }
}
