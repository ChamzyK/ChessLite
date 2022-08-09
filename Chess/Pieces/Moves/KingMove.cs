using System;
using System.Collections.Generic;

namespace Chess.Pieces.Moves
{
    [Serializable]
    class KingMove : MoveBehavior
    {
        public KingMove(Piece piece) : base(piece)
        {
        }

        private Piece attackedPiece; //Фигура которая атакует короля (ставит шах)

        public override bool TryMove(int X, int Y, bool isTest = false)
        {
            if (X > 0 && X < 9 && Y > 0 && Y < 9)
            {
                //Находится ли X и Y на 1 шаг от короля (проверка спец. условий)
                int tempX = Math.Abs(piece.X - X); //TODO: написать свой метод возвращения абсолютного значения
                int tempY = Math.Abs(piece.Y - Y);
                if ((tempX == 1 || tempX == 0) &&
                    (tempY == 1 || tempY == 0))
                {
                    return Move(X, Y, isTest); //проверка общих условий
                }
                //Проверка на возможность рокирровки
                else if(!piece.IsMoving && !board.IsChecked && piece.Y == Y && Math.Abs(piece.X - X) == 2)
                {
                    //Проверка ладьи
                    var castle = GetCastlingCastle(X,Y);
                    if(castle != null && !castle.IsMoving)
                    {
                        return TryCastling(castle);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Попытка рокировки
        /// </summary>
        /// <param name="castle"></param>
        /// <returns></returns>
        private bool TryCastling(Piece castle)
        {
            int sign = GetCastlingSign(castle);

            //Проверка на наличие фигур по пути
            if(CheckCastlingLine(sign))
            {
                //Проверка на наличие шаха на клетках рокировки
               if(!CheckKingSafety(piece.X + sign,piece.Y) && !CheckKingSafety(piece.X + sign + sign, piece.Y))
                {
                    //Рокировка
                    piece.ChangeSquare(piece.X + sign + sign, piece.Y);
                    castle.ChangeSquare(piece.X - sign, castle.Y);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Стоит ли фигура на пути рокировки
        /// </summary>
        /// <param name="sign"></param>
        /// <returns></returns>
        private bool CheckCastlingLine(int sign)
        {
            int X = sign == 1 ? 7 : 2;

            for (int beginX = piece.X + sign; (beginX < 9) && (beginX > 0); beginX += sign)
            {
                var endPiece = board.GetPiece(beginX, piece.Y);
                if (X == beginX)
                {
                    return true;
                }
                else if(endPiece != null)
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Получение ладьи с которой необходимо совершить рокировку
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        private Piece GetCastlingCastle(int X, int Y)
        {
            return board.GetPiece(X == 3 ? 1 : 8, Y);
        }

        private int GetCastlingSign(Piece castle)
        {
            return castle.X < piece.X ? -1 : 1;
        }

        #region Недостаточно протестировано!!!
        /// <summary>
        /// Проверка на мат
        /// </summary>
        /// <returns></returns>
        public bool CheckMate()
        {
            CheckKingSafety(piece.X, piece.Y); //обновление списка фигур которые атакуют короля
            if (kingAttackPieces.Count != 0)
            {
                //Если количество атакующих фигур больше одного, то это мат. Дальше проверять смысла нет
                if (kingAttackPieces.Count != 1)
                {
                    return true;
                }

                attackedPiece = kingAttackPieces[0]; //"фиксация элемента" потому что в дальнейшем статический список будет менятся

                //Если король все еще может двигаться или можно сбить атаку, то это не мат
                if (CanKingMove() || CanCaptureAttackingPiece())
                {
                    return false;
                }

                //Если атакующая фигура конь или пешка, то это мат, т.к. по предыдущему пункту видно что убрать эту фигуру не можем
                if (attackedPiece.Type == PieceType.Knight || attackedPiece.Type == PieceType.Pawn)
                {
                    return true;
                }

                //Если можно перекрыть путь атаки, то это не мат
                if (CanPiecesMove())
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Может ли король двигаться
        /// </summary>
        /// <returns></returns>
        private bool CanKingMove()
        {
            //Проверяем может ли король ходить (перебираем все возможные варианты)
            return TryMove(piece.X + 1, piece.Y + 1, true) ||
                   TryMove(piece.X + 1, piece.Y - 1, true) ||
                   TryMove(piece.X - 1, piece.Y + 1, true) ||
                   TryMove(piece.X - 1, piece.Y - 1, true) ||
                   TryMove(piece.X + 1, piece.Y    , true) ||
                   TryMove(piece.X    , piece.Y + 1, true) ||
                   TryMove(piece.X - 1, piece.Y    , true) ||
                   TryMove(piece.X    , piece.Y - 1, true);
        }

        /// <summary>
        /// Можно ли убрать атакующую фигуру
        /// </summary>
        /// <returns></returns>
        private bool CanCaptureAttackingPiece()
        {
            int x = attackedPiece.X;
            int y = attackedPiece.Y;
            //Проверяем может ли какая-нибудь фигура сходить на клетку где стоит атакующая фигура
            foreach (var item in board.PiecesArr)
            {
                if(item != null && item.Color == piece.Color)
                {
                    if (item.move.TryMove(x,y,true))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Получение пути атаки
        /// </summary>
        /// <returns></returns>
        private List<(int,int)> GetAttackTrajectory()
        {
            var result = new List<(int, int)>(); //Лист для записи координат пути атаки
            int signX = GetSign(attackedPiece.X, piece.X); //получаем знак X
            int signY = GetSign(attackedPiece.Y, piece.Y); //получаем знак Y
            var attackedPieceX = attackedPiece.X - signX; //Первый раз присваиваем чтобы исключить вариант местонахождения самой фигуры
            var attackedPieceY = attackedPiece.Y - signY;

            //движемся от фигуры, которая атакует, до короля чтобы записать путь атаки
            for (; attackedPieceX != piece.X && attackedPieceY != piece.Y;
                    attackedPieceX -= signX, attackedPieceY -= signY)
            {
                result.Add((attackedPieceX, attackedPieceY));
            }
            return result;
        }

        /// <summary>
        /// Могут ли фигуры цвета короля перекрыть путь атаки
        /// </summary>
        /// <returns></returns>
        private bool CanPiecesMove()
        {
            var attackList = GetAttackTrajectory();

            //Проверяем может ли какая нибудь фигура перекрыть путь атаки
            foreach (var item in board.PiecesArr)
            {
                if (item != null && item.Color == piece.Color)
                {
                    for (int i = 0; i < attackList.Count; i++)
                    {
                        if (item.move.TryMove(attackList[i].Item1, attackList[i].Item2,true))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
