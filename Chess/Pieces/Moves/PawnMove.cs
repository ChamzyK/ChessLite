using System;

namespace Chess.Pieces.Moves
{
    [Serializable]
    class PawnMove : MoveBehavior
    {
        public PawnMove(Piece piece) : base(piece)
        {
            direction = piece.Color == Color.White ? 1 : -1;
        }

        //TODO: надо ли реализовывать взятие на проходе? нет
        public override bool TryMove(int X, int Y, bool isTest = false)
        {
            if (piece.X == X)
            {

                if ((((piece.Y + direction) == Y) || 
                    (!piece.IsMoving && ((piece.Y + direction + direction) == Y) && 
                    (board.GetPiece(X, piece.Y + direction) == null))) &&
                    (board.GetPiece(X, Y) == null))
                {
                    var result = Move(X,Y, isTest);
                    if (result && (piece.Y == 8 || piece.Y == 1))
                    {
                        if (board.Question != null)
                            promotionType = board.Question(piece);
                        PromotionEvent(piece);
                    }
                    return result;
                }
            }
            else if (Math.Abs(piece.X - X) == 1 && ((piece.Y + direction) == Y))
            {
                var tempPiece = board.GetPiece(X, Y);
                if (tempPiece != null && tempPiece.Color != piece.Color)
                {
                    var result = Move(X, Y, isTest);
                    if (result && (piece.Y == 8 || piece.Y == 1))
                    {
                        if (board.Question != null)
                            promotionType = board.Question(piece);
                        PromotionEvent(piece);
                    }
                    return result;
                }
            }
            return false;       
        }

        private readonly int direction;

        public event Action<Piece> PromotionEvent;
        private PieceType promotionType = PieceType.Qween;
        public PieceType PromotionType 
        { 
            get
            {
                return promotionType;
            }
            set
            {
                if(value != PieceType.King && value != PieceType.Pawn)
                {
                    promotionType = value;
                }
            }
        }
    }
}
