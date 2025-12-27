using System.Linq;
using Core.Game;

namespace Core.Stacks;

public class LockableStack : AbstractStack
{
    private readonly int _index;

    public bool Locked { get; set; }

    public LockableStack(int index)
    {
        _index = index;
    }

    public LockableStack(LockableStack stack)
        : base(stack)
    {
        _index = stack._index;
        Locked = stack.Locked;
    }

    public override int MovableCards => Locked || Cards.Count == 0 ? 0 : 1;

    public override bool Accepts(Card card, int count)
    {
        if (Locked)
            return false;
        if (count == 1 && Cards.Count == 0)
            return true;
        if (count == 4 && card.Value == Value.Dragon)
        {
            if (Cards.Count == 0)
                return true;
            if (
                Cards.Count == 1
                && Cards.Single().Value == Value.Dragon
                && Cards.Single().Color == card.Color
            )
                return true;
        }
        return false;
    }

    public override string ToString() => $"Lockable {_index}";
}
