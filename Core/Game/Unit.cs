using System.Collections.Generic;
using System.Linq;

namespace Core.Game
{
    public record Unit(ICollection<Card> Cards)
    {
        public bool Valid =>
            Cards.ConsecutivePairs().All(cards => cards.Item1.CanHold(cards.Item2));

        public override string ToString() => string.Join(", ", Cards);
    }
}
