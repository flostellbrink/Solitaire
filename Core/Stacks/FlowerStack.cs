using System.Linq;
using Core.Game;

namespace Core.Stacks;

public class FlowerStack : AbstractStack
{
    public FlowerStack() { }

    public FlowerStack(FlowerStack flowerStack) : base(flowerStack) { }

    public override int MovableCards => 0;

    public override bool Accepts(Card card, int count) =>
        !Cards.Any() && count == 1 && card.Color == Color.Flower && card.Value == Value.Flower;

    public override string ToString() => "Flower stack";
}
