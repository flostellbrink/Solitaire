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
            if (VisitedBoards.Contains(hash) || InFrontier.Contains(hash)) return;
            InFrontier.Add(hash);
            var loss = board.Loss;
            if (Frontier.ContainsKey(loss))
                Frontier[loss].Add(board);
            else
                Frontier.Add(loss, new List<Board> {board});
        }

        private Board GetFromFrontier()
        {
            if (Frontier.Count == 0) return null;
            var (key, value) = Frontier.First();

            var result = value.First();
            value.Remove(result);
            if (!value.Any())
            {
                Frontier.Remove(key);
            }

            return result;
        }

        public Solver(Board board)
        {
            board.ApplyForcedMove();
            Board = board;
            AddToFrontier(board);
        }

        public Board Solve(bool debugOutput = true)
        {
            while (true)
            {
                var currentBoard = GetFromFrontier();
                if (currentBoard == null) return null;

                if (debugOutput)
                {
                    Console.Write($"Current Loss (Smaller is better): {currentBoard.Loss}, visited: {VisitedBoards.Count}, active: {Frontier.Values.Sum(v => v.Count)} \r");
                }

                if(!VisitedBoards.Add(currentBoard.GetHashCode())) continue;
                if (currentBoard.Solved) return currentBoard;

                foreach (var move in currentBoard.AllMoves.ToList())
                {
                    var clone = new Board(currentBoard);
                    move.Clone(clone).Apply();
                    AddToFrontier(clone);
                }
            }
        }
    }
}
