using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

        public Solver(Board board)
        {
            Board = board;
            AddToFrontier(board);
        }

        public Board Solve(bool debugOutput = true)
        {
            while (true)
            {
                if (Frontier.Count == 0) return null;
                var (key, value) = Frontier.First();
                if ( !value.Any())
                {
                    Frontier.Remove(key);
                    continue;
                }
                var currentBoard = value.First();
                value.Remove(currentBoard);

                if (debugOutput)
                {
                    Console.Write($"Current Loss (Smaller is better): {currentBoard.Loss}, visited: {VisitedBoards.Count}, active: {Frontier.Values.Sum(v => v.Count)} \r");
                }

                if(!VisitedBoards.Add(currentBoard.GetHashCode())) continue;
                if (currentBoard.Solved) return currentBoard;

                foreach (var move in currentBoard.AllMoves.ToList())
                {
                    var clone = new Board(currentBoard);
                    move.Translate(clone).Apply();
                    AddToFrontier(clone);
                }
            }
        }
    }
}
