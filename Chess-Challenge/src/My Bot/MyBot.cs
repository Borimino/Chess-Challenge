using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    // Bonus for a Check
    int checkBonus = 100;
    // Bonus for a Capture
    int captureBonus = 10;
    // Bonus for a CheckMate
    int checkMateBonus = 1000000;
    // Mallus for a Draw
    int drawMallus = 1000;


    // Start depth
    int startDepth = 5;

    // Randomness
    Random rng = new Random();

    public Move Think(Board board, Timer timer)
    {
        int numberOfEnemyPieces = board.GetAllPieceLists().Where((piece) => piece.IsWhitePieceList != board.IsWhiteToMove).Select(piece => piece.Count).Sum();
        int depth = startDepth;
        if (numberOfEnemyPieces < 3)
        {
            depth += 2;
        }
        System.Console.WriteLine("Starting depth: " + depth);
        ScoredMove scoredMove = PickBestMove(board, depth, int.MinValue + 10, int.MaxValue - 10, Score(board));
        System.Console.WriteLine("Score for this move was " +  scoredMove.score);
        System.Console.WriteLine("This is an improvement of " + (scoredMove.score - Score(board)));
        return scoredMove.move;
    }

    private ScoredMove PickBestMove(Board board, int depth, int myMinimumAssuredScore, int opponentsMaximumAssuredScore, int baseScore)
    {
        List<Move> allMoves = board.GetLegalMoves().ToList<Move>();
        List<Move> capturingMoves = allMoves.Where((move) => move.IsCapture).ToList<Move>();
        List<Move> nonCapturingMoves = allMoves.Where((move) =>  !move.IsCapture).ToList<Move>();
        List<Move> moves = capturingMoves.Concat(nonCapturingMoves).ToList<Move>();

        if (moves.Count == 0)
        {
            return new ScoredMove(new Move(), -checkMateBonus);
        }

        ScoredMove bestMove = new ScoredMove(moves[0], int.MinValue);
        List<ScoredMove> bestMoves = new List<ScoredMove> { bestMove };
        foreach (Move move in moves)
        {
            int score;
            board.MakeMove(move);
            if (depth == 0) // If we are stopping the search, return the score of the player, who just made a move
            {
                int tmpScore = -Score(board);
                score = tmpScore;
            }
            else
            {
                // Get the score of the best move our opponent could make, and return -1 * that
                score = -PickBestMove(board, depth - 1, -opponentsMaximumAssuredScore, -myMinimumAssuredScore, baseScore).score;
            }
            if (move.IsCapture)
            {
                score += captureBonus;
            }
            if (board.IsInCheck())
            {
                score += checkBonus;
            }
            if (board.IsInCheckmate())
            {
                score += checkMateBonus;
            }
            if (board.IsDraw())
            {
                score -= drawMallus;
            }
            board.UndoMove(move);
            /*if (score == bestMove.score)
            {
                bestMoves.Add(new ScoredMove(move, score));
            }*/
            if (score > bestMove.score)
            {
                bestMove = new ScoredMove(move, score);
                bestMoves = new List<ScoredMove> { bestMove };
            }
            if (score > myMinimumAssuredScore)
            {
                myMinimumAssuredScore = score;
            }
            if (opponentsMaximumAssuredScore <= myMinimumAssuredScore)
            {
                return bestMove;
            }
        }

        return bestMoves[rng.Next(bestMoves.Count)];
    }

    // Goes through each of the Piece Lists,
    // if it is the same color as whoever is about to move, add the value * count to the score,
    // if it is the other color, subtract the value * count from the score
    private int Score(Board board)
    {
        int score = 0;
        PieceList[] pieces = board.GetAllPieceLists();
        foreach(PieceList piece in pieces)
        {
            int sign = piece.IsWhitePieceList ? (board.IsWhiteToMove ? 1 : -1) : (board.IsWhiteToMove ? -1 : 1);
            score += sign * pieceValues[(int) piece.TypeOfPieceInList] * piece.Count;
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