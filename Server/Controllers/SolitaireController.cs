using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Solitaire;

namespace Server.Controllers
{
    [ApiController]
    [Route("solitaire")]
    public class SolitaireController : ControllerBase
    {
        [HttpPost("solvable")]
        public string Solvable([Required] IFormFile file)
        {
            AnsiColorHelper.Enabled = false;
            using var image = Image.Load<Rgba32>(file.OpenReadStream());

            var board = new ImageBoard(image);
            Console.WriteLine(board.ToString());
            if (board.Solved) return "Good job!";
            if (!board.IsValid()) return "Sorry cannot read this board.";

            var solver = new Solver(board, Solver.Mode.Normal);
            var solution = solver.Solve();
            Console.WriteLine();
            if (solution == null) return "Looks like you're stuck!";

            var nextMove = solution.MoveHistory.LastOrDefault();
            if (nextMove == null) return "Good job!";
            Console.WriteLine(nextMove);
            return nextMove.ToString();
        }
    }
}