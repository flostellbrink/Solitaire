﻿using System;
using System.Linq;

namespace Solitaire.Game
{
    public class RandomBoard : Board
    {
        public int Seed { get; }

        public RandomBoard(int? seed = null) : base(true)
        {
            Seed = seed ?? Environment.TickCount;
            Console.WriteLine($"Random board based on seed: {Seed}");

            var random = new Random(Seed);
            var deck = Card.FullSet.OrderBy(_ => random.Next()).ToList();

            var stackIndex = 0;
            foreach (var card in deck)
            {
                _stacks.ElementAt(stackIndex++ % _stacks.Count).Cards.Add(card);
            }
        }
    }
}