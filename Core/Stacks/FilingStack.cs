using System.Linq;
using Core.Game;

namespace Core.Stacks;

public class FilingStack : AbstractStack
{
    private readonly int _index;

    public FilingStack(int index)
    {
        _index = index;
    }

    public FilingStack(FilingStack stack) : base(stack)
    {
        _index = stack._index;
    }

    public Value NextIndex => Cards.LastOrDefault()?.Value + 1 ?? Value.N1;

    public override int MovableCards => 0;

    public override bool Accepts(Card card, int count)
    {
        if (count != 1)
            return false;
        if (Cards.Count != 0 && card.Color != Cards[0].Color)
            return false;
        if (!Card.NumericValues.Contains(card.Value))
            return false;
        return card.Value == NextIndex;
    }

    public override string ToString() => $"Filing {_index}";
}
