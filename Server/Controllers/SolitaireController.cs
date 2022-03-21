using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Solitaire;
using Solitaire.Game;

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
            if (!board.IsValid())
            {
                return "Sorry cannot read this board.";
            }

            var solver = new Solver(board);
            var solution = solver.Solve();
            Console.WriteLine();

            if (solution == null)
            {
                return "Looks like you're stuck!";
            }

            var nextMove = solution.MoveHistory.LastOrDefault();
            if (nextMove == null)
            {
                return "Good job!";
            }

            return nextMove.ToString();
        }
    }
}