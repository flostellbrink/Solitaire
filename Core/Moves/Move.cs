using System.Diagnostics;
using System.Linq;
using Core.Game;
using Core.Stacks;

namespace Core.Moves;

internal class Move : IMove
{
    public readonly int Source;

    public readonly int Destination;

    public readonly int Count;

    public Move(int source, int destination, int count)
    {
        Source = source;
        Destination = destination;
        Count = count;
    }

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
        var unit = new Unit(source.Cards.Skip(source.Cards.Count - Count).ToList());

        source.Remove(unit);
        Debug.Assert(
            destination.Accepts(unit),
            $"Destination {destination} does not accept {unit}"
        );
        destination.Add(unit);

        board.MoveHistory.Push(this);
        board.ApplyForcedMoves();
    }

    public string Stringify(Board board)
    {
        var source = board.AllStacks[Source];
        var destination = board.AllStacks[Destination];
        var unit = new Unit(source.Cards.Skip(source.Cards.Count - Count).ToList());
        return $"Move {unit} from {source} to {destination}";
    }
}
