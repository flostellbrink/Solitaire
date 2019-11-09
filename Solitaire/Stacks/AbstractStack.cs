using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Solitaire.Game;

namespace Solitaire.Stacks
{
    internal abstract class AbstractStack : IStack
    {
        internal ICollection<Card> Cards = new List<Card>();

        public abstract IEnumerable<Unit> MovableCards { get; }

        public abstract bool Accepts(Unit _);

        public void Add(Unit unit)
        {
            Debug.Assert(Accepts(unit));
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
    }
}