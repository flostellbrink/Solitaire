using System;
using System.Linq;
using Solitaire.Game;

namespace Solitaire
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // We want a completely blank board to replay the history on
            var board = new Board(applyForcedMoves: false);
            Console.WriteLine(board);

            // Solve the board on a copy with automatic moves
            var clone = new Board(board) {ApplyForcedMoves = true};
            clone.ApplyForcedMove();
            var solver = new Solver(clone);
            var solution = solver.Solve();

            // Replay the solution and count manual moves
            var manualMoves = 0;
            foreach (var move in solution.MoveHistory.Reverse())
            {
                move.Translate(board).Apply();
                Console.Write(move);
                if (!move.IsForced())
                {
                    Console.WriteLine(" (manual)\n");
                    Console.WriteLine(board);
                    manualMoves++;
                }
                else
                {
                    Console.WriteLine(" (automatic)\n");
                }
                
            }

            Console.WriteLine();
            Console.WriteLine($"Solution has {solution.MoveHistory.Count} moves, of which {manualMoves} are not automatic.");
        }
    }
}
