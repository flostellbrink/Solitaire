using System;
using System.Collections.Generic;
using System.Linq;
using Solitaire.Game;

namespace Solitaire
{
    public class Solver
    {
        public Board Board { get; }

        private HashSet<int> VisitedBoards { get; } = new HashSet<int>();

        private HashSet<int> InFrontier { get; } = new HashSet<int>();

        private SortedList<int, List<Board>> Frontier { get; } = new SortedList<int, List<Board>>();

        private void AddToFrontier(Board board)
        {
            var hash = board.GetHashCode();
            var loss = board.Loss;

            lock (VisitedBoards)
            {
                if (VisitedBoards.Contains(hash)) return;
            }

            lock (Frontier)
            {
                if (!InFrontier.Add(hash)) return;
                if (Frontier.ContainsKey(loss))
                    Frontier[loss].Add(board);
                else
                    Frontier.Add(loss, new List<Board> {board});
            }
        }

        private IEnumerable<Board> GetFromFrontier()
        {
            lock (Frontier)
            {
                if (Frontier.Count == 0) return null;
                var (key, value) = Frontier.First();
                Frontier.Remove(key);
                return value;
            }
        }

        public Solver(Board board)
        {
            board.ApplyForcedMove();
            Board = board;
            AddToFrontier(board);
        }

        public Board Solve()
        {
            while (true)
            {
                Console.Write("\r".PadRight(Console.WindowWidth) + "\r");

                Console.Write($"Visited: {VisitedBoards.Count}, Active: {Frontier.Values.Sum(v => v.Count)}");

                var boards = GetFromFrontier();
                if (boards == null) return null;

                var solution = boards.AsParallel().Select(Solve)
                    .FirstOrDefault(board => board != null);
                if (solution != null) return solution;
            }
        }

        private Board Solve(Board board)
        {
            if (board == null) return null;
            if (board.Solved) return board;

            lock (VisitedBoards)
            {
                if (!VisitedBoards.Add(board.GetHashCode())) return null;
            }

            foreach (var move in board.AllMoves.ToList())
            {
                var clone = new Board(board);
                move.Clone(clone).Apply();
                AddToFrontier(clone);
            }

            return null;
        }
    }
}
