using System.Linq;
using Core;
using Core.Game;
using Core.Stacks;
using Solitaire;
using Xunit;

namespace Test;

public class BoardSerializationTest
{
    public BoardSerializationTest()
    {
        AnsiColorHelper.Enabled = false;
    }

    [Fact]
    public void ParsesPastedSerializationFromRandomBoard()
    {
        var original = new RandomBoard(123456);
        var serialized = original.ToString();

        var parsed = new PastedBoard(serialized);

        Assert.True(parsed.IsValid(null));
        Assert.Equal(original.GetHashCode(), parsed.GetHashCode());
    }

    [Fact]
    public void LockedLockableSlotInfersMissingColor()
    {
        var original = new RandomBoard(424242);
        var firstLockable = original.AllStacks.OfType<LockableStack>().First();
        firstLockable.Locked = true;

        var removed = 0;
        foreach (var stack in original.AllStacks)
            removed += stack.Cards.RemoveAll(card =>
                card.Value == Value.Dragon && card.Color == Color.Red
            );

        Assert.Equal(4, removed);

        foreach (var _ in Enumerable.Range(0, 4))
            firstLockable.Cards.Add(new Card(Color.Red, Value.Dragon));

        Assert.True(original.IsValid(null));

        var serialized = original.ToString();

        var parsed = new PastedBoard(serialized);

        var parsedFirstLockable = parsed.AllStacks.OfType<LockableStack>().First();
        Assert.True(parsedFirstLockable.Locked);
        Assert.Equal(4, parsedFirstLockable.Cards.Count);
        Assert.All(
            parsedFirstLockable.Cards,
            c => Assert.Equal(new Card(Color.Red, Value.Dragon), c)
        );
    }
}
