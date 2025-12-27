using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Game;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;

namespace Server.Controllers;

[ApiController]
[Route("solitaire")]
public class SolitaireController : ControllerBase
{
    [HttpPost("solvable")]
    [Consumes("multipart/form-data")]
    public async Task<string> Solvable([FromForm, Required] IFormFile file)
    {
        AnsiColorHelper.Enabled = false;
        Console.WriteLine($"Got image {file.FileName} with {file.Length} bytes");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        using var mat = Mat.ImDecode(memoryStream.ToArray(), ImreadModes.Color);
        Console.WriteLine($"Loaded image with {mat.Width}x{mat.Height} pixels");

        var board = new ImageBoard(mat);
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
