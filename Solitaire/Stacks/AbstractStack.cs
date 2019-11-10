using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Solitaire.Game;

namespace Solitaire.Stacks
{
    internal abstract class AbstractStack : IStack
    {
        internal readonly ICollection<Card> Cards = new List<Card>();

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
            foreach (var card in unit.Cards)
            {
                Cards.Remove(card);
            }
        }

        public override int GetHashCode()
        {
            return (Cards != null ? Cards.GetCollectionHashCode() : 0);
        }
    }
}