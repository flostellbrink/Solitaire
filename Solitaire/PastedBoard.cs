using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Game;

namespace Solitaire;

public class PastedBoard : Board
{
    public PastedBoard(string serializedBoardText)
        : base()
    {
        if (string.IsNullOrWhiteSpace(serializedBoardText))
            return;

        foreach (var lockable in LockableStacks)
        {
            lockable.Cards.Clear();
            lockable.Locked = false;
        }

        FlowerStack.Cards.Clear();
        foreach (var filing in FilingStacks)
            filing.Cards.Clear();
        foreach (var stack in Stacks)
            stack.Cards.Clear();

        var lines = serializedBoardText
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Split('\n')
            .Select(line => line.StripAnsi())
            .Select(line => line.TrimEnd())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var headerTopIndex = lines.FindIndex(line =>
            line.Contains("Lockable", StringComparison.OrdinalIgnoreCase) && line.Contains('|')
        );
        if (headerTopIndex < 0 || headerTopIndex + 2 >= lines.Count)
            return;

        var valuesTop = lines[headerTopIndex + 1];
        var headerBottom = lines[headerTopIndex + 2];

        var topCells = valuesTop.Split('|').Select(cell => cell.Trim()).ToArray();
        if (topCells.Length != 7)
            throw new FormatException($"Expected 7 columns in top section, got {topCells.Length}.");

        var bottomHeaderCells = headerBottom.Split('|').Select(cell => cell.Trim()).ToArray();
        if (bottomHeaderCells.Length != 8)
            throw new FormatException(
                $"Expected 8 columns in bottom section header, got {bottomHeaderCells.Length}."
            );

        var lockedLockableIndices = new List<int>();
        for (var i = 0; i < LockableStacks.Count; i++)
        {
            var cell = topCells[i];
            var lockable = LockableStacks.ElementAt(i);

            if (cell.Equals("Locked", StringComparison.OrdinalIgnoreCase))
            {
                lockable.Locked = true;
                lockedLockableIndices.Add(i);
            }
            else if (!string.IsNullOrWhiteSpace(cell))
            {
                lockable.Cards.Add(ParseCardCell(cell));
            }
        }

        var flowerCell = topCells[3];
        if (!string.IsNullOrWhiteSpace(flowerCell))
            FlowerStack.Cards.Add(ParseCardCell(flowerCell));

        for (var i = 0; i < FilingStacks.Count; i++)
        {
            var cell = topCells[4 + i];
            if (string.IsNullOrWhiteSpace(cell))
                continue;

            var topCard = ParseCardCell(cell);
            if (!Card.NumericValues.Contains(topCard.Value))
                throw new FormatException(
                    $"Filing stack {i} top card must be numeric, got {topCard}."
                );

            foreach (
                var numeric in Card.NumericValues.OrderBy(v => v).TakeWhile(v => v <= topCard.Value)
            )
                FilingStacks.ElementAt(i).Cards.Add(new Card(topCard.Color, numeric));
        }

        var bottomValueLines = lines.Skip(headerTopIndex + 3).ToList();
        foreach (var line in bottomValueLines)
        {
            var cells = line.Split('|').Select(cell => cell.Trim()).ToArray();
            if (cells.Length != 8)
                throw new FormatException($"Expected 8 columns in stack rows, got {cells.Length}.");

            for (var stackIndex = 0; stackIndex < Stacks.Count; stackIndex++)
            {
                var cell = cells[stackIndex];
                if (string.IsNullOrWhiteSpace(cell))
                    continue;
                Stacks.ElementAt(stackIndex).Cards.Add(ParseCardCell(cell));
            }
        }

        if (lockedLockableIndices.Count == 0)
            return;

        var allDragons = Stacks
            .SelectMany(stack => stack.Cards)
            .Concat(LockableStacks.SelectMany(stack => stack.Cards))
            .Where(card => card.Value == Value.Dragon)
            .ToList();

        var baseColors = new[] { Color.Red, Color.Green, Color.Black };
        var dragonCountByColor = baseColors.ToDictionary(
            color => color,
            color => allDragons.Count(dragon => dragon.Color == color)
        );

        var invalidCounts = dragonCountByColor
            .Where(entry => entry.Value != 0 && entry.Value != 4)
            .Select(entry => $"{entry.Key}:{entry.Value}")
            .ToList();
        if (invalidCounts.Count != 0)
            return;

        var colorsWithMissingDragons = dragonCountByColor
            .Where(entry => entry.Value == 0)
            .Select(entry => entry.Key)
            .ToList();

        if (colorsWithMissingDragons.Count != lockedLockableIndices.Count)
            return;

        foreach (var (color, lockableIndex) in colorsWithMissingDragons.Zip(lockedLockableIndices))
        {
            var lockable = LockableStacks.ElementAt(lockableIndex);
            foreach (var _ in Enumerable.Range(0, 4))
                lockable.Cards.Add(new Card(color, Value.Dragon));
        }
    }

    private static Card ParseCardCell(string cell)
    {
        var trimmed = cell.Trim();
        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            throw new FormatException($"Invalid card cell: '{cell}'");

        var colorString = parts[0];
        var valueString = parts[^1];

        var color = Enum.Parse<Color>(colorString, ignoreCase: true);
        var value = Enum.GetValues<Value>()
            .Cast<Value>()
            .Single(v => v.ToDescription().Equals(valueString, StringComparison.OrdinalIgnoreCase));

        return new Card(color, value);
    }
}
