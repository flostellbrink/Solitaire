using System.Collections.Generic;
using Core.Game;

namespace Core.Stacks;

public interface IStack
{
    IEnumerable<Unit> MovableCards { get; }

    bool Accepts(Unit unit);

    void Add(Unit unit);

    void Remove(Unit unit);
}
