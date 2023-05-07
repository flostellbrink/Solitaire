using System.Diagnostics;
using System.Linq;
using Core.Game;
using Core.Stacks;

namespace Core.Moves;

public record Move(int Source, int Destination, int Count) : IMove
{
    public bool IsForced(Board board)
    {
        if (Count != 1)
            return false;

        var card = board.AllStacks[Source].Cards.Last();
        var destination = board.AllStacks[Destination];
        return destination switch
        {
            FlowerStack _ when card.Color == Color.Flower && card.Value == Value.Flower => true,
            FilingStack _ when board.HighestAutomaticFilingValue >= card.Value => true,
            _ => false
        };
    }

    public void Apply(Board board)
    {
        var source = board.AllStacks[Source];
        var destination = board.AllStacks[Destination];

        Debug.Assert(
            destination.Accepts(source.Cards[^Count], Count),
            $"Cannot move {source.Cards.Last()} to {destination}.\n{board}"
        );
        destination.Add(source.Cards.Skip(source.Cards.Count - Count));
        source.Remove(Count);

        board.MoveHistory.Push(this);
    }

    public void Undo(Board board)
    {
        var source = board.AllStacks[Source];
        var destination = board.AllStacks[Destination];

        source.Add(destination.Cards.Skip(destination.Cards.Count - Count));
        destination.Remove(Count);

        var popped = board.MoveHistory.Pop();
        Debug.Assert(
            popped == (IMove)this,
            $"Expected {ToString(board)}, got {popped.ToString(board)}."
        );
    }

    public string ToString(Board board)
    {
        var source = board.AllStacks[Source];
        var destination = board.AllStacks[Destination];
        return $"Move {string.Join(", ", source.Cards.Skip(source.Cards.Count - Count))} from {source} to {destination}";
    }
}
