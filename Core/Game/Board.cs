using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Core.Moves;
using Core.Stacks;

namespace Core.Game;

public class Board
{
    internal const int StackCount = 8;

    internal const int SymbolsPerColor = 4;

    public Stack<IMove> MoveHistory { get; }
    protected ICollection<LockableStack> LockableStacks { get; }
    protected FlowerStack FlowerStack { get; }
    protected ICollection<FilingStack> FilingStacks { get; }
    protected ICollection<Stack> Stacks { get; }
    public List<AbstractStack> AllStacks { get; }

    protected Board()
    {
        MoveHistory = new();
        LockableStacks = Card.BaseColors
            .Select((_, index) => new LockableStack(index + 1))
            .ToList();
        FlowerStack = new();
        FilingStacks = Card.BaseColors.Select((_, index) => new FilingStack(index)).ToList();
        Stacks = Enumerable.Range(1, StackCount).Select(index => new Stack(index)).ToList();

        AllStacks = Enumerable
            .Empty<AbstractStack>()
            .Concat(LockableStacks)
            .Append(FlowerStack)
            .Concat(FilingStacks)
            .Concat(Stacks)
            .ToList();
    }

    public Board(Board board)
    {
        MoveHistory = new Stack<IMove>(board.MoveHistory.Reverse());
        LockableStacks = board.LockableStacks
            .Select(lockable => new LockableStack(lockable))
            .ToList();
        FlowerStack = new FlowerStack(board.FlowerStack);
        FilingStacks = board.FilingStacks.Select(filing => new FilingStack(filing)).ToList();
        Stacks = board.Stacks.Select(stack => new Stack(stack)).ToList();

        AllStacks = Enumerable
            .Empty<AbstractStack>()
            .Concat(LockableStacks)
            .Append(FlowerStack)
            .Concat(FilingStacks)
            .Concat(Stacks)
            .ToList();

        Debug.Assert(GetHashCode() == board.GetHashCode());
    }

    public bool Solved => Stacks.All(stack => !stack.Cards.Any());

    public bool HardModeValid => FilingStacks.All(stack => !stack.Cards.Any()) || Solved;

    public bool IsValid()
    {
        var abstractStacks = Enumerable
            .Empty<AbstractStack>()
            .Concat(LockableStacks)
            .Append(FlowerStack)
            .Concat(FilingStacks)
            .Concat(Stacks);
        var cardsOnBoard = abstractStacks.SelectMany(stack => stack.Cards).ToList();

        var missingCards = Card.FullSet.ExceptQuantitative(cardsOnBoard).ToList();
        if (missingCards.Any())
        {
            Console.Error.WriteLine(
                $"The following cards are missing on this board: {string.Join(", ", missingCards)}"
            );
        }

        var extraCards = cardsOnBoard.ExceptQuantitative(Card.FullSet).ToList();
        if (extraCards.Any())
        {
            Console.Error.WriteLine(
                $"The following cards should not be on this board: {string.Join(", ", extraCards)}"
            );
        }

        return !extraCards.Any() && !missingCards.Any();
    }

    public int Loss =>
        MoveHistory.Count
        + Stacks.Sum(stack => stack.Cards.Count)
        + (Stacks.Sum(stack => stack.Cards.Count - stack.MovableCards.Count()) * 2)
        + LockableStacks.Count(locked => locked.Cards.Any(card => card.Value != Value.Dragon))
        + (LockableStacks.Count(locked => !locked.Locked) * 100);

    private IEnumerable<Move> AllStandardMoves =>
        AllStacks.SelectMany(
            (source, sourceIndex) =>
                source.MovableCards.SelectMany(
                    unit =>
                        AllStacks
                            .Select((stack, index) => (stack, index))
                            .Where(destination => destination.index != sourceIndex)
                            .Where(destination => destination.stack.Accepts(unit))
                            .Select(
                                destination =>
                                    new Move(sourceIndex, destination.index, unit.Cards.Count)
                            )
                )
        );

    private readonly static List<Unit> LockUnits = Card.BaseColors
        .Select(color => new Unit(new List<Card> { new Card(color, Value.Dragon) }))
        .ToList();

    private IEnumerable<LockMove> AllLockMoves()
    {
        var lockableStacks = LockableStacks
            .Select((stack, index) => (stack, index))
            .Where(stack => !stack.stack.Locked);

        foreach (var unit in LockUnits)
        {
            var sources = AllStacks
                .Select((stack, index) => (stack, index))
                .Where(stack => stack.stack.MovableCards.Contains(unit))
                .Select(stack => stack.index)
                .ToList();

            if (sources.Count != SymbolsPerColor)
                continue;

            var targets = lockableStacks
                .Where(target => target.stack.Accepts(unit))
                .Select(target => target.index);

            foreach (var target in targets)
                yield return new LockMove(sources, target, unit.Cards.Count);
        }
    }

    public IEnumerable<IMove> AllMoves =>
        Enumerable.Empty<IMove>().Concat(AllStandardMoves).Concat(AllLockMoves());

    // Moves that create distinct boards (e.g. no difference onto which empty stack a card is moved)
    public IEnumerable<IMove> DistinctMoves =>
        AllMoves.DistinctBy(
            move =>
            {
                var clone = new Board(this);
                move.Apply(clone);
                return clone.GetHashCode();
            },
            GetHashCode()
        );

    public Value HighestAutomaticFilingValue => FilingStacks.Min(filing => filing.NextIndex + 1);

    public void ApplyForcedMoves()
    {
        while (AllMoves.FirstOrDefault(move => move.IsForced(this)) is { } forcedMove)
            forcedMove.Apply(this);
    }

    public override string ToString()
    {
        const int maxStackWidth = 13;
        const string separator = "|";
        var builder = new StringBuilder();

        // Header top
        builder.BeginColor(AnsiColor.BackgroundYellow);
        builder.AppendJoinPadded(
            separator,
            maxStackWidth,
            Enumerable
                .Empty<AbstractStack>()
                .Concat(LockableStacks)
                .Append(FlowerStack)
                .Concat(FilingStacks)
        );
        builder.EndColor();
        builder.AppendLine();

        // Values top
        builder.AppendJoinPadded(
            separator,
            maxStackWidth,
            LockableStacks
                .Select(
                    stack =>
                        stack.Locked
                            ? "Locked"
                            : stack.Cards.LastOrDefault()?.ToString() ?? string.Empty
                )
                .Concat(
                    Enumerable
                        .Empty<AbstractStack>()
                        .Append(FlowerStack)
                        .Concat(FilingStacks)
                        .Select(stack => stack.Cards.LastOrDefault()?.ToString() ?? string.Empty)
                )
        );
        builder.AppendLine();

        // Header bottom
        builder.BeginColor(AnsiColor.BackgroundYellow);
        builder.AppendJoinPadded(separator, maxStackWidth, Stacks);
        builder.EndColor();
        builder.AppendLine();

        // Values bottom
        for (var index = 0; index < Stacks.Max(stack => stack.Cards.Count); index++)
        {
            var indexLocal = index;
            var cards = Stacks.Select(stack => stack.Cards.Skip(indexLocal).FirstOrDefault());
            builder.AppendJoinPadded(separator, maxStackWidth, cards);
            builder.AppendLine();
        }

        return builder.ToString();
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (LockableStacks?.GetCollectionHashCodeUnordered()) ?? 0;
            hashCode = (hashCode * 397) ^ ((FlowerStack?.GetHashCode()) ?? 0);
            hashCode = (hashCode * 397) ^ ((FilingStacks?.GetCollectionHashCode()) ?? 0);
            hashCode = (hashCode * 397) ^ ((Stacks?.GetCollectionHashCodeUnordered()) ?? 0);
            return hashCode;
        }
    }
}
