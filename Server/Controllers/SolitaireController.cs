﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.Game;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Core;

namespace Server.Controllers;

[ApiController]
[Route("solitaire")]
public class SolitaireController : ControllerBase
{
    [HttpPost("solvable")]
    public static string Solvable([Required] IFormFile file)
    {
        AnsiColorHelper.Enabled = false;
        Console.WriteLine($"Got image {file.FileName} with {file.Length} bytes");

        using var image = Image.Load<Rgba32>(file.OpenReadStream());
        Console.WriteLine($"Loaded image with {image.Width}x{image.Height} pixels");

        var board = new ImageBoard(image);
        Console.WriteLine(board.ToString());
        if (!board.IsValid())
            return "Sorry cannot read this board.";
        if (board.Solved)
            return "Good job!";

        var solver = new DFSolver(board, Mode.Normal);
        var solution = solver.Solve();
        Console.WriteLine();
        if (solution == null)
            return "Looks like you're stuck!";

        var nextMove = solution.MoveHistory.LastOrDefault();
        if (nextMove == null)
            return "Good job!";

        return $"You got this! {nextMove.ToString(board)} :)";
    }
}
