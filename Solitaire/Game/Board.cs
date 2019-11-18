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

        public Stack<IMove> MoveHistory = new Stack<IMove>();

        private readonly ICollection<LockableStack> _lockableStacks =
            Card.BaseColors.Select(index => new LockableStack((int)index + 1)).ToList();

        private readonly FlowerStack _flowerStack = new FlowerStack();

        private readonly ICollection<FilingStack> _filingStacks =
            Card.BaseColors.Select(color => new FilingStack(color)).ToList();

        private protected readonly ICollection<Stack> _stacks =
            Enumerable.Range(1, StackCount).Select(index => new Stack(index)).ToList();

        protected Board(bool applyForcedMoves = true)
        {
            ApplyForcedMoves = applyForcedMoves;
        }

        public Board(Board board)
        {
            ApplyForcedMoves = board.ApplyForcedMoves;
            MoveHistory = new Stack<IMove>(board.MoveHistory.Reverse());
            _lockableStacks = board._lockableStacks.Select(lockable => new LockableStack(lockable)).ToList();
            _flowerStack = new FlowerStack(board._flowerStack);
            _filingStacks = board._filingStacks.Select(filing => new FilingStack(filing)).ToList();
            _stacks = board._stacks.Select(stack => new Stack(stack)).ToList();
            Debug.Assert(GetHashCode() == board.GetHashCode());
        }

        public bool Solved => _stacks.All(stack => !stack.Cards.Any());

        public bool IsValid()
        {
            var abstractStacks = Enumerable.Empty<AbstractStack>()
                .Concat(_lockableStacks).Append(_flowerStack).Concat(_filingStacks).Concat(_stacks);
            var cardsOnBoard = abstractStacks.SelectMany(stack => stack.Cards).ToList();

            var missingCards = Card.FullSet.ExceptQuantitative(cardsOnBoard).ToList();
            if (missingCards.Any())
                Console.Error.WriteLine($"The following cards are missing on this board: {string.Join(", ", missingCards)}");

            var extraCards = cardsOnBoard.ExceptQuantitative(Card.FullSet).ToList();
            if (extraCards.Any())
                Console.Error.WriteLine($"The following cards should not be on this board: {string.Join(", ", missingCards)}");

            return !extraCards.Any() && !missingCards.Any();
        }

        public int Loss =>
            MoveHistory.Count +
            _stacks.Sum(stack => stack.Cards.Count) +
            _stacks.Sum(stack => stack.Cards.Count - stack.MovableCards.Count()) * 2 +
            _lockableStacks.Count(locked => locked.Cards.Any(card => card.Value != Value.Symbol)) +
            _lockableStacks.Count(locked => !locked.Locked) * 100;

        public IEnumerable<IStack> AllStacks => Enumerable.Empty<IStack>()
            .Concat(_lockableStacks).Append(_flowerStack).Concat(_filingStacks).Concat(_stacks);

        private IEnumerable<Move> AllStandardMoves => AllStacks
            .SelectMany(source => source.MovableCards
                .SelectMany(unit => AllStacks
                    .Where(destination => destination != source)
                    .Where(destination => destination.Accepts(unit))
                    .Select(destination => new Move(this, source, destination, unit))));

        private IEnumerable<LockMove> AllLockMoves => Card.BaseColors
            .Select(color => new Unit(new[] {new Card(color, Value.Symbol)}))
            .Select(unit => new LockMove(
                this,
                AllStacks.Where(stack => stack.MovableCards.Contains(unit)).ToList(),
                _lockableStacks.FirstOrDefault(lockable => lockable.Accepts(unit) || lockable.MovableCards.Contains(unit)),
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

        public Value HighestAutomaticFilingValue => _filingStacks.Min(filing => filing.NextIndex + 1);

        public void ApplyForcedMove()
        {
            if (!ApplyForcedMoves) return;
            var forcedMove = AllMoves.FirstOrDefault(move => move.IsForced());
            forcedMove?.Apply();
        }

        public override string ToString()
        {
            const int maxStackWidth = 13;
            var builder = new StringBuilder();

            // Header top
            builder.Append("\u001b[43m");
            builder.AppendJoinPadded("|", maxStackWidth,
                Enumerable.Empty<IStack>().Concat(_lockableStacks).Append(_flowerStack).Concat(_filingStacks));
            builder.AppendLine("\u001b[0m");

            // Values top
            builder.AppendJoinPadded("|", maxStackWidth,
                _lockableStacks.Select(stack =>
                        stack.Locked ? "Locked" : stack.Cards.LastOrDefault()?.ToString() ?? string.Empty)
                    .Concat(Enumerable.Empty<AbstractStack>().Append(_flowerStack).Concat(_filingStacks)
                        .Select(stack => stack.Cards.LastOrDefault()?.ToString() ?? string.Empty)));
            builder.AppendLine();

            // Header bottom
            builder.AppendLine("\u001b[43m");
            builder.AppendJoinPadded("|", maxStackWidth, _stacks);
            builder.AppendLine("\u001b[0m");

            // Values bottom
            for (var index = 0; index < _stacks.Max(stack => stack.Cards.Count); index++)
            {
                var indexLocal = index;
                var cards = _stacks.Select(stack => stack.Cards.Skip(indexLocal).FirstOrDefault());
                builder.AppendJoinPadded("|", maxStackWidth, cards);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_lockableStacks != null ? _lockableStacks.GetCollectionHashCodeUnordered() : 0);
                hashCode = (hashCode * 397) ^ (_flowerStack != null ? _flowerStack.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_filingStacks != null ? _filingStacks.GetCollectionHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_stacks != null ? _stacks.GetCollectionHashCodeUnordered() : 0);
                return hashCode;
            }
        }
    }
}