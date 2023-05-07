using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Game;
using Core.Stacks;

namespace Core.Moves;

public record LockMove(ICollection<int> Sources, int Destination) : IMove
{
    public bool IsForced(Board board) => false;

    public void Apply(Board board)
    {
        var sources = Sources.Select(source => board.AllStacks[source]);
        var destination = (LockableStack)board.AllStacks[Destination];
        var card = sources.First().Cards.Last();

        Debug.Assert(destination.Cards.Count <= 1);
        Debug.Assert(destination.Cards.All(card => card.Color == card.Color));
        Debug.Assert(destination.Cards.All(card => card.Value == Value.Dragon));

        foreach (var source in sources)
        {
            source.Remove(1);
            destination.Add(card);
        }

        destination.Locked = true;

        Debug.Assert(destination.Cards.Count == 4);
        Debug.Assert(destination.Cards.All(card => card.Color == card.Color));
        Debug.Assert(destination.Cards.All(card => card.Value == Value.Dragon));

        board.MoveHistory.Push(this);
    }

    public void Undo(Board board)
    {
        var sources = Sources.Select(source => board.AllStacks[source]);
        var destination = (LockableStack)board.AllStacks[Destination];
        var card = destination.Cards.Last();

        Debug.Assert(destination.Cards.Count == 4);
        Debug.Assert(destination.Cards.All(card => card.Color == card.Color));
        Debug.Assert(destination.Cards.All(card => card.Value == Value.Dragon));

        foreach (var source in sources)
        {
            source.Add(card);
            destination.Remove(1);
        }

        destination.Locked = false;

        Debug.Assert(destination.Cards.Count <= 1);
        Debug.Assert(destination.Cards.All(card => card.Color == card.Color));
        Debug.Assert(destination.Cards.All(card => card.Value == Value.Dragon));

        var popped = board.MoveHistory.Pop();
        Debug.Assert(
            popped == (IMove)this,
            $"Expected {ToString(board)}, got {popped.ToString(board)}."
        );
    }

    public string ToString(Board board)
    {
        var card = board.AllStacks[Sources.First()].Cards.Last();
        var sources = Sources.Select(source => board.AllStacks[source]);
        var destination = board.AllStacks[Destination];
        return $"Lock {card} from {string.Join(", ", sources)} in at {destination}";
    }
}
