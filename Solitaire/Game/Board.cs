using System;
using System.Collections.Generic;
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

        public Board()
        {
            var random = new Random();
            var deck = Card.FullSet.OrderBy(_ => random.Next()).ToList();

            var stackIndex = 0;
            foreach (var card in deck)
            {
                _stacks.ElementAt(stackIndex++ % _stacks.Count).Cards.Add(card);
            }

            ApplyForcedMove();
        }

        private readonly ICollection<LockableStack> _lockableStacks =
            Card.BaseColors.Select(index => new LockableStack((int)index)).ToList();

        private readonly FlowerStack _flowerStack = new FlowerStack();

        private readonly ICollection<FilingStack> _filingStacks =
            Card.BaseColors.Select(color => new FilingStack(color)).ToList();

        private readonly ICollection<Stack> _stacks =
            Enumerable.Range(0, StackCount).Select(index => new Stack(index)).ToList();

        public IEnumerable<IStack> AllStacks => Enumerable.Empty<IStack>()
            .Concat(_lockableStacks).Append(_flowerStack).Concat(_filingStacks).Concat(_stacks);

        private IEnumerable<Move> AllStandardMoves => AllStacks
            .SelectMany(source => source.MovableCards
                .SelectMany(unit => AllStacks
                    .Where(destination => destination != source)
                    .Where(destination => destination.Accepts(unit))
                    .Select(destination => new Move(source, destination, unit))));

        private IEnumerable<LockMove> AllLockMoves => Card.BaseColors
            .Select(color => new Unit(new[] {new Card(color, Value.Symbol)}))
            .Select(unit => new LockMove(
                AllStacks.Where(stack => stack.MovableCards.Contains(unit)).ToList(),
                _lockableStacks.FirstOrDefault(lockable => lockable.Accepts(unit) || lockable.MovableCards.Contains(unit)),
                unit))
            .Where(move => move.Sources.Count == SymbolsPerColor && move.Destination != null);

        public IEnumerable<IMove> AllMoves => Enumerable.Empty<IMove>().Concat(AllStandardMoves).Concat(AllLockMoves);

        public Value MinNextFilingValue => _filingStacks.Min(filing => filing.NextIndex);

        public bool ApplyForcedMove()
        {
            var forcedMove = AllMoves.FirstOrDefault(move => move.IsForced(this));
            if (forcedMove == null) return false;
            forcedMove.Apply(this);
            return true;
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
                Enumerable.Empty<AbstractStack>().Concat(_lockableStacks).Append(_flowerStack).Concat(_filingStacks).Select(stack => stack.Cards.LastOrDefault()));
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
                var hashCode = (_lockableStacks != null ? _lockableStacks.GetCollectionHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_flowerStack != null ? _flowerStack.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_filingStacks != null ? _filingStacks.GetCollectionHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_stacks != null ? _stacks.GetCollectionHashCode() : 0);
                return hashCode;
            }
        }
    }
}