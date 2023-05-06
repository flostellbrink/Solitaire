using System;
using System.Collections.Generic;
using System.Linq;
using Core.Game;

namespace Core;

/// <summary>
/// Breadth-first solver.
/// Great for finding an intuitive solution slowly.
/// </summary>
public class BFSolver
{
    private HashSet<int> VisitedBoards { get; } = new();

    private HashSet<int> InFrontier { get; } = new();

    private SortedList<int, List<Board>> Frontier { get; } = new();

    private readonly Mode mode;

    private void AddToFrontier(Board board)
    {
        var hash = board.GetHashCode();
        var loss = board.Loss;

        lock (VisitedBoards)
        {
            if (VisitedBoards.Contains(hash))
                return;
        }

        lock (Frontier)
        {
            if (!InFrontier.Add(hash))
                return;
            if (Frontier.TryGetValue(loss, out List<Board>? value))
                value.Add(board);
            else
                Frontier.Add(loss, new List<Board> { board });
        }
    }

    private IEnumerable<Board>? GetFromFrontier()
    {
        lock (Frontier)
        {
            if (Frontier.Count == 0)
                return null;
            var (key, value) = Frontier.First();
            Frontier.Remove(key);
            return value;
        }
    }

    public BFSolver(Board board, Mode mode)
    {
        this.mode = mode;

        var clone = new Board(board);
        clone.ApplyForcedMoves();
        AddToFrontier(clone);
    }

    public Board? Solve()
    {
        while (true)
        {
            Console.Write("\r".PadRight(Console.WindowWidth) + "\r");

            if (VisitedBoards.Count % 10_000 == 0)
                Console.Write($"{VisitedBoards.Count} visited");

            var boards = GetFromFrontier();
            if (boards == null)
                return null;

            var solution = boards.AsParallel().Select(Solve).FirstOrDefault(board => board != null);
            if (solution != null)
                return solution;
        }
    }

    private Board? Solve(Board board)
    {
        if (mode == Mode.Hard && !board.HardModeValid)
            return null;
        if (board.Solved)
            return board;

        lock (VisitedBoards)
        {
            if (!VisitedBoards.Add(board.GetHashCode()))
                return null;
        }

        foreach (var move in board.AllMoves)
        {
            var clone = new Board(board);
            move.Apply(clone);
            clone.ApplyForcedMoves();
            AddToFrontier(clone);
        }

        return null;
    }
}
