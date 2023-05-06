using System.Collections.Generic;
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

    public LockableStack(LockableStack stack) : base(stack)
    {
        _index = stack._index;
        Locked = stack.Locked;
    }

    public override IEnumerable<Unit> MovableCards =>
        Locked || !Cards.Any() ? Enumerable.Empty<Unit>() : new[] { new Unit(new[] { Cards[0] }) };

    public override bool Accepts(Unit unit) => !Locked && !Cards.Any() && unit.Cards.Count == 1;

    public override string ToString() => $"Lockable {_index}";
}
