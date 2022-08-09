using System;

namespace Chess.Pieces.Moves
{
    [Serializable]
    class KnightMove : MoveBehavior
    {
        public KnightMove(Piece piece) : base(piece)
        {
        }

        public override bool TryMove(int X, int Y, bool isTest = false)
        {
            if((Math.Abs(piece.X - X) == 2 && Math.Abs(piece.Y - Y) == 1) || 
                (Math.Abs(piece.X - X) == 1 && Math.Abs(piece.Y - Y) == 2))
            {
                return Move(X,Y, isTest);
            }

            return false;
        }
    }
}
