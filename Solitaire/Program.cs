using System;
using System.Collections.Generic;
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
            var index = 0;
            foreach (var move in solution.MoveHistory.Reverse())
            {
                var translated = move.Translate(board);
                var automatic = translated.IsForced();
                translated.Apply();

                if (!automatic) manualMoves++;

                Console.WriteLine($"MinNextFilingLevel: {board.MinNextFilingValue}");
                Console.Write($"({++index}/{solution.MoveHistory.Count}) {move}" +
                              (automatic ? "(automatic)\n\n" : $"\n\n{board}"));
            }

            Console.WriteLine();
            Console.WriteLine($"Solution has {solution.MoveHistory.Count} moves, of which {manualMoves} are not automatic.");
        }

        public static void RandomGame()
        {
            var random = new Random();
            var board = new Board();
            var knownBoards = new HashSet<int>();

            while (true)
            {
                if (!knownBoards.Add(board.GetHashCode()))
                {
                    Console.WriteLine("Already known board state");
                    break;
                }

                Console.WriteLine($"Board ({board.GetHashCode()}):");
                Console.WriteLine(board);

                var moves = board.AllMoves.ToList();
                if (!moves.Any()) break;

                var selectedMove = moves[random.Next(moves.Count)];
                Console.WriteLine("Selected move: " + selectedMove);
                Console.WriteLine();

                selectedMove.Apply();
            }
        }
    }
}
