using System.Collections.Generic;
using System.Linq;
using Solitaire.Game;

namespace Solitaire.Stacks
{
    internal class Stack : AbstractStack
    {
        private readonly int _index;

        public Stack(int index)
        {
            _index = index;
        }

        public override IEnumerable<Unit> MovableCards =>
            Cards.Reverse().Aggregate(Enumerable.Empty<Unit>(), (result, card) => result
                    .Append(new Unit((result.LastOrDefault()?.Cards ?? Enumerable.Empty<Card>())
                        .Prepend(card).ToList())))
                .TakeWhile(unit => unit.Valid);

        public override bool Accepts(Unit unit)
        {
            return !Cards.Any() || Cards.Last().CanHold(unit.Cards.First());
        }

        public override string ToString() => $"Stack {_index}";
    }
}