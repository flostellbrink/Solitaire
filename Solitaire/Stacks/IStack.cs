using System.Collections.Generic;
using Solitaire.Game;

namespace Solitaire.Stacks
{
    public interface IStack
    {
        IEnumerable<Unit> MovableCards { get; }

        bool Accepts(Unit _);

        void Add(Unit unit);

        void Remove(Unit unit);
    }
}