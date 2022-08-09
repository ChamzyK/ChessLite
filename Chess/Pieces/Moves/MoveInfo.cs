using System;

namespace Chess.Pieces.Moves
{
    [Serializable]
    public class MoveInfo
    {
        public bool IsMove { get; } //Результат движения
        public int TurnNumber { get; } //Номер хода
        public Color WhoseTurned { get; } //Цвет игрока который ходил
        public Piece MovedPiece { get; } //Фигура которая ходила
        public Piece TargetPiece { get; } //Фигура которая была выбита
        public MoveType Type { get; }
        public int BeginX { get; }
        public int BeginY { get; }
        public int EndX { get; }
        public int EndY { get; }
        public bool IsChecked { get; }
        public bool IsMovingPiece { get; }

        public MoveInfo(Piece movedPiece, Piece targetPiece, bool isMove, int beginX, int beginY, int endX, int endY, int turnNumber, MoveType type, bool isChecked = false, bool isMovingPiece = true)
        {
            MovedPiece = movedPiece;
            WhoseTurned = movedPiece.Color;
            IsMovingPiece = isMovingPiece;
            TargetPiece = targetPiece;
            IsMove = isMove;
            BeginX = beginX;
            BeginY = beginY;
            EndX = endX;
            EndY = endY;
            TurnNumber = turnNumber;
            Type = type;
            IsChecked = isChecked;
        }
        public override string ToString()
        {
            return $"{GetSymbolY(BeginX)}{BeginY} - {GetSymbolY(EndX)}{EndY}";
        }

        private char GetSymbolY(int X)
        {
            return (char)('a' + X - 1);
        }

    }
}
