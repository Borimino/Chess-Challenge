using ChessChallenge.API;

public class MyBot : IChessBot
{
    bool goodMoves = false;

    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    // Start depth
    int startDepth = 3;

    public Move Think(Board board, Timer timer)
    {
        ScoredMove scoredMove = PickBestMove(board, startDepth);
        System.Console.WriteLine("Score for this move was " +  scoredMove.score);
        return scoredMove.move;
    }

    private ScoredMove PickBestMove(Board board, int depth)
    {
        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0)
        {
            System.Console.WriteLine("No more moves to make");
            return new ScoredMove(new Move(), int.MinValue);
        }

        ScoredMove bestMove = new ScoredMove(moves[0], goodMoves ? int.MinValue : int.MaxValue);
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
            board.UndoMove(move);
            if (goodMoves ? score > bestMove.score : score < bestMove.score)
            {
                bestMove = new ScoredMove(move, score);
            }
        }

        if (bestMove.score != 0 && depth != 0)
        {
            System.Console.WriteLine("Score at depth " + depth + ": " + bestMove.score);
        }

        return bestMove;
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