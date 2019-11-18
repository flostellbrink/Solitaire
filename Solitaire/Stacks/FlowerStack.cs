using System.Collections.Generic;
using System.Linq;
using Solitaire.Game;

namespace Solitaire.Stacks
{
    public class FlowerStack : AbstractStack
    {
        public FlowerStack() { }

        public FlowerStack(FlowerStack flowerStack) : base(flowerStack)
        {
        }

        public override IEnumerable<Unit> MovableCards => Enumerable.Empty<Unit>();

        public override bool Accepts(Unit unit) =>
            !Cards.Any() &&
            unit.Cards.Count == 1 &&
            unit.Cards.First().Color == Color.Flower &&
            unit.Cards.First().Value == Value.Flower;

        public override string ToString() => "Flower stack";
    }
}