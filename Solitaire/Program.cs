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
            RandomGame();
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
                if(!moves.Any()) break;

                var selectedMove = moves[random.Next(moves.Count)];
                Console.WriteLine("Selected move: " + selectedMove);
                Console.WriteLine();

                selectedMove.Apply(board);
            }
        }
    }
}
