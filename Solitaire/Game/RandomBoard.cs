using System;
using System.Linq;

namespace Solitaire.Game
{
    public class RandomBoard : Board
    {
        public int Seed { get; }

        public RandomBoard(int? seed = null) : base(true)
        {
            Seed = seed ?? Environment.TickCount;

            var random = new Random(Seed);
            var deck = Card.FullSet.OrderBy(_ => random.Next()).ToList();

            var stackIndex = 0;
            foreach (var card in deck)
            {
                Stacks.ElementAt(stackIndex++ % Stacks.Count).Cards.Add(card);
            }

            Console.WriteLine($"Random board based on seed: {Seed}");
            Console.WriteLine(this);
        }
    }
}