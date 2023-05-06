using System;
using System.Linq;
using Core;
using Core.Game;

namespace Solitaire
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine(
                @"
 _______  _____         _____ _______ _______ _____  ______ _______
 |______ |     | |        |      |    |_____|   |   |_____/ |______
 ______| |_____| |_____ __|__    |    |     | __|__ |    \_ |______
"
            );

            const string createGame = "Create new game";
            const string benchmarkNormal = "Benchmark solvability";
            const string benchmarkHard = "Hard mode benchmark";
            var decision = "{0}".AskForDecision(createGame, benchmarkNormal, benchmarkHard);
            switch (decision)
            {
                case createGame:
                    Console.WriteLine("Creating new game");
                    Console.WriteLine();
                    CreateGame();
                    return;
                case benchmarkNormal:
                    Console.WriteLine("Running benchmark:");
                    Console.WriteLine();
                    BenchmarkSolvability(Solver.Mode.Normal);
                    return;
                case benchmarkHard:
                    Console.WriteLine("Running benchmark:");
                    Console.WriteLine();
                    BenchmarkSolvability(Solver.Mode.Hard);
                    return;
                default:
                    throw new($"Unexpected decision: {decision}");
            }
        }

        private static void CreateGame()
        {
            Board board;
            while (true)
            {
                Console.WriteLine("Creating board");
                Console.WriteLine();
                board = CreateBoard();
                Console.WriteLine(board);
                var valid = board.IsValid();
                Console.WriteLine();
                if (valid)
                    break;
            }

            const string solve = "Solve automatically";
            const string play = "Play";
            var decision = "{0}".AskForDecision(solve, play);
            switch (decision)
            {
                case solve:
                    Console.WriteLine("Solving board automatically");
                    Console.WriteLine();
                    Solve(board, Solver.Mode.Normal);
                    return;
                case play:
                    Console.WriteLine("Playing game");
                    Console.WriteLine();
                    Play(board);
                    return;
                default:
                    throw new ArgumentException();
            }
        }

        private static Board CreateBoard()
        {
            const string random = "Random board";
            const string custom = "Custom board";
            const string seed = "Seed board";
            var decision = "{0}".AskForDecision(random, custom, seed);
            switch (decision)
            {
                case random:
                    Console.WriteLine("Creating random board");
                    Console.WriteLine();
                    return new RandomBoard();
                case custom:
                    Console.WriteLine("Creating custom board");
                    Console.WriteLine();
                    return new CustomBoard();
                case seed:
                    Console.WriteLine("Creating board from seed");
                    Console.WriteLine();
                    return new RandomBoard("Board seed".AskForNumber(0, int.MaxValue));
                default:
                    throw new ArgumentException();
            }
        }

        private static void BenchmarkSolvability(Solver.Mode mode)
        {
            var solved = 0;
            var failed = 0;
            while (true)
            {
                var board = new RandomBoard();
                var solver = new Solver(board, mode);
                var solution = solver.Solve();
                if (solution == null)
                {
                    Console.WriteLine($"\nFailed to solve game with seed {board.Seed}");
                    failed++;
                }
                else
                {
                    Console.WriteLine(
                        $"\nSolved. Solution contains {solution.MoveHistory.Count} steps total."
                    );
                    solved++;
                }

                Console.WriteLine(
                    $"Solve rate: {(double)solved / (solved + failed):P}, solved: {solved}, failed: {failed}"
                );
            }
        }

        private static void Solve(Board board, Solver.Mode mode)
        {
            // Create a clone for replay
            var clone = new Board(board);

            // Solve the original board
            var solver = new Solver(board, mode);
            var solution = solver.Solve();
            if (solution == null)
            {
                Console.WriteLine("Failed to find a solution");
                return;
            }

            // Replay the solution and count manual moves
            var manualMoves = 0;
            var index = 0;
            foreach (var move in solution.MoveHistory.Reverse())
            {
                var translated = move.Clone(clone);
                var automatic = translated.IsForced();
                translated.Apply();

                if (!automatic)
                    manualMoves++;
                Console.Write($"({++index}/{solution.MoveHistory.Count}) {move}");
                Console.WriteLine(automatic ? " (automatic)\n" : $"\n\n{clone}");
            }

            Console.WriteLine();
            Console.Write($"Solution has {solution.MoveHistory.Count} moves, ");
            Console.Write($"of which {manualMoves} are not automatic.");
        }

        private static void Play(Board board)
        {
            board.ApplyForcedMoves();
            while (true)
            {
                Console.WriteLine("Current board:");
                Console.WriteLine(board);

                if (board.Solved)
                {
                    Console.WriteLine(
                        "Congratulations, you solved the puzzle! You'll have to imagine the animation for now ;)"
                    );
                    return;
                }

                var moves = board.DistinctMoves.ToArray();
                if (!moves.Any())
                {
                    Console.WriteLine("No moves left, you lose.");
                    return;
                }

                var selectedMove = "Chose your move".AskForIndexedDecision(moves);
                Console.WriteLine($"Selected move: {selectedMove}");
                Console.WriteLine();
                selectedMove.Apply();
            }
        }
    }
}
