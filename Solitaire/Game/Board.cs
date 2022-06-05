using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Solitaire.Moves;
using Solitaire.Stacks;

namespace Solitaire.Game
{
    public class Board
    {
        internal const int StackCount = 8;

        internal const int SymbolsPerColor = 4;

        public bool ApplyForcedMoves;

        public Stack<IMove> MoveHistory = new();

        protected readonly ICollection<LockableStack> LockableStacks =
            Card.BaseColors.Select((_, index) => new LockableStack(index + 1)).ToList();

        protected readonly FlowerStack FlowerStack = new();

        protected readonly ICollection<FilingStack> FilingStacks =
            Card.BaseColors.Select((_, index) => new FilingStack(index)).ToList();

        protected readonly ICollection<Stack> Stacks =
            Enumerable.Range(1, StackCount).Select(index => new Stack(index)).ToList();

        protected Board(bool applyForcedMoves = true)
        {
            ApplyForcedMoves = applyForcedMoves;
        }

        public Board(Board board)
        {
            ApplyForcedMoves = board.ApplyForcedMoves;
            MoveHistory = new Stack<IMove>(board.MoveHistory.Reverse());
            LockableStacks = board.LockableStacks.Select(lockable => new LockableStack(lockable)).ToList();
            FlowerStack = new FlowerStack(board.FlowerStack);
            FilingStacks = board.FilingStacks.Select(filing => new FilingStack(filing)).ToList();
            Stacks = board.Stacks.Select(stack => new Stack(stack)).ToList();
            Debug.Assert(GetHashCode() == board.GetHashCode());
        }

        public bool Solved => Stacks.All(stack => !stack.Cards.Any());

        public bool HardModeValid => FilingStacks.All(stack => !stack.Cards.Any()) || Solved;

        public bool IsValid()
        {
            var abstractStacks = Enumerable.Empty<AbstractStack>()
                .Concat(LockableStacks).Append(FlowerStack).Concat(FilingStacks).Concat(Stacks);
            var cardsOnBoard = abstractStacks.SelectMany(stack => stack.Cards).ToList();

            var missingCards = Card.FullSet.ExceptQuantitative(cardsOnBoard).ToList();
            if (missingCards.Any())
                Console.Error.WriteLine(
                    $"The following cards are missing on this board: {string.Join(", ", missingCards)}");

            var extraCards = cardsOnBoard.ExceptQuantitative(Card.FullSet).ToList();
            if (extraCards.Any())
                Console.Error.WriteLine(
                    $"The following cards should not be on this board: {string.Join(", ", extraCards)}");

            return !extraCards.Any() && !missingCards.Any();
        }

        public int Loss =>
            MoveHistory.Count +
            Stacks.Sum(stack => stack.Cards.Count) +
            Stacks.Sum(stack => stack.Cards.Count - stack.MovableCards.Count()) * 2 +
            LockableStacks.Count(locked => locked.Cards.Any(card => card.Value != Value.Dragon)) +
            LockableStacks.Count(locked => !locked.Locked) * 100;

        public IEnumerable<IStack> AllStacks => Enumerable.Empty<IStack>()
            .Concat(LockableStacks).Append(FlowerStack).Concat(FilingStacks).Concat(Stacks);

        private IEnumerable<Move> AllStandardMoves => AllStacks
            .SelectMany(source => source.MovableCards
                .SelectMany(unit => AllStacks
                    .Where(destination => destination != source)
                    .Where(destination => destination.Accepts(unit))
                    .Select(destination => new Move(this, source, destination, unit))));

        private IEnumerable<LockMove> AllLockMoves => Card.BaseColors
            .Select(color => new Unit(new[] { new Card(color, Value.Dragon) }))
            .Select(unit => new LockMove(
                this,
                AllStacks.Where(stack => stack.MovableCards.Contains(unit)).ToList(),
                LockableStacks.FirstOrDefault(
                    lockable => lockable.Accepts(unit) || lockable.MovableCards.Contains(unit)),
                unit))
            .Where(move => move.Sources.Count == SymbolsPerColor && move.Destination != null);

        public IEnumerable<IMove> AllMoves => Enumerable.Empty<IMove>().Concat(AllStandardMoves).Concat(AllLockMoves);

        // Moves that create distinct boards (e.g. no difference onto which empty stack a card is moved)
        public IEnumerable<IMove> DistinctMoves => AllMoves.DistinctBy(move =>
        {
            var clone = new Board(this);
            move.Clone(clone).Apply();
            return clone.GetHashCode();
        }, this.GetHashCode());

        public Value HighestAutomaticFilingValue => FilingStacks.Min(filing => filing.NextIndex + 1);

        public void ApplyForcedMove()
        {
            if (!ApplyForcedMoves) return;
            var forcedMove = AllMoves.FirstOrDefault(move => move.IsForced());
            forcedMove?.Apply();
        }

        public override string ToString()
        {
            const int maxStackWidth = 13;
            const string separator = "|";
            var builder = new StringBuilder();

            // Header top
            builder.BeginColor(AnsiColor.BackgroundYellow);
            builder.AppendJoinPadded(separator, maxStackWidth,
                Enumerable.Empty<IStack>().Concat(LockableStacks).Append(FlowerStack).Concat(FilingStacks));
            builder.EndColor();
            builder.AppendLine();

            // Values top
            builder.AppendJoinPadded(separator, maxStackWidth,
                LockableStacks.Select(stack =>
                        stack.Locked ? "Locked" : stack.Cards.LastOrDefault()?.ToString() ?? string.Empty)
                    .Concat(Enumerable.Empty<AbstractStack>().Append(FlowerStack).Concat(FilingStacks)
                        .Select(stack => stack.Cards.LastOrDefault()?.ToString() ?? string.Empty)));
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
                var hashCode = (LockableStacks != null ? LockableStacks.GetCollectionHashCodeUnordered() : 0);
                hashCode = (hashCode * 397) ^ (FlowerStack != null ? FlowerStack.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FilingStacks != null ? FilingStacks.GetCollectionHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Stacks != null ? Stacks.GetCollectionHashCodeUnordered() : 0);
                return hashCode;
            }
        }
    }
}