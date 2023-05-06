using System.Collections.Generic;
using System.Linq;

namespace Core.Game;

public sealed record Unit(ICollection<Card> Cards)
{
    public bool Valid => Cards.ConsecutivePairs().All(cards => cards.Item1.CanHold(cards.Item2));

    public bool Equals(Unit? other) => other != null && Cards.SequenceEqual(other.Cards);

    public override int GetHashCode() => Cards.GetCollectionHashCode();

    public override string ToString() => string.Join(", ", Cards);
}
