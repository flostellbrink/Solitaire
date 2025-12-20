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

    public override int MovableCards => Locked || Cards.Count == 0 ? 0 : 1;

    public override bool Accepts(Card card, int count) => !Locked && Cards.Count == 0 && count == 1;

    public override string ToString() => $"Lockable {_index}";
}
