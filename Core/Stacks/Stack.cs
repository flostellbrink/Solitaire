using System.Linq;
using Core.Game;

namespace Core.Stacks;

public class Stack : AbstractStack
{
    private readonly int _index;

    public Stack(int index)
    {
        _index = index;
    }

    public Stack(Stack stack) : base(stack)
    {
        _index = stack._index;
    }

    public override int MovableCards
    {
        get
        {
            if (!Cards.Any())
                return 0;
            if (Cards.Count == 1)
                return 1;

            var result = 1;
            for (var i = Cards.Count - 2; i >= 0; i--)
            {
                if (!Cards[i].CanHold(Cards[i + 1]))
                    break;

                result++;
            }
            return result;
        }
    }

    public override bool Accepts(Card card, int count) =>
        !Cards.Any() || Cards.Last().CanHold(card);

    public override string ToString() => $"Stack {_index}";
}
