using API.Interface;
using API.Model;
using API.Model.Response;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace API.Service
{
    public class ChessService : IChessService
    {
        private ChessOption _option;
        private List<NextStepResponse> steps;
        private int beginVal = 0;

        public ChessService(IOptions<ChessOption> option)
        {
            _option = option.Value;
            steps = new List<NextStepResponse>();
        }

        public NextStepResponse GetNextStep(ChessParam param)
        {
            beginVal = ValBoard(param.Pieces);
            var result = new NextStepResponse();
            Minimax(_option.Depth, param.Pieces, 0, -1000, 1000, param.IsMaxmizer, ref result);
            return result;
        }

        private List<List<Piece>> NewBoard(List<List<Piece>> board)
        {
            List<List<Piece>> newBoard = new List<List<Piece>>();
            for(int i = 0; i < board.Count; i++)
            {
                newBoard.Add(new List<Piece>());
                foreach(var piece in board[i])
                {
                    var newPiece = new Piece()
                    {
                        Chess = piece.Chess,
                        IsMoved = piece.IsMoved,
                        Value = piece.Value,
                    };
                    newBoard[i].Add(piece);
                }
            }
            return newBoard;
        }

        private int ValBoard(List<List<Piece>> board)
        {
            int res = 0;
            for (int i = 0; i < board.Count; i++)
            {
                foreach (var piece in board[i])
                {
                    res += piece.Value;
                }
            }
            return res;
        }
        #region Minimax
        private int Minimax(int depth, List<List<Piece>> board, int value, int alpha, int beta, bool isMax, ref NextStepResponse result)
        {
            if(depth == 0 || CheckEnd(board)){
                return value;
            }
            int bestVal = isMax ? -1000 : 1000;
            for (int i = 0; i < _option.Size; i++)
            {
                for (int j = 0; j < _option.Size; j++)
                {
                    var piece = board[i][j];
                    if(piece.Chess != Enum.ChessEnum.None && piece.Value * (isMax ? 1 : -1) > 0)
                    {
                        var nextSteps = NextStep(i, j, board);
                        foreach(Step step in nextSteps)
                        {
                            var val = board[step.X][step.Y].Value;
                            var boardTmp = NewBoard(board);
                            var newPiece = new Piece()
                            {
                                Chess = piece.Chess,
                                IsMoved = piece.IsMoved,
                                Value = piece.Value,
                            };
                            boardTmp[step.X][step.Y] = newPiece;
                            boardTmp[i][j] = new Piece();
                            var value1 = Minimax(depth - 1, boardTmp, value - val, alpha, beta, !isMax, ref result);
                            bestVal = isMax ? Math.Max(bestVal, value1) : Math.Min(bestVal, value1);
                            if (isMax && alpha <= bestVal)
                            {
                                
                                
                                if (depth == _option.Depth)
                                {
                                    if (alpha < bestVal)
                                    {
                                        steps = new List<NextStepResponse>();
                                    }
                                    steps.Add(new NextStepResponse()
                                        {
                                            CurrentStep = new Step(i, j),
                                            NextStep = step,
                                        });

                                }
                                alpha = bestVal;
                            }
                            else if(!isMax && bestVal < beta)
                            {
                                
                                
                                if (depth == _option.Depth)
                                {
                                    if (beta > bestVal)
                                    {
                                        steps = new List<NextStepResponse>();
                                    }
                                    steps.Add(new NextStepResponse()
                                        {
                                            CurrentStep = new Step(i, j),
                                            NextStep = step,
                                        });

                                }
                                beta = bestVal;
                            }
                            if (beta <= alpha)
                            {

                                return bestVal;
                            }
                        }
                    }
                }
            }
            if ((result == null || result.CurrentStep == null) && depth == _option.Depth)
            {
                Random rng = new Random();
                result = steps[rng.Next(steps.Count)];
            }
            return bestVal;
        }

        private bool CheckEnd(List<List<Piece>> board)
        {
            int count = 0;
            for (int i = 0; i < _option.Size; i++)
            {
                for (int j = 0; j < _option.Size; j++)
                {
                    if (board[i][j].Chess == Enum.ChessEnum.King)
                    {
                        count++;
                        if(count > 1)   return false;
                    }
                }
            }
            return true;
        }

        #endregion
        #region di chuyển
        public bool Check(int x, int y)
        {
            return (x < _option.Size && y < _option.Size && x >= 0 && y >= 0);
        }
        private List<Step> NextStep(int x, int y, List<List<Piece>> board)
        {
            Piece piece = board[x][y];
            var result = new List<Step>();
            switch (piece.Chess)
            {
                case Enum.ChessEnum.Pawn:
                    result = NextStepPawn(x, y, board);
                    break;
                case Enum.ChessEnum.Rook:
                    result = NextStepRook(x, y, board);
                    break;
                case Enum.ChessEnum.Bishop:
                    result = NextStepBishop(x, y, board);
                    break;
                case Enum.ChessEnum.Knight:
                    result = NextStepKnight(x, y, board);
                    break;
                case Enum.ChessEnum.Queen:
                    result = NextStepQueen(x, y, board);
                    break;
                case Enum.ChessEnum.King:
                    result = NextStepKing(x, y, board);
                    break;
            }
            return result;
        }
        /// <summary>
        /// Các nước tiếp theo quân tốt có thể đi
        /// </summary>
        /// <param name="x">vị trí quân cờ hiện tại</param>
        /// <param name="y">vị trí quân cờ hiện tại</param>
        /// <param name="board">bàn cờ</param>
        /// <returns></returns>
        private List<Step> NextStepPawn(int x, int y, List<List<Piece>> board)
        {
            var result = new List<Step>();
            var tmp = board[x][y].Value > 0 ? 1 : -1;
            if (x + tmp >= 0 && x + tmp < _option.Size && board[x + tmp][y].Chess == Enum.ChessEnum.None)
            {
                result.Add(new Step(x + tmp, y));
                if (!board[x][y].IsMoved && x + 2*tmp >= 0 && x + 2 * tmp < _option.Size && board[x + 2 * tmp][y].Chess == Enum.ChessEnum.None)
                {
                    result.Add(new Step(x + 2 * tmp, y));
                }
            }
            if (Check(x+tmp, y+tmp) && board[x + tmp][y + tmp].Value * board[x][y].Value < 0)
            {
                result.Add(new Step(x + tmp, y + tmp));
            }
            if (Check(x + tmp, y - tmp) && board[x + tmp][y - tmp].Value * board[x][y].Value < 0)
            {
                result.Add(new Step(x + tmp, y - tmp));
            }
            return result;
        }
        /// <summary>
        /// Các nước tiếp theo quân xe có thể đi
        /// </summary>
        /// <param name="x">vị trí quân cờ hiện tại</param>
        /// <param name="y">vị trí quân cờ hiện tại</param>
        /// <param name="board">bàn cờ</param>
        /// <returns></returns>
        private List<Step> NextStepRook(int x, int y, List<List<Piece>> board)
        {
            var result = new List<Step>();
            int[] a = { -1, 1, 0, 0 };
            int[] b = { 0, 0, -1, 1 };
            int p;
            int q;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; ; j++)
                {
                    p = x + a[i] * j;
                    q = y + b[i] * j;
                    if (Check(p, q))
                    {
                        if (board[p][q].Value == 0)
                        {
                            result.Add(new Step(p, q));
                        }
                        else 
                        {
                            if (board[p][q].Value * board[x][y].Value < 0)
                                result.Add(new Step(p, q));
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Các nước tiếp theo quân tượng có thể đi
        /// </summary>
        /// <param name="x">vị trí quân cờ hiện tại</param>
        /// <param name="y">vị trí quân cờ hiện tại</param>
        /// <param name="board">bàn cờ</param>
        /// <returns></returns>
        private List<Step> NextStepBishop(int x, int y, List<List<Piece>> board)
        {
            var result = new List<Step>();
            int[] a = { -1, -1, 1, 1 };
            int[] b = { -1, 1, -1, 1 };
            int p;
            int q;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; ; j++)
                {
                    p = x + a[i] * j;
                    q = y + b[i] * j;
                    if (Check(p, q))
                    {
                        if (board[p][q].Value == 0)
                        {
                            result.Add(new Step(p,q));
                        }
                        else 
                        {
                            if (board[p][q].Value * board[x][y].Value < 0)
                                result.Add(new Step(p, q));
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Các nước tiếp theo quân mã có thể đi
        /// </summary>
        /// <param name="x">vị trí quân cờ hiện tại</param>
        /// <param name="y">vị trí quân cờ hiện tại</param>
        /// <param name="board">bàn cờ</param>
        /// <returns></returns>
        private List<Step> NextStepKnight(int x, int y, List<List<Piece>> board)
        {
            var result = new List<Step>();
            int[] a = { -2, -2, -1, -1, 1, 1, 2, 2 };
            int[] b = { -1, 1, -2, 2, -2, 2, -1, 1 };
            int p;
            int q;
            for (int i = 0; i < 8; i++)
            {
                p = x + a[i];
                q = y + b[i];
                if (Check(p, q) && (board[p][q].Value == 0 || board[p][q].Value * board[x][y].Value < 0))
                {
                    result.Add(new Step(p, q));
                }

            }
            return result;
        }
        /// <summary>
        /// Các nước tiếp theo quân hậu có thể đi
        /// </summary>
        /// <param name="x">vị trí quân cờ hiện tại</param>
        /// <param name="y">vị trí quân cờ hiện tại</param>
        /// <param name="board">bàn cờ</param>
        /// <returns></returns>
        private List<Step> NextStepQueen(int x, int y, List<List<Piece>> board)
        {
            var result = new List<Step>();
            int[] a = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] b = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int p;
            int q;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 1; ; j++)
                {
                    p = x + a[i] * j;
                    q = y + b[i] * j;
                    if (Check(p, q))
                    {
                        if (board[p][q].Value == 0)
                        {
                            result.Add(new Step(p, q));
                        }
                        else 
                        {
                            if (board[p][q].Value * board[x][y].Value < 0)
                                result.Add(new Step(p, q));
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Các nước tiếp theo quân vua có thể đi
        /// </summary>
        /// <param name="x">vị trí quân cờ hiện tại</param>
        /// <param name="y">vị trí quân cờ hiện tại</param>
        /// <param name="board">bàn cờ</param>
        /// <returns></returns>
        private List<Step> NextStepKing(int x, int y, List<List<Piece>> board)
        {
            var result = new List<Step>();
            int[] a = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] b = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int p;
            int q;
            for (int i = 0; i < 8; i++)
            {
                p = x + a[i];
                q = y + b[i];
                if (Check(p, q))
                {
                    if (board[p][q].Value == 0 || board[p][q].Value * board[x][y].Value < 0)
                    {
                        result.Add(new Step(p, q));
                    }
                }
            }
            //if (CheckArtificalCastling(board, x, y, 1))
            //{
            //    result.Add(new Step(x, y + 2));
            //}
            //if (CheckArtificalCastling(board, x, y, -1))
            //{
            //    result.Add(new Step(x, y - 2));
            //}
            return result;
        }
        /// <summary>
        /// Nhập thành
        /// </summary>
        /// <param name="board"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="increate"></param>
        /// <returns></returns>
        public bool CheckArtificalCastling(List<List<Piece>> board, int x, int y, int increate)
        {
            if (board[x][y].IsMoved == true)
            {
                return false;
            }
            int i;
            for (i = increate; y + i > 0 && y + i < _option.Size - 1; i += increate)
            {
                if (board[x][y+i].Value != 0)
                {
                    return false;
                }
            }
            return board[x][y + i].Value == Constant.Constant.Rook && board[x][y + i].IsMoved == false;
        }
        #endregion
    }
}
