using System.Collections.Generic;
using System.Linq;
using Solitaire.Game;

namespace Solitaire.Stacks
{
    internal class FilingStack : AbstractStack
    {
        private readonly Color _color;

        public FilingStack(Color color)
        {
            _color = color;
        }

        public Value NextIndex => Cards.FirstOrDefault()?.Value + 1 ?? Value.N1;

        public override IEnumerable<Unit> MovableCards => Enumerable.Empty<Unit>();

        public override bool Accepts(Unit unit)
        {
            if (unit.Cards.Count != 1) return false;
            var card = unit.Cards.First();
            if (card.Color != _color || !Card.NumericValues.Contains(card.Value)) return false;
            return card.Value == NextIndex;
        }

        public override string ToString() => $"Filing {_color}";
    }
}