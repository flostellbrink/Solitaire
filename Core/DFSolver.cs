using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Game;
using Core.Moves;

namespace Core;

/// <summary>
/// Depth-first solver.
/// Great for finding a bad solution quickly.
/// </summary>
public class DFSolver
{
    private HashSet<int> VisitedBoards { get; } = new();

    private readonly Mode mode;

    private readonly Board board;

    private readonly List<Board> Solutions = new();

    public bool Silent { get; set; }

    public DFSolver(Board board, Mode mode)
    {
        this.mode = mode;
        this.board = new Board(board);
    }

    public Board? Solve()
    {
        if (!Silent)
            Console.WriteLine();
        Solutions.Clear();

        stack.Push(new State { PreForceHash = board.GetHashCode() });
        SolveIteratively();

        return Solutions.OrderBy(board => board.MoveHistory.Count).FirstOrDefault();
    }

    class State
    {
        public required int PreForceHash;
        public List<IMove>? ForcedMoves;
        public int PreMoveHash;
        public IEnumerator<IMove>? Moves;
        public IMove? Move;
    }

    private readonly Stack<State> stack = new();

    private void UndoForcedMoves(List<IMove> forcedMoves)
    {
        foreach (var forcedMove in forcedMoves.AsEnumerable().Reverse())
            forcedMove.Undo(board);
    }

    private void SolveIteratively()
    {
        while (stack.Any())
        {
            var state = stack.Peek();
            if (state.ForcedMoves == null)
            {
                var preForcedHash = board.GetHashCode();
                if (!VisitedBoards.Add(preForcedHash))
                {
                    stack.Pop();
                    continue;
                }

                state.ForcedMoves = new List<IMove>();
                var moves = board.AllMoves.ToList();
                while (true)
                {
                    var forcedMove = moves.Find(move => move.IsForced(board));
                    if (forcedMove == null)
                        break;

                    state.ForcedMoves.Add(forcedMove);
                    forcedMove.Apply(board);
                    moves = board.AllMoves.ToList();
                }

                state.Moves = moves.GetEnumerator();

                var hash = board.GetHashCode();
                if (hash != preForcedHash && !VisitedBoards.Add(hash))
                {
                    UndoForcedMoves(state.ForcedMoves);
                    stack.Pop();
                    continue;
                }

                if (mode == Mode.Hard && !board.HardModeValid)
                {
                    UndoForcedMoves(state.ForcedMoves);
                    stack.Pop();
                    continue;
                }

                if (board.Solved)
                {
                    Solutions.Add(new Board(board));
                    UndoForcedMoves(state.ForcedMoves);
                    stack.Pop();
                    return;
                }

                if (VisitedBoards.Count % 10_000 == 0 && !Silent)
                    Console.Write($"\r{VisitedBoards.Count:n0} boards visited");

                state.PreMoveHash = board.GetHashCode();
            }

            state.Move?.Undo(board);
            Debug.Assert(board.GetHashCode() == state.PreMoveHash);

            if (state.Moves!.MoveNext())
            {
                state.Move = state.Moves.Current;
                state.Move.Apply(board);
                stack.Push(new State { PreForceHash = board.GetHashCode() });
            }
            else
            {
                UndoForcedMoves(state.ForcedMoves!);
                Debug.Assert(board.GetHashCode() == state.PreForceHash);
                stack.Pop();
            }
        }
    }
}
