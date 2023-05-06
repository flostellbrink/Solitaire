using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Game;

namespace Core.Stacks
{
    public abstract class AbstractStack : IStack
    {
        public List<Card> Cards { get; } = new();

        public abstract IEnumerable<Unit> MovableCards { get; }

        public abstract bool Accepts(Unit unit);

        protected AbstractStack() { }

        protected AbstractStack(AbstractStack stack)
        {
            Cards = new List<Card>(stack.Cards);
        }

        public void Add(Unit unit)
        {
            Cards.AddRange(unit.Cards);
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
            return Cards.GetCollectionHashCode();
        }
    }
}
