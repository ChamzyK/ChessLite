using System;

namespace Chess.Pieces.Moves
{
    [Serializable]
    class CastleMove : MoveBehavior
    {
        public CastleMove(Piece piece) : base(piece)
        {
        }

        public override bool TryMove(int X, int Y, bool isTest = false)
        {
            if ((piece.X == X || piece.Y == Y) && CheckLine(X, Y))
            {
                return Move( X, Y, isTest);
            }
            return false;
        }
    }
}
