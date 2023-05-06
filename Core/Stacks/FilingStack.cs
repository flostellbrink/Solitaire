using System.Collections.Generic;
using System.Linq;
using Core.Game;

namespace Core.Stacks
{
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

        public override IEnumerable<Unit> MovableCards => Enumerable.Empty<Unit>();

        public override bool Accepts(Unit unit)
        {
            if (unit.Cards.Count != 1)
                return false;
            var card = unit.Cards.First();
            if (Cards.Any() && card.Color != Cards[0].Color)
                return false;
            if (!Card.NumericValues.Contains(card.Value))
                return false;
            return card.Value == NextIndex;
        }

        public override string ToString() => $"Filing {_index}";
    }
}
