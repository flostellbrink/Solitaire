using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Solitaire.Game;
using Solitaire.Stacks;

namespace Solitaire.Moves
{
    internal class LockMove : IMove
    {
        public readonly Board Board;

        public readonly ICollection<IStack> Sources;

        public readonly LockableStack Destination;

        public readonly Unit Unit;

        public LockMove(Board board, ICollection<IStack> sources, LockableStack destination, Unit unit)
        {
            Board = board;
            Sources = sources;
            Destination = destination;
            Unit = unit;
        }

        public IMove Clone(Board targetBoard)
        {
            return new LockMove(targetBoard,
                Sources.Select(mSource =>
                    targetBoard.AllStacks.Single(source => source.ToString() == mSource.ToString())).ToList(),
                targetBoard.AllStacks.Single(destination => destination.ToString() == Destination.ToString()) as LockableStack,
                Unit);
        }

        public bool IsForced() => false;

        public void Apply()
        {
            Debug.Assert(Destination.Cards.Count <= 1);
            Debug.Assert(Destination.Cards.All(card => card.Color == Unit.Cards.First().Color));
            Debug.Assert(Destination.Cards.All(card => card.Value == Value.Dragon));

            foreach (var source in Sources)
            {
                source.Remove(Unit);
                Destination.Add(Unit);
            }

            Destination.Locked = true;

            Debug.Assert(Destination.Cards.Count == 4);
            Debug.Assert(Destination.Cards.All(card => card.Color == Unit.Cards.First().Color));
            Debug.Assert(Destination.Cards.All(card => card.Value == Value.Dragon));

            Board.MoveHistory.Push(this);
            Board.ApplyForcedMove();
        }

        public override string ToString()
        {
            return $"Lock {Unit.Cards.FirstOrDefault()} from {string.Join(", ", Sources)} in at {Destination}";
        }
    }
}