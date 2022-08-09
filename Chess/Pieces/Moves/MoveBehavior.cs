using System;
using System.Collections.Generic;

namespace Chess.Pieces.Moves
{
    [Serializable]
    abstract class MoveBehavior
    {
        protected Board board;
        protected Piece piece;
        protected static List<Piece> kingAttackPieces;
        public MoveBehavior(Piece piece)
        {
            this.board = piece.Board;
            this.piece = piece;
            kingAttackPieces = new List<Piece>();
        }

        //Метод движения (проверка общих условий)
        protected bool Move(int X, int Y, bool IsTest = false)
        {
            var tempX = piece.X;
            var tempY = piece.Y;
            var tempPiece = board.GetPiece(X, Y);
            if (tempPiece != null && tempPiece.Color == piece.Color)
            {
                return false;
            }
            piece.ChangeSquare(X, Y);

            var king = board.GetKing(piece.Color);
            void back()
            {
                piece.ChangeSquare(tempX, tempY);
                if (tempPiece != null)
                    board.AnimatePiece(tempPiece.Color, tempPiece.X, tempPiece.Y, tempPiece.Type, tempPiece.IsMoving);
            }
            if (CheckKingSafety(king.X, king.Y))
            {
                back();
                return false;
            }
            if(IsTest)
            {
                back();
            }
            return true;
        }

        //TODO: !!!Осторожно!!! !!!Болото!!! Попытаться исправить.
        #region Протестировано недостаточно!!!
        internal bool CheckKingSafety(int X, int Y)
        {
            if (kingAttackPieces != null)
                kingAttackPieces.Clear();
            else
                kingAttackPieces = new List<Piece>();
            if (CanPawnAttackKing(board.WhoCanTurn == Color.White ? 1 : -1, X, Y) ||
                CanKnightAttackKing(X, Y) ||
                AreKingsTogether()||
                IsAttacked(X, Y, -1, -1) ||
                IsAttacked(X, Y, -1,  0) ||
                IsAttacked(X, Y,  0, -1) ||
                IsAttacked(X, Y,  0,  1) ||
                IsAttacked(X, Y,  1,  0) ||
                IsAttacked(X, Y, -1,  1) ||
                IsAttacked(X, Y,  1, -1) ||
                IsAttacked(X, Y,  1,  1) ) 
                return true;

            return false;
        }
        protected bool IsAttacked(int X, int Y,int signX, int signY)
        {

            for (; X > 0 && X < 9 && Y > 0 && Y < 9; X += signX, Y += signY)
            {
                var piece = board.GetPiece(X + signX,Y + signY);
                if(piece != null)
                {
                    if(piece.Color != board.WhoCanTurn)
                    {
                        if (((signX != 0 && signY != 0 && (piece.Type == PieceType.Bishop ||
                            piece.Type == PieceType.Qween))) ||
                            ((signX == 0 || signY == 0) && (piece.Type == PieceType.Castle ||
                            piece.Type == PieceType.Qween)))
                        {
                            kingAttackPieces.Add(piece);
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }
        protected bool CanPawnAttackKing(int direction, int X, int Y)
        {
            var kingColor = board.WhoCanTurn;
            var pawn1 = board.GetPiece(X + 1, Y + direction);
            var pawn2 = board.GetPiece(X - 1, Y + direction);

            if (pawn1 != null && pawn1.Type == PieceType.Pawn && pawn1.Color != kingColor ||
                pawn2 != null && pawn2.Type == PieceType.Pawn && pawn2.Color != kingColor)
            {
                kingAttackPieces.Add(pawn1 ?? pawn2);
                return true;
            }
            return false;
        }
        protected bool CanKnightAttackKing(int X, int Y)
        {
            bool check(int x, int y, Color color)
            {
                var potentialKnight = board.GetPiece(x, y);
                if (potentialKnight != null && potentialKnight.Type == PieceType.Knight && potentialKnight.Color != color)
                {
                    kingAttackPieces.Add(potentialKnight);
                    return true;
                }
                return false;
            }

            var kingColor = board.WhoCanTurn;

            if(check(X + 2, Y + 1, kingColor) ||
               check(X - 2, Y + 1, kingColor) ||
               check(X + 2, Y - 1, kingColor) ||
               check(X - 2, Y - 1, kingColor) ||
               check(X + 1, Y + 2, kingColor) ||
               check(X + 1, Y - 2, kingColor) ||
               check(X - 1, Y + 2, kingColor) ||
               check(X - 1, Y - 2, kingColor)) 
                return true;

            return false;

        }
        protected bool AreKingsTogether()
        {
            var blackKing = board.GetKing(Color.Black);
            var whiteKing = board.GetKing(Color.White);
            if ((Math.Abs((whiteKing.X - blackKing.X)) == 1 || whiteKing.X - blackKing.X == 0) &&
                (Math.Abs((whiteKing.Y - blackKing.Y)) == 1 || whiteKing.Y - blackKing.Y == 0)) 
                return true;
            return false;
        }
        #endregion

        //Проверяет сможет ли дойти до указанной координаты
        protected bool CheckLine(int X, int Y)
        {
            int signX = GetSign(X,piece.X);
            int signY = GetSign(Y,piece.Y);
            int beginX = piece.X;
            int beginY = piece.Y;

            if ((X != beginX || Y != beginY))
            {
                for (; (beginX < 9) && (beginX > 0) &&
                    (beginY < 9) && (beginY > 0);
                    beginX += signX, beginY += signY)
                {
                    var endPiece = board.GetPiece(beginX, beginY);
                    if (X == beginX && Y == beginY)
                    {
                        return true;
                    }
                    else if(endPiece != null && endPiece != piece)
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        //Получает знак "поиска"
        protected static int GetSign(int first, int second)
        {
            if (first == second)
                return 0;
            return first > second ? 1 : -1;
        }

        //классы-наследники должны реализовать свое поведение используя базовые методы определенные в родительском классе
        public abstract bool TryMove(int X, int Y, bool isTest = false);
    }
}
