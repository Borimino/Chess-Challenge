using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        // BOT START
        // DECENT VERSION OF MY BOT
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
        // Bonus for a Check
        int checkBonus = 100;
        // Bonus for a CheckMate
        int checkMateBonus = 1000000;
        // Start depth
        int startDepth = 3;

        // Randomness
        Random rng = new Random();

        public Move Think(Board board, Timer timer)
        {
            //int numberOfPieces = board.GetAllPieceLists().Aggregate(0, (acc, piece) => acc + piece.Count);
            int numberOfEnemyPieces = board.GetAllPieceLists().Where((piece) => piece.IsWhitePieceList != board.IsWhiteToMove).Select(piece => piece.Count).Sum();
            int depth = startDepth;
            if (numberOfEnemyPieces < 3)
            {
                depth += 2;
            }
            /*
            int depth = startDepth;
            if (numberOfPieces < 16)
            {
                depth += 1;
            }
            if (numberOfPieces < 8)
            {
                depth += 1;
            }
            */
            //System.Console.WriteLine("Starting depth: " + depth);
            ScoredMove scoredMove = PickBestMove(board, depth, int.MinValue, int.MaxValue);
            //System.Console.WriteLine("Score for this move was " +  scoredMove.score);
            return scoredMove.move;
        }

        private ScoredMove PickBestMove(Board board, int depth, int alpha, int beta)
        {
            Move[] moves = board.GetLegalMoves();

            if (moves.Length == 0)
            {
                //System.Console.WriteLine("No more moves to make at position " + board.GetFenString());
                return new ScoredMove(new Move(), -1 * checkMateBonus);
            }

            ScoredMove bestMove = new ScoredMove(moves[0], int.MinValue);
            List<ScoredMove> bestMoves = new List<ScoredMove> { bestMove };
            foreach (Move move in moves)
            {
                int score;
                Boolean shouldBreak = false;
                board.MakeMove(move);
                if (depth == 0) // If we are stopping the search, return the score of the player, who just made a move
                {
                    int tmpScore = -1 * Score(board);
                    /*if (tmpScore != 0)
                    {
                        System.Console.WriteLine("Score at depth 0: " + tmpScore);
                    }*/
                    score = tmpScore;
                }
                else
                {
                    // Get the score of the best move our opponent could make, and return -1 * that
                    score = PickBestMove(board, depth - 1, -beta, -alpha).score;
                    /*if (score > beta)
                    {
                        shouldBreak = true;
                    }
                    alpha = Math.Max(alpha, score);*/
                    score *= -1;
                }
                if (board.IsInCheck())
                {
                    score += checkBonus;
                }
                if (board.IsInCheckmate())
                {
                    score += checkMateBonus;
                    //System.Console.WriteLine("Found a checkmate with score " + score + " at position " + board.GetFenString());
                }
                board.UndoMove(move);
                if (score == bestMove.score)
                {
                    bestMoves.Add(new ScoredMove(move, score));
                }
                if (score > bestMove.score)
                {
                    bestMove = new ScoredMove(move, score);
                    bestMoves = new List<ScoredMove> { bestMove };
                }
                if (shouldBreak)
                {
                    break;
                }
            }

            /*if (bestMove.score != 0 && depth != 0)
            {
                System.Console.WriteLine("Score at depth " + depth + ": " + bestMove.score);
            }*/

            return bestMoves[rng.Next(bestMoves.Count)];
            //return bestMoves[0];
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
        // BOT END
        /*
        // EvilBot START
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();

            // Pick a random move to play if nothing better is found
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
            int highestValueCapture = 0;

            foreach (Move move in allMoves)
            {
                // Always play checkmate in one
                if (MoveIsCheckmate(board, move))
                {
                    moveToPlay = move;
                    break;
                }

                // Find highest value capture
                Piece capturedPiece = board.GetPiece(move.TargetSquare);
                int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

                if (capturedPieceValue > highestValueCapture)
                {
                    moveToPlay = move;
                    highestValueCapture = capturedPieceValue;
                }
            }

            return moveToPlay;
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }
        */
    }
}