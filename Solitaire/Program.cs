using System;
using System.Linq;
using Solitaire.Game;

namespace Solitaire
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RandomGame();
        }

        public static void RandomGame()
        {
            var random = new Random();
            var board = new Board();

            while (true)
            {
                Console.WriteLine("Board:");
                Console.WriteLine(board);

                var moves = board.AllMoves.ToList();
                if(!moves.Any()) break;

                var selectedMove = moves[random.Next(moves.Count)];
                Console.WriteLine("Selected move: " + selectedMove);
                Console.WriteLine();

                selectedMove.Apply(board);

                Console.ReadKey();
            }
        }
    }
}
