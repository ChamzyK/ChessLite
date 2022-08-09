using Chess.Pieces.Moves;
using System;

namespace Chess.Pieces
{
    [Serializable]
    public class Piece
    {
        //Цвет
        public Color Color { get; }

        //Доска
        internal Board Board { get; }

        //Координаты
        public int X { get; private set; }
        public int Y { get; private set; }

        internal void ChangeSquare(int x, int y)
        {
            Board.PiecesArr[X - 1, Y - 1] = null;
            X = x;
            Y = y;
            Board.PiecesArr[X - 1, Y - 1] = this;
        }

        //Способ движения
        internal readonly MoveBehavior move;

        //Попытка движения
        internal MoveInfo TryMove(int x, int y)
        {
            //TODO: Переосмыслить
            var capturedPiece = Board.GetPiece(x, y);
            var prevX = X;
            var prevY = Y;
            var isMove = move.TryMove(x, y);
            var isMoving = IsMoving;
            MoveType tempMoveType = MoveType.ussually;
            if(isMove)
            {
                IsMoving = true;
            }

            if (Type == PieceType.King && Math.Abs(prevX - X) == 2)
            {
                tempMoveType = MoveType.castling;
            }
            else if (capturedPiece != null)
            {
                tempMoveType = MoveType.capture;
            }

            var turnNumber = Board.TurnCount;
            return new MoveInfo(this, capturedPiece, isMove, prevX, prevY, x, y, turnNumber, tempMoveType, Board.IsChecked, isMoving);
        }

        //Тип
        public PieceType Type { get; set; }

        //Двигался ли
        internal bool IsMoving { get; set; }

        //Конструктор
        internal Piece(Board board, Color color, int x, int y, PieceType type, bool isMoving = false)
        {
            Board = board;
            Color = color;
            X = x;
            Y = y;
            Type = type;
            IsMoving = isMoving;
            move = GetMoveBehaviorFromPieceType(type);

            if (type == PieceType.Pawn)
            {
                (move as PawnMove).PromotionEvent += (piece) =>
                {
                    Type = (move as PawnMove).PromotionType;
                };
            }
        }
        private MoveBehavior GetMoveBehaviorFromPieceType(PieceType pieceType)
        {
            switch (pieceType)
            {
                case PieceType.Bishop:
                    return new BishopMove(this);
                case PieceType.Castle:
                    return new CastleMove(this);
                case PieceType.Qween:
                    return new QweenMove(this);
                case PieceType.Pawn:
                    return new PawnMove(this);
                case PieceType.Knight:
                    return new KnightMove(this);
                case PieceType.King:
                    return new KingMove(this);
            }
            throw new Exception("Такого типа фигуры не существует!!!" + pieceType);
        }
    }

    [Serializable]
    public enum Color
    {
        White,
        Black
    }

    [Serializable]
    public enum PieceType
    {
        Pawn,
        Castle,
        Knight,
        Bishop,
        Qween,
        King
    }
}
