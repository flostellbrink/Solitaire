using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Solitaire.Game
{
    public class Unit : IEquatable<Unit>
    {
        internal readonly ICollection<Card> Cards;

        public Unit(ICollection<Card> cards)
        {
            Debug.Assert(cards.Any());
            Cards = cards;
        }

        public bool Valid => Cards.ConsecutivePairs().All(cards => cards.Item1.CanHold(cards.Item2));

        public bool Equals(Unit other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Cards.SequenceEqual(other.Cards);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Unit) obj);
        }

        public override string ToString() => string.Join(", ", Cards);
    }
}