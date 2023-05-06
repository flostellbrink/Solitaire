using System;
using System.Linq;

namespace Core.Game;

public class RandomBoard : Board
{
    public int Seed { get; }

    public RandomBoard(int? seed = null)
    {
        Seed = seed ?? Environment.TickCount;
        var random = new Random(Seed);
        var deck = Card.FullSet.OrderBy(_ => random.Next()).ToList();

        for (var i = 0; i < deck.Count; i++)
            Stacks.ElementAt(i % Stacks.Count).Cards.Add(deck[i]);

        Console.WriteLine($"Random board based on seed: {Seed}");
        Console.WriteLine(this);
    }
}
