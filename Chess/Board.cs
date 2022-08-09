using Chess.Pieces;
using Chess.Pieces.Moves;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Chess
{
    [Serializable]
    public class Board
    {
        //TODO: примерный список функций, которые надо реализовать: Взятие на проходе, Пат. UPD: не нужно реализовывать
        public Piece[,] PiecesArr { get; private set; }
        public const int Size = 8;

        //Получение индекса короля из списка Pieces
        internal Piece GetKing(Color kingColor)
        {
            foreach (var item in PiecesArr)
            {
                if(item != null && item.Color == kingColor && item.Type == PieceType.King)
                {
                    return item;
                }
            }
            return null;
        }

        public bool IsChecked { get; private set; }

        //История партии
        public ObservableCollection<MoveInfo> Turns { get; private set; }
        private List<Piece> prevPawns;
        public string Player1 { get; }
        public string Player2 { get; }

        //Попытка движения
        public bool Move(int BeginX, int BeginY, int EndX, int EndY)
        {
            var piece = GetPiece(BeginX, BeginY);
            if (piece != null && piece.Color == WhoCanTurn)
            {
                var info = piece.TryMove(EndX, EndY);
                if (info.IsMove)
                {
                    PieceMoveEvent(piece, info);
                }
                return info.IsMove;
            }
            return false;
        }

        //[field: NonSerialized]
        //Фигура двинулась
        public event Action<object, MoveInfo> PieceMoveEvent;
        //[field: NonSerialized]
        //Конец игры
        public event Action<GameResult> GameEndEvent;
        //Опрос пользователя по поводу выбора фигуры (если не реализовано, то фигура по умолчанию ферзь)
        //[NonSerialized]
        public Func<Piece, PieceType> Question;

        //Количество ходов
        public int TurnCount { get; private set; }

        //Цвет игрока который может ходить
        public Color WhoCanTurn { get; private set; }

        //Конструктор
        public Board(string player1 = "Player1", string player2 = "Player2")
        {
            PiecesArr = new Piece[Size, Size];
            prevPawns = new List<Piece>();
            Turns = new ObservableCollection<MoveInfo>();
            TurnCount = 0;
            StandardInit();

            WhoCanTurn = Color.White;

            PieceMoveEvent += ChangeParameters;

            Player1 = player1;
            Player2 = player2;
        }

        private void ChangeParameters(object obj, MoveInfo info)
        {
            WhoCanTurn = Color.White == WhoCanTurn ? Color.Black : Color.White;
            Turns.Insert(0, info);
            TurnCount++;
            var king = GetKing(WhoCanTurn);
            IsChecked = (obj as Piece).move.CheckKingSafety(king.X, king.Y);
            if (IsChecked && (king.move as KingMove).CheckMate())
            {
                GameEndEvent(WhoCanTurn == Color.Black ? GameResult.WinWhite : GameResult.WinBlack);
            }
        }

        //Превращение пешки
        internal void Promotion(Piece piece)
        {
            AnimatePiece(piece.Color, piece.X, piece.Y, (piece.move as PawnMove).PromotionType, true);
            prevPawns.Add(PiecesArr[piece.X - 1, piece.Y - 1]);
        }

        //Получение фигуры по координатам
        public Piece GetPiece(int X, int Y)
        {
            if (X < 9 && X > 0 && Y < 9 && Y > 0)
            {
                return PiecesArr[X - 1, Y - 1];
            }
            return null;
        }

        //Добавление фигуры в лист
        internal void AnimatePiece(Color color, int X, int Y, PieceType pieceType, bool isMoving = false)
        {
            PiecesArr[X - 1, Y - 1] = new Piece(this, color, X, Y, pieceType, isMoving);
            if (pieceType == PieceType.Pawn)
            {
                (PiecesArr[X - 1, Y - 1].move as PawnMove).PromotionEvent += Promotion;
            }
        }

        //Удаление фигуры с листа по координатам
        internal void CapturePiece(int X, int Y)
        {
            PiecesArr[X - 1, Y - 1] = null;
        }

        //Стандартная доска
        private void StandardInit()
        {

            //Pawns
            for (int i = 1; i <= 8; i++)
            {
                AnimatePiece(Color.Black, i, 7, PieceType.Pawn);
                AnimatePiece(Color.White, i, 2, PieceType.Pawn);
            }

            //Knights
            for (int i = 2; i <= 7; i += 5)
            {
                AnimatePiece(Color.Black, i, 8, PieceType.Knight);
                AnimatePiece(Color.White, i, 1, PieceType.Knight);
            }

            //Bishops
            for (int i = 3; i <= 6; i += 3)
            {
                AnimatePiece(Color.Black, i, 8, PieceType.Bishop);
                AnimatePiece(Color.White, i, 1, PieceType.Bishop);
            }

            //Castles
            for (int i = 1; i <= 8; i += 7)
            {
                AnimatePiece(Color.Black, i, 8, PieceType.Castle);
                AnimatePiece(Color.White, i, 1, PieceType.Castle);
            }

            AnimatePiece(Color.White, 4, 1, PieceType.Qween);
            AnimatePiece(Color.Black, 4, 8, PieceType.Qween);

            AnimatePiece(Color.White, 5, 1, PieceType.King);
            AnimatePiece(Color.Black, 5, 8, PieceType.King);
        }

        //TODO: остановка
        /// <summary>
        /// Отмена хода
        /// </summary>
        public void TurnBack()
        {
            if (Turns.Count != 0)
            {
                var currentTurn = Turns[0];
                PieceBack(currentTurn);
                TurnCount -= 2;
                Turns.RemoveAt(0);
                PieceMoveEvent(currentTurn.MovedPiece, currentTurn);
                Turns.RemoveAt(0);
            }
        }

        private void PieceBack(MoveInfo info)
        {
            if(info.Type == MoveType.ussually || info.Type == MoveType.capture)
            {
                info.MovedPiece.ChangeSquare(info.BeginX, info.BeginY);
                info.MovedPiece.IsMoving = info.IsMovingPiece;
                if (info.TargetPiece != null)
                {
                    info.TargetPiece.ChangeSquare(info.EndX, info.EndY);
                }
                if(info.MovedPiece.Type != PieceType.Pawn && (info.MovedPiece.move is PawnMove))
                {
                    info.MovedPiece.Type = PieceType.Pawn;
                }
            }
            else
            {
                var line = info.MovedPiece.Color == Color.Black ? 8 : 1;
                var castle = info.MovedPiece.X == 7 ? PiecesArr[5, line - 1] : PiecesArr[3, line - 1];

                castle.ChangeSquare(castle.X == 6 ? 8 : 1, line);
                castle.IsMoving = false;

                info.MovedPiece.ChangeSquare(info.BeginX, info.BeginY);
                info.MovedPiece.IsMoving = false;
            }
        }
    }

    [Serializable]
    public enum GameResult
    {
        WinWhite,
        WinBlack,
        Draw
    }
}
