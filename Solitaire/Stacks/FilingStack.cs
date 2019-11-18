using System.Collections.Generic;
using System.Linq;
using Solitaire.Game;

namespace Solitaire.Stacks
{
    public class FilingStack : AbstractStack
    {
        public Color Color { get; }

        public FilingStack(Color color)
        {
            Color = color;
        }

        public FilingStack(FilingStack stack) : base(stack)
        {
            Color = stack.Color;
        }

        public Value NextIndex => Cards.LastOrDefault()?.Value + 1 ?? Value.N1;

        public override IEnumerable<Unit> MovableCards => Enumerable.Empty<Unit>();

        public override bool Accepts(Unit unit)
        {
            if (unit.Cards.Count != 1) return false;
            var card = unit.Cards.First();
            if (card.Color != Color || !Card.NumericValues.Contains(card.Value)) return false;
            return card.Value == NextIndex;
        }

        public override string ToString() => $"Filing {Color}";
    }
}