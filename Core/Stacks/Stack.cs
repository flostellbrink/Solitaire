using System.Collections.Generic;
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

    public override IEnumerable<Unit> MovableCards =>
        Cards
            .AsEnumerable()
            .Reverse()
            .Aggregate(
                Enumerable.Empty<Unit>(),
                (result, card) =>
                    result.Append(
                        new Unit(
                            (result.LastOrDefault()?.Cards ?? Enumerable.Empty<Card>())
                                .Prepend(card)
                                .ToList()
                        )
                    )
            )
            .TakeWhile(unit => unit.Valid);

    public override bool Accepts(Unit unit)
    {
        return !Cards.Any() || Cards.Last().CanHold(unit.Cards.First());
    }

    public override string ToString() => $"Stack {_index}";
}
