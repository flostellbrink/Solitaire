using System;
using System.ComponentModel;
using System.Linq;
using Core;
using Core.Game;
using Solitaire;

Console.WriteLine(
    @"
 _______  _____         _____ _______ _______ _____  ______ _______
 |______ |     | |        |      |    |_____|   |   |_____/ |______
 ______| |_____| |_____ __|__    |    |     | __|__ |    \_ |______
"
);

var decision = "{0}".AskForDecision(Tool.CreateGame, Tool.BenchmarkNormal, Tool.BenchmarkHard);
switch (decision)
{
    case Tool.CreateGame:
        Console.WriteLine("Creating new game");
        Console.WriteLine();
        CreateGame();
        return;
    case Tool.BenchmarkNormal:
        Console.WriteLine("Running benchmark:");
        Console.WriteLine();
        BenchmarkSolvability(Solver.Mode.Normal);
        return;
    case Tool.BenchmarkHard:
        Console.WriteLine("Running benchmark:");
        Console.WriteLine();
        BenchmarkSolvability(Solver.Mode.Hard);
        return;
    default:
        throw new($"Unexpected decision: {decision}");
}

static void CreateGame()
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

    var decision = "{0}".AskForDecision(PlayMode.Auto, PlayMode.Manual);
    switch (decision)
    {
        case PlayMode.Auto:
            Console.WriteLine("Solving board automatically");
            Console.WriteLine();
            Solve(board, Solver.Mode.Normal);
            return;
        case PlayMode.Manual:
            Console.WriteLine("Playing game");
            Console.WriteLine();
            Play(board);
            return;
        default:
            throw new($"Unexpected decision: {decision}");
    }
}

static Board CreateBoard()
{
    var decision = "{0}".AskForDecision(BoardType.Random, BoardType.Custom, BoardType.Seed);
    switch (decision)
    {
        case BoardType.Random:
            Console.WriteLine("Creating random board");
            Console.WriteLine();
            return new RandomBoard();
        case BoardType.Custom:
            Console.WriteLine("Creating custom board");
            Console.WriteLine();
            return new CustomBoard();
        case BoardType.Seed:
            Console.WriteLine("Creating board from seed");
            Console.WriteLine();
            return new RandomBoard("Board seed".AskForNumber(0, int.MaxValue));
        default:
            throw new($"Unexpected decision: {decision}");
    }
}

static void BenchmarkSolvability(Solver.Mode mode)
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

static void Solve(Board board, Solver.Mode mode)
{
    // Create a clone for replay
    var clone = new Board(board);

    // Solve the original board
    var solver = new Solver(board, mode);
    var solution = solver.Solve();
    Console.WriteLine();
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
        var automatic = move.IsForced(clone);
        if (!automatic)
            manualMoves++;

        Console.Write($"({++index}/{solution.MoveHistory.Count}) {move.Stringify(clone)}");
        move.Apply(clone);

        Console.WriteLine(automatic ? " (automatic)\n" : $"\n\n{clone}");
    }

    Console.WriteLine();
    Console.Write($"Solution has {solution.MoveHistory.Count} moves, ");
    Console.Write($"of which {manualMoves} are not automatic.");
}

static void Play(Board board)
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

        var selectedMove = "Chose your move".AskForIndexedDecision(
            moves.Select(m => m.Stringify(board)).ToArray()
        );
        Console.WriteLine($"Selected move: {selectedMove}");
        Console.WriteLine();
        moves.Single(m => m.Stringify(board) == selectedMove).Apply(board);
    }
}

namespace Solitaire
{
    enum Tool
    {
        [Description("Create new game")]
        CreateGame,

        [Description("Benchmark solvability")]
        BenchmarkNormal,

        [Description("Hard mode benchmark")]
        BenchmarkHard,
    }

    enum PlayMode
    {
        [Description("SolveAutomatically")]
        Auto,

        [Description("Play")]
        Manual,
    }

    enum BoardType
    {
        [Description("Random board")]
        Random,

        [Description("Custom board")]
        Custom,

        [Description("Seed board")]
        Seed,
    }
}
