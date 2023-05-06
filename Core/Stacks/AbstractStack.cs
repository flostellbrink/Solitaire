using System.Collections.Generic;
using Core.Game;

namespace Core.Stacks;

public abstract class AbstractStack
{
    public List<Card> Cards { get; } = new();

    public abstract int MovableCards { get; }

    public abstract bool Accepts(Card card, int count);

    protected AbstractStack() { }

    protected AbstractStack(AbstractStack stack)
    {
        Cards = new List<Card>(stack.Cards);
    }

    public void Add(IEnumerable<Card> cards)
    {
        Cards.AddRange(cards);
    }

    public void Add(Card card)
    {
        Cards.Add(card);
    }

    public void Remove(int count)
    {
        Cards.RemoveRange(Cards.Count - count, count);
    }

    public override int GetHashCode()
    {
        return Cards.GetCollectionHashCode();
    }
}
