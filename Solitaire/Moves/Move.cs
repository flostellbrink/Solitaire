using System.Linq;
using Solitaire.Game;
using Solitaire.Stacks;

namespace Solitaire.Moves
{
    internal class Move : IMove
    {
        public readonly IStack Source;

        public readonly IStack Destination;

        public readonly Unit Unit;

        public Move(IStack source, IStack destination, Unit unit)
        {
            Source = source;
            Destination = destination;
            Unit = unit;
        }

        public bool IsForced(Board board)
        {
            if (Unit.Cards.Count != 1) return false;
            var card = Unit.Cards.First();

            return Destination switch
            {
                FlowerStack _ when card.Color == Color.Flower && card.Value == Value.Flower => true,
                FilingStack _ when board.MinNextFilingValue == card.Value => true,
                _ => false
            };
        }

        public void Apply(Board board)
        {
            Source.Remove(Unit);
            Destination.Add(Unit);
            board.ApplyForcedMove();
        }

        public override string ToString()
        {
            return $"Move {Unit} from {Source} to {Destination}";
        }
    }
}