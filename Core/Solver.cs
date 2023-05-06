using System;
using System.Collections.Generic;
using System.Linq;
using Core.Game;

namespace Core;

public class Solver
{
    public enum Mode
    {
        Normal,
        Hard,
    }

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

    public Solver(Board board, Mode mode)
    {
        this.mode = mode;

        board.ApplyForcedMoves();
        AddToFrontier(board);
    }

    public Board? Solve()
    {
        while (true)
        {
            Console.Write("\r".PadRight(Console.WindowWidth) + "\r");

            Console.Write(
                $"Visited: {VisitedBoards.Count}, Active: {Frontier.Values.Sum(v => v.Count)}"
            );

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
            AddToFrontier(clone);
        }

        return null;
    }
}
