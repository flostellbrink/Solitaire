using System;
using System.Linq;

namespace Core.Game;

public class RandomBoard : Board
{
    public int Seed { get; }

    public RandomBoard(int? seed = null)
    {
        var random = seed is null ? new Random() : new Random(seed.Value);
        var deck = Card.FullSet.OrderBy(_ => random.Next()).ToList();

        for (var i = 0; i < deck.Count; i++)
            Stacks.ElementAt(i % Stacks.Count).Cards.Add(deck[i]);

        Console.WriteLine($"Random board based on seed: {Seed}");
        Console.WriteLine(this);
    }
}
