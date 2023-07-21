using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    bool goodMoves = false;

    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    // Bonus for a Check
    int checkBonus = 20000;
    // Bonus for a CheckMate
    int checkMateBonus = 100000;
    // Start depth
    int startDepth = 3;

    // Randomness
    Random rng = new Random();

    public Move Think(Board board, Timer timer)
    {
        ScoredMove scoredMove = PickBestMove(board, startDepth);
        //System.Console.WriteLine("Score for this move was " +  scoredMove.score);
        return scoredMove.move;
    }

    private ScoredMove PickBestMove(Board board, int depth)
    {
        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0)
        {
            //System.Console.WriteLine("No more moves to make");
            return new ScoredMove(new Move(), int.MinValue);
        }

        ScoredMove bestMove = new ScoredMove(moves[0], goodMoves ? int.MinValue : int.MaxValue);
        List<ScoredMove> bestMoves = new List<ScoredMove> { bestMove };
        foreach (Move move in moves)
        {
            int score;
            board.MakeMove(move);
            if (depth == 0)
            {
                int tmpScore = Score(board);
                /*if (tmpScore != 0)
                {
                    System.Console.WriteLine("Score at depth 0: " + tmpScore);
                }*/
                score = tmpScore;
            }
            else
            {
                score = -1 * PickBestMove(board, depth - 1).score;
            }
            if (board.IsInCheck())
            {
                score += goodMoves ? checkBonus : -1 * checkBonus;
            }
            if (board.IsInCheckmate())
            {
                score += goodMoves ? checkMateBonus : -1 * checkMateBonus;
            }
            board.UndoMove(move);
            if (score == bestMove.score)
            {
                bestMoves.Add(new ScoredMove(move, score));
            }
            if (goodMoves ? score > bestMove.score : score < bestMove.score)
            {
                bestMove = new ScoredMove(move, score);
                bestMoves = new List<ScoredMove> { bestMove };
            }
        }

        /*if (bestMove.score != 0 && depth != 0)
        {
            System.Console.WriteLine("Score at depth " + depth + ": " + bestMove.score);
        }*/

        return bestMoves[rng.Next(bestMoves.Count)];
    }

    private int Score(Board board)
    {
        int score = 0;
        PieceList[] pieces = board.GetAllPieceLists();
        foreach(PieceList piece in pieces)
        {
            int multiplier = piece.IsWhitePieceList ? (board.IsWhiteToMove ? 1 : -1) : (board.IsWhiteToMove ? -1 : 1);
            score += multiplier * pieceValues[(int) piece.TypeOfPieceInList] * piece.Count;
        }
        return score;
    }

    private class ScoredMove
    {
        public Move move;
        public int score;

        public ScoredMove(Move move, int score)
        {
            this.move = move;
            this.score = score;
        }
    }
}