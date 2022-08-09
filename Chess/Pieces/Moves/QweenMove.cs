using System;

namespace Chess.Pieces.Moves
{
    [Serializable]
    class QweenMove : MoveBehavior
    {
        public QweenMove(Piece piece) : base(piece)
        {
        }

        public override bool TryMove(int X, int Y, bool isTest = false)
        {
            if(CheckLine(X,Y))
                return Move(X, Y, isTest);
            return false;
        }
    }
}
