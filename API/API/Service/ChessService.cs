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

        public ChessService(IOptions<ChessOption> option)
        {
            _option = option.Value;
        }

        public NextStepResponse GetNextStep(ChessParam param)
        {
            var result = new NextStepResponse();
            Minimax(_option.Depth, param.Pieces, 0, -1000, 1000, param.IsMaxmizer, ref result);
            return result;
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
                            var boardTmp = board;
                            boardTmp[step.X][step.Y] = board[i][j];
                            boardTmp[i][j] = new Piece();
                            var value1 = Minimax(depth - 1, boardTmp, value + val, alpha, beta, !isMax, ref result);
                            bestVal = isMax ? Math.Max(bestVal, value1) : Math.Min(bestVal, value1);
                            if (isMax)
                            {
                                alpha = Math.Max(alpha, bestVal);
                            }
                            else
                            {
                                beta = Math.Min(beta, bestVal);
                            }
                            if(beta < alpha)
                            {
                                if(depth == _option.Depth)
                                {
                                    result = new NextStepResponse()
                                    {
                                        CurrentStep = new Step(i, j),
                                        NextStep = step,
                                    };

                                }
                                return bestVal;
                            }
                        }
                    }
                }
            }
            return bestVal;
        }

        private bool CheckEnd(List<List<Piece>> board)
        {
            for (int i = 0; i < _option.Size; i++)
            {
                for (int j = 0; j < _option.Size; j++)
                {
                    if (board[i][j].Value == Constant.Constant.King)
                    {
                        return false;
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
                    NextStepPawn(x, y, board);
                    break;
                case Enum.ChessEnum.Rook:
                    NextStepRook(x, y, board);
                    break;
                case Enum.ChessEnum.Bishop:
                    NextStepBishop(x, y, board);
                    break;
                case Enum.ChessEnum.Knight:
                    NextStepKnight(x, y, board);
                    break;
                case Enum.ChessEnum.Queen:
                    NextStepQueen(x, y, board);
                    break;
                case Enum.ChessEnum.King:
                    NextStepKing(x, y, board);
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
            if (x + tmp > _option.Size && board[x + tmp][y].Chess == Enum.ChessEnum.None)
            {
                result.Add(new Step(x + tmp, y));
                if (board[x][y].IsMoved && x + 2 * tmp > _option.Size && board[x + 2 * tmp][y].Chess == Enum.ChessEnum.None)
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
