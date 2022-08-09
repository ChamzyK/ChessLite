using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChessLite
{
    public partial class MenuWindow : Window
    {
        public MenuWindow()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Window));
        }

        //Выход из приложения
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Owner.Close();
        }

        //Выход из главного окна
        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //О программе
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            
            var messageStackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            var messageTextBlock = new TextBlock
            {
                Text = "Программа: ChessLite\nРеализация:\nСибирский государственный\nуниверситет путей сообщения\nФакультет \"Бизнес-информатика\"\nБИСТ-211\nЧамзы К.Э. \nНовосибирск 2019г"
            };
            var messageButton = new Button
            {
                Content = "Вернуться"
            };
            messageButton.Click += (o, a) =>
            {
                Content = MenuStackPanel;
            };
            messageStackPanel.Children.Add(messageTextBlock);
            messageStackPanel.Children.Add(messageButton);
            Content = messageStackPanel;
        }

        //Сохранение
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "Сохранения|*.dat",
                Title = "Выберите файл с расширением .dat"
            };
            fileDialog.ShowDialog();
            if (fileDialog.FileName != string.Empty && Regex.IsMatch(fileDialog.FileName, "/*.dat$"))
            {
                (Owner as MainWindow).LoadBoard(fileDialog.FileName);
                Close();
            }
        }

        //Сохранение
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string saveName = string.Empty;

            //Инициализация элементов управления
            var saveStackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            var saveLabel = new Label
            {
                Content = "Введите название файла:",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0,10,0,0)
            };
            var saveTextBox = new TextBox
            {
                MaxLength = 16,
                Margin = new Thickness(15,0,15,0)
            };
            var okButton = new Button
            {
                Content = "Сохранить",
                IsEnabled = false
            };
            var cancelButton = new Button
            {
                Content = "Отмена"
            };

            //Подписка на события
            saveTextBox.TextChanged += (obj, arg) =>
            {
                okButton.IsEnabled = saveTextBox.Text != string.Empty;
            };
            okButton.Click += (obj, arg) =>
            {
                saveName = saveTextBox.Text;

                (Owner as MainWindow).SaveBoard(saveName);

                saveStackPanel.Children.Clear();
                var txtBlock = new TextBlock
                {
                    Text = "Успешно сохранено",
                    Margin = new Thickness(5,10,5,0)
                };
                var closeButton = new Button
                {
                    Content = "Ок",
                };
                closeButton.Click += (o, a) =>
                {
                    Close();
                };
                saveStackPanel.Children.Add(txtBlock);
                saveStackPanel.Children.Add(closeButton);
            };
            cancelButton.Click += (obj, arg) =>
            {
                Content = MenuStackPanel;
            };
            saveStackPanel.Children.Add(saveLabel);
            saveStackPanel.Children.Add(saveTextBox);
            saveStackPanel.Children.Add(okButton);
            saveStackPanel.Children.Add(cancelButton);
            Content = saveStackPanel;
        }

        //Перетаскивание
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
