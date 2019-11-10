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

        private List<Board> Frontier { get; } = new List<Board>();

        private Board Result { get; set; } = null;

        public Solver(Board board)
        {
            Board = board;
            Frontier.Add(board);
        }

        public Board Solve()
        {
            while (true)
            {
                Frontier.Sort((a, b) => a.Loss.CompareTo(b.Loss));
                var currentBoard = Frontier.FirstOrDefault();
                if (currentBoard == null) return null;
                Frontier.Remove(currentBoard);
                Console.Write($"Current Loss (Smaller is better): {currentBoard.Loss}  \r");

                if(!VisitedBoards.Add(currentBoard.GetHashCode())) continue;
                if (currentBoard.Solved) return currentBoard;

                foreach (var move in currentBoard.AllMoves.ToList())
                {
                    var clone = new Board(currentBoard);
                    move.Translate(clone).Apply();
                    Frontier.Add(clone);
                }
            }
        }
    }
}
