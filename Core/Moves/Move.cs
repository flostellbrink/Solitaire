using System.Diagnostics;
using System.Linq;
using Core.Game;
using Core.Stacks;

namespace Core.Moves
{
    internal class Move : IMove
    {
        public readonly Board Board;

        public readonly IStack Source;

        public readonly IStack Destination;

        public readonly Unit Unit;

        public Move(Board board, IStack source, IStack destination, Unit unit)
        {
            Board = board;
            Source = source;
            Destination = destination;
            Unit = unit;
        }

        public IMove Clone(Board targetBoard)
        {
            return new Move(targetBoard,
                targetBoard.AllStacks.Single(source => source.ToString() == Source.ToString()),
                targetBoard.AllStacks.Single(destination => destination.ToString() == Destination.ToString()),
                Unit);
        }

        public bool IsForced()
        {
            if (Unit.Cards.Count != 1) return false;
            var card = Unit.Cards.First();

            return Destination switch
            {
                FlowerStack _ when card.Color == Color.Flower && card.Value == Value.Flower => true,
                FilingStack _ when Board.HighestAutomaticFilingValue >= card.Value => true,
                _ => false
            };
        }

        public void Apply()
        {
            Source.Remove(Unit);
            Debug.Assert(Destination.Accepts(Unit));
            Destination.Add(Unit);

            Board.MoveHistory.Push(this);
            Board.ApplyForcedMoves();
        }

        public override string ToString()
        {
            return $"Move {Unit} from {Source} to {Destination}";
        }
    }
}