using Chess;
using Chess.Pieces;
using Chess.Pieces.Moves;
using ChessLite.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChessLite
{
    public partial class MainWindow : Window
    {
        //Шахматная доска
        private Board board;

        //Выбранная фигура
        private Piece selectedPiece;

        //Зеленый
        private readonly Brush MyGreenBrush;

        //Красный
        private readonly Brush MyRedBrush;

        //Для хранения изображений
        private readonly Dictionary<string, BitmapSource> piecesImages;

        //Фоновый рисунок
        private ImageBrush menuBackground;

        //Пустое изображение
        private Image image;

        public GameType GameType { get; set; }

        //Конструктор
        public MainWindow()
        {
            InitializeComponent();
            image = new Image();
            Back_btn.IsEnabled = false;
            Style = (Style)FindResource(typeof(Window));
            var imgBrush = new ImageBrush
            {
                ImageSource = Imaging.CreateBitmapSourceFromHBitmap
                        ((Properties.Resources.ResourceManager.GetObject("Буквы") as System.Drawing.Bitmap).GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions())
            };
            boardBorder.Background = imgBrush;
            menuBackground = new ImageBrush
            {
                ImageSource = Imaging.CreateBitmapSourceFromHBitmap
                        (Properties.Resources.MenuBackground.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions())
            };
            piecesImages = new Dictionary<string, BitmapSource>();
            for (int i = 0; i < 6; i++)
            {
                var imageResourceNameW = "W" + (PieceType.Pawn + i);
                var imageResourceNameB = "B" + (PieceType.Pawn + i);

                piecesImages.Add(
                    imageResourceNameB,
                    Imaging.CreateBitmapSourceFromHBitmap
                        ((Properties.Resources.ResourceManager.GetObject(imageResourceNameB) as System.Drawing.Bitmap).GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions()));

                piecesImages.Add(
                    imageResourceNameW,
                    Imaging.CreateBitmapSourceFromHBitmap
                        ((Properties.Resources.ResourceManager.GetObject(imageResourceNameW) as System.Drawing.Bitmap).GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions()));
            }

            var brushConverter = new BrushConverter();
            MyGreenBrush = (Brush)brushConverter.ConvertFrom("#FF45E845");
            MyRedBrush = (Brush)brushConverter.ConvertFrom("#FFE44B4B");

            //создание доски (отображение)
            //Перебор по строкам
            for (int i = 0; i < 8; i++)
            {
                //Добавление столбца и строки
                boardGrid.RowDefinitions.Add(new RowDefinition());
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition());

                //Перебор по столбцам
                for (int j = 0; j < 8; j++)
                {
                    //Новый прямоугольник
                    var rect = new Rectangle
                    {
                        Fill = (i + j) % 2 == 0 ? Brushes.White : Brushes.Gray,
                        Stroke = Brushes.Black
                    };

                    //Добавление прямоугльника
                    Grid.SetRow(rect, i);
                    Grid.SetColumn(rect, j);
                    boardGrid.Children.Add(rect);

                    //Новое пустое изображение (в будущем при работе с доской чтобы на каждой клетке гарантировано было изображение)
                    var image = new Image();
                    Grid.SetRow(image, i);
                    Grid.SetColumn(image, j);
                    boardGrid.Children.Add(image);
                }
            }

            Player1_txtBlock.Text = "Начните новую игру";
            Player2_txtBlock.Text = "Или загрузите сохраненную";
        }

        private void RefreshMoveInfo(object sender, MoveInfo args)
        {
            TurnCount_txtBlock.Text = "Количество ходов: " + board.TurnCount.ToString();
            Back_btn.IsEnabled = board.TurnCount != 0 && GameType != GameType.Local;
            if (board.WhoCanTurn == Chess.Pieces.Color.Black)
            {
                Player1_txtBlock.Background = board.IsChecked ? Brushes.Yellow : MyGreenBrush;
                Player2_txtBlock.Background = MyRedBrush;
            }
            else
            {
                Player2_txtBlock.Background = board.IsChecked ? Brushes.Yellow : MyGreenBrush;
                Player1_txtBlock.Background = MyRedBrush;
            }
        }

        private void ShowGameEndDialog(GameResult result)
        {
            string resultString = string.Empty;

            switch (result)
            {
                case GameResult.WinBlack:
                    resultString = $"Игрок {board.Player1} одержал победу!!!";
                    break;
                case GameResult.WinWhite:
                    resultString = $"Игрок {board.Player2} одержал победу!!!";
                    break;
                case GameResult.Draw:
                    resultString = "Партия закончилась ничьей";
                    break;
            }
            var resultWin = new Window
            {
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Background = menuBackground,
                Height = 250,
                Width = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            var resultStackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var resultTextBlock = new TextBlock
            {
                Text = resultString,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 26
            };
            var resultButton = new Button
            {
                Content = "Ок"
            };
            resultWin.MouseDown += (o, a) =>
            {
                resultWin.DragMove();
            };
            resultButton.Click += (o, a) =>
            {
                resultWin.Close();
            };
            resultStackPanel.Children.Add(resultTextBlock);
            resultStackPanel.Children.Add(resultButton);
            resultWin.Content = resultStackPanel;
            resultWin.ShowDialog();
        }

        private PieceType ShowQuestionDialog(Piece piece)
        {
            PieceType type = PieceType.Qween;
            var qWin = new Window()
            {
                Background = menuBackground,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Height = 355,
                Width = 250,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            //Создаем элементы управления
            var buttons = new Button[]
            {
                new Button
                {
                    Content = "Ферзь"
                },
                new Button
                {
                    Content = "Конь"
                },new Button
                {
                    Content = "Ладья"
                },new Button
                {
                    Content = "Слон"
                }
            };
            var qTextBlock = new TextBlock
            {
                Text = "Ваша пешка дошла до последней линии. \nВыберите в какую фигуру вы хотите её превратить:",
                TextWrapping = TextWrapping.Wrap
            };

            //Обработчик события для всех кнопок
            void Click(object sender, RoutedEventArgs args)
            {
                var btn = (Button)sender;
                switch (btn.Content)
                {
                    case "Ферзь":
                        type = PieceType.Qween;
                        break;
                    case "Конь":
                        type = PieceType.Knight;
                        break;
                    case "Ладья":
                        type = PieceType.Castle;
                        break;
                    case "Слон":
                        type = PieceType.Bishop;
                        break;
                }
                qWin.Close();
            }

            //Подписываемся на метод Click
            foreach (var item in buttons)
            {
                item.Click += Click;
            }

            //Создаем контейнер компановки   
            var qStackPanel = new StackPanel()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center

            };
            qStackPanel.Children.Add(qTextBlock);
            foreach (var item in buttons)
            {
                qStackPanel.Children.Add(item);
            }

            //Настраиваем окно перед запуском
            qWin.Content = qStackPanel;
            qWin.MouseDown += (o, a) =>
            {
                qWin.DragMove();
            };
            qWin.ShowDialog();
            return type;
        }

        private void ColoringPlayerTextBox(TextBlock player_TextBlock, Brush color)
        {
            player_TextBlock.Background = color;
        }

        //Инициализация новой доски
        private void InitNewBoard()
        {
            if (board != null)
            {
                //При каждом ходе перерисовываются только клетки в которых произошли изменения
                board.PieceMoveEvent += RefreshBoardImage;

                Player1_txtBlock.Background = board.WhoCanTurn == Chess.Pieces.Color.Black ? MyGreenBrush : MyRedBrush;
                Player2_txtBlock.Background = board.WhoCanTurn == Chess.Pieces.Color.White ? MyGreenBrush : MyRedBrush;

                if (board.IsChecked)
                {
                    ColoringPlayerTextBox(board.WhoCanTurn == Chess.Pieces.Color.Black ? Player1_txtBlock : Player2_txtBlock, Brushes.Yellow);
                }

                TurnCount_txtBlock.Text = "Количество ходов: " + board.TurnCount.ToString();

                board.PieceMoveEvent += RefreshMoveInfo;

                board.GameEndEvent += ShowGameEndDialog;

                board.Question += ShowQuestionDialog;


                TurnHistory_lstBox.ItemsSource = board.Turns;

                //Имя первого игрока (черные)
                Player1_txtBlock.Text = board.Player1;
                //Имя второго игрока (белые)
                Player2_txtBlock.Text = board.Player2;

                //Удаление предыдущих изображений
                for (int i = 0; i < 64; i++)
                {
                    var index = (i * 2) + 1;
                    (boardGrid.Children[index] as Image).Source = image.Source;
                }

                //Полная отрисовка всех фигур
                InitBoard();
            }
        }

        //Индекс в коллекции Children у Grid
        private int GetIndexOfSquare(int X, int Y)
        {
            return Y * 8 + X;
        }

        //Обновление изображения шахматной доски
        private void RefreshBoardImage(object arg1, MoveInfo arg2)
        {
            RefreshImage(arg2.BeginX, arg2.BeginY);

            if (arg2.Type == MoveType.castling)
            {

                RefreshImage(arg2.BeginX - arg2.EndX > 0 ? 1 : 8, arg2.BeginY);

                RefreshImage(arg2.EndX, arg2.EndY);

                RefreshImage(arg2.EndX == 3 ? 4 : 6, arg2.BeginY);
            }
            else
            {
                RefreshImage(arg2.EndX, arg2.EndY);
            }
        }

        private void RefreshImage(int X, int Y)
        {
            int index = (GetIndexOfSquare(X - 1, 8 - Y) * 2) + 1;
            var isEmpty = board.PiecesArr[X - 1, Y - 1] == null;
            (boardGrid.Children[index] as Image).Source = isEmpty ? image.Source : piecesImages[GetNamePicture(board.PiecesArr[X - 1, Y - 1])];
        }

        //Изначальная инициализация доски (отображение)
        private void InitBoard()
        {
            foreach (var item in board.PiecesArr)
            {
                if (item != null)
                {
                    var index = (GetIndexOfSquare(item.X - 1, 8 - item.Y) * 2) + 1;
                    (boardGrid.Children[index] as Image).Source = piecesImages[GetNamePicture(item)];
                }
            }
        }

        //Получение имени картинки фигуры для испольования ресурсов
        private string GetNamePicture(Piece piece)
        {
            string result = string.Empty;
            result += piece.Color == Chess.Pieces.Color.White ? "W" : "B";
            result += piece.Type;
            return result;
        }

        //Обводка выбранной фигуры
        private void SelectedSquareBorderDraw(int X, int Y)
        {
            var index = GetIndexOfSquare(X, Y) * 2;
            var bc = new BrushConverter();
            (boardGrid.Children[index] as Rectangle).Stroke = (Brush)bc.ConvertFrom("#FF45E845");
            (boardGrid.Children[index] as Rectangle).StrokeThickness = 4;
        }

        //Отмена обводки
        private void SelectedSquareBorderReturn(int X, int Y)
        {
            var index = GetIndexOfSquare(X, Y) * 2;
            (boardGrid.Children[index] as Rectangle).Stroke = Brushes.Black;
            (boardGrid.Children[index] as Rectangle).StrokeThickness = 1;
        }

        //TODO: если будет время добавить обводку прошлой клетки (для наглядности откуда и куда ходил противник)

        //Клик по доске
        private void boardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (board != null && (GameType == GameType.Single  || (GameType == GameType.Local && board.WhoCanTurn == MyColor)))
            {
                var element = (UIElement)e.Source;

                var X = Grid.GetColumn(element) + 1;
                var Y = 8 - Grid.GetRow(element);

                if (X < 9 && X > 0 && Y < 9 && Y > 0)
                {
                    var piece = board.GetPiece(X, Y);
                    if (selectedPiece != null)
                    {
                        var prev_X = selectedPiece.X;
                        var prev_Y = selectedPiece.Y;
                        var isMove = board.Move(prev_X, prev_Y, X, Y);
                        if (isMove)
                        {
                            selectedPiece = null;
                            SelectedSquareBorderReturn(prev_X - 1, 8 - prev_Y);
                            if(GameType == GameType.Local)
                            {
                                if (Client != null)
                                {
                                    Client.Send(new byte[] { (byte)prev_X, (byte)prev_Y, (byte)X, (byte)Y });
                                }
                                else answer = new byte[] { (byte)prev_X, (byte)prev_Y, (byte)X, (byte)Y };
                            }
                        }
                        else if (piece != null && selectedPiece.Color == piece.Color)
                        {
                            SelectedSquareBorderReturn(prev_X - 1, 8 - prev_Y);
                            selectedPiece = piece;
                            SelectedSquareBorderDraw(X - 1, 8 - Y);
                        }
                    }
                    else if (piece != null && piece.Color == board.WhoCanTurn)
                    {
                        selectedPiece = piece;
                        SelectedSquareBorderDraw(X - 1, 8 - Y);
                    }
                }
            }
        }
        //Перегруженная версия (для сети)
        private void boardGrid_MouseDown(byte[] turn)
        {
            if (turn != null && turn.Length == 4)
            {
                int prev_X = Convert.ToInt32(turn[0]);
                int prev_Y = Convert.ToInt32(turn[1]);
                int X = Convert.ToInt32(turn[2]);
                int Y = Convert.ToInt32(turn[3]);
                Dispatcher.Invoke(() => board.Move(prev_X, prev_Y, X, Y));
                answer = null;
            }
        }

        internal void StartNewGame(string name1, string name2)
        {
            board = new Board(name1, name2);
            if (selectedPiece != null)
            {
                SelectedSquareBorderReturn(selectedPiece.X - 1, 8 - selectedPiece.Y);
                selectedPiece = null;
            }

            InitNewBoard();
        }

        //Новая игра
        private void Start_btn_Click(object sender, RoutedEventArgs e)
        {
            var nWindow = new NamesWindow { Owner = this };
            nWindow.Background = menuBackground;
            nWindow.ShowDialog();
        }

        //Вызов меню
        private void Menu_btn_Click(object sender, RoutedEventArgs e)
        {
            var menu = new MenuWindow
            {
                Owner = this,
                Background = menuBackground
            };
            menu.SaveButton.IsEnabled = board != null && GameType != GameType.Local;
            menu.ShowDialog();
        }

        //Сохранение
        public void SaveBoard(string saveName)
        {
            //TODO: Попытаться исправить
            //Проблема: при сериализации делегатов, их подписчиками являются методы в других классах (например MainWindow)
            //и для того чтобы сериализовать эти делегаты, дополнительно придется сериализовать все классы в которых
            //определены эти методы, это в корне неправильное решение. Одним из способов решения является пометить делегаты как
            //несериализуемыми, тогда возникает трудность десериализации, т.к. при десериализации все подписчики делегатов теряются.
            //Другое решение проблемы заключается в том чтобы реализовать интерфейс ISerializable. Этот способ слишком трудоёмкий,
            //поэтому было принято решение пойти костыльным путем: отписать все методы которые определены в несериализуемом классе
            //и заново их подписывать. Способ рабочий конкретно в этом проекте но не будет решена при использовании библиотеки Chess
            //в других проектах. Самое адекватное решение: отказ от сериализации.


            if (board != null)
            {
                board.PieceMoveEvent -= RefreshBoardImage;
                board.PieceMoveEvent -= RefreshMoveInfo;
                board.GameEndEvent -= ShowGameEndDialog;
                board.Question -= ShowQuestionDialog;

                var bf = new BinaryFormatter();
                using (FileStream fs = new FileStream($"{saveName}.dat", FileMode.Create))
                {
                    bf.Serialize(fs, board);
                }

                board.PieceMoveEvent += RefreshBoardImage;
                board.PieceMoveEvent += RefreshMoveInfo;
                board.GameEndEvent += ShowGameEndDialog;
                board.Question += ShowQuestionDialog;
            }
        }

        //Загрузка
        public void LoadBoard(string loadName)
        {
            if (File.Exists(loadName))
            {
                var bf = new BinaryFormatter();
                using (FileStream fs = new FileStream(loadName, FileMode.Open))
                {
                    board = (Board)bf.Deserialize(fs);
                }
                if (selectedPiece != null)
                {
                    SelectedSquareBorderReturn(selectedPiece.X - 1, 8 - selectedPiece.Y);
                    selectedPiece = null;
                }
                GameType = GameType.Single;
                InitNewBoard();
            }
        }

        //Отмена хода
        public void TurnBack_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPiece != null)
            {
                if (selectedPiece != null)
                {
                    SelectedSquareBorderReturn(selectedPiece.X - 1, 8 - selectedPiece.Y);
                    selectedPiece = null;
                }
            }
            board.TurnBack();
        }


        internal Server Server { get; set; }
        internal Client Client { get; set; }
        internal byte[] answer;
        internal Chess.Pieces.Color MyColor { get; set; }

        public async void IamServerAsync(string IP,int port, string playerName, object sender)
        {
            Server = new Server(IP,port,playerName);
            await Task.Run(() => Server.InitServer());
            StartNewGame(Server.MyName, Server.OpponentName);
            ((sender as LocalSettings).Parent as NamesWindow).Close();
            MyColor = Chess.Pieces.Color.Black;
            Server.AnsweredEvent += (obj, str) =>
            {
                boardGrid_MouseDown(str);
            };
            Server.Answer += () =>
            {
                answer = new byte[0];
                while(answer.Length == 0)
                {
                    Thread.Sleep(100);
                }
                return answer;
            };
            await Task.Run(() => Server.StartServer());
        }

        public async void IamClientAsync(string IP, int port, string playerName)
        {
            Client = new Client(IP, port, playerName);
            await Task.Run(() => Client.ConnectToServer());
            StartNewGame(Client.OpponentName, Client.MyName);
            MyColor = Chess.Pieces.Color.White;
            Client.AnsweredEvent += (obj, str) =>
            {
                boardGrid_MouseDown(str);
            };
        }
    }
}

        
