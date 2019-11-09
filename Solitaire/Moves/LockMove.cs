using System.Collections.Generic;
using Solitaire.Game;
using Solitaire.Stacks;

namespace Solitaire.Moves
{
    internal class LockMove : IMove
    {
        public readonly ICollection<IStack> Sources;

        public readonly LockableStack Destination;

        public readonly Unit Unit;

        public LockMove(ICollection<IStack> sources, LockableStack destination, Unit unit)
        {
            Sources = sources;
            Destination = destination;
            Unit = unit;
        }

        public bool IsForced(Board board) => false;

        public void Apply(Board board)
        {
            foreach (var source in Sources)
            {
                source.Remove(Unit);
                Destination.Add(Unit);
            }
            board.ApplyForcedMove();
        }
    }
}