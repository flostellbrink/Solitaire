using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Game;
using Core.Stacks;

namespace Core.Moves;

internal class LockMove : IMove
{
    public readonly ICollection<int> Sources;

    public readonly int Destination;

    public readonly int Count;

    public LockMove(ICollection<int> sources, int destination, int count)
    {
        Sources = sources;
        Destination = destination;
        Count = count;
    }

    public bool IsForced(Board board) => false;

    public void Apply(Board board)
    {
        var sources = Sources.Select(source => board.AllStacks[source]);
        var destination = (LockableStack)board.AllStacks[Destination];
        var card = sources.First().Cards.Last();
        var unit = new Unit(new List<Card> { card });

        Debug.Assert(destination.Cards.Count <= 1);
        Debug.Assert(destination.Cards.All(card => card.Color == card.Color));
        Debug.Assert(destination.Cards.All(card => card.Value == Value.Dragon));

        foreach (var source in sources)
        {
            source.Remove(unit);
            destination.Add(unit);
        }

        destination.Locked = true;

        Debug.Assert(destination.Cards.Count == 4);
        Debug.Assert(destination.Cards.All(card => card.Color == card.Color));
        Debug.Assert(destination.Cards.All(card => card.Value == Value.Dragon));

        board.MoveHistory.Push(this);
    }

    public string Stringify(Board board)
    {
        var card = board.AllStacks[Sources.First()].Cards.Last();
        var sources = Sources.Select(source => board.AllStacks[source]);
        var destination = board.AllStacks[Destination];
        return $"Lock {card} from {string.Join(", ", sources)} in at {destination}";
    }
}
