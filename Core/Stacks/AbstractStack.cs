﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Game;

namespace Core.Stacks
{
    public abstract class AbstractStack : IStack
    {
        public readonly List<Card> Cards = new();

        public abstract IEnumerable<Unit> MovableCards { get; }

        public abstract bool Accepts(Unit _);

        protected AbstractStack()
        {
        }

        protected AbstractStack(AbstractStack stack)
        {
            Cards = new List<Card>(stack.Cards);
        }

        public void Add(Unit unit)
        {
            foreach (var card in unit.Cards)
            {
                Cards.Add(card);
            }
        }

        public void Remove(Unit unit)
        {
            Debug.Assert(MovableCards.Contains(unit));
            foreach (var card in unit.Cards.Reverse())
            {
                Debug.Assert(Cards[^1].Equals(card));
                Cards.RemoveAt(Cards.Count - 1);
            }
        }

        public override int GetHashCode()
        {
            return Cards != null ? Cards.GetCollectionHashCode() : 0;
        }
    }
}