using System;
using System.Linq;
using Solitaire.Stacks;

namespace Solitaire.Game
{
    public class CustomBoard : Board
    {
        private static readonly Color[] Colors =
            Enum.GetValues(typeof(Color)).Cast<Color>().ToArray();

        private static readonly Value[] Values =
            Enum.GetValues(typeof(Value)).Cast<Value>().Except(new[] {Value.Flower}).ToArray();

        private static readonly Value[] NumericValuesAndNone =
            {Value.None, Value.N1, Value.N2, Value.N3, Value.N4, Value.N5, Value.N6, Value.N7, Value.N8, Value.N9};

        public CustomBoard(bool applyForcedMoves = true) : base(applyForcedMoves)
        {
            foreach (var lockableStack in LockableStacks)
                PopulateSingle(lockableStack);

            const string yes = "Yes";
            const string no = "No";
            if ("Is the flower on its stack? {0}".AskForDecision(yes, no) == yes)
                FlowerStack.Cards.Add(Print(new Card(Color.Flower, Value.Flower), FlowerStack));

            foreach (var filingStack in FilingStacks)
                PopulateFiling(filingStack);

            foreach (var stack in Stacks)
                PopulateStack(stack);
        }

        private static void PopulateSingle(AbstractStack stack)
        {
            Console.WriteLine($"Choose card for {stack}");
            var card = AskForCard();
            if (card != null) stack.Cards.Add(Print(card, stack));
        }

        private static void PopulateFiling(FilingStack stack)
        {
            var value = $"Chose highest value of {stack} {{0}}".AskForDecision(NumericValuesAndNone);
            if (value == Value.None) return;

            foreach (var numeric in Card.NumericValues.TakeWhile(numeric => numeric != value))
                stack.Cards.Add(Print(new Card(stack.Color, numeric), stack));
        }

        private static void PopulateStack(Stack stack)
        {
            Console.WriteLine($"Choose card for {stack}");
            while (true)
            {
                var card = AskForCard();
                if (card == null) break;
                stack.Cards.Add(Print(card, stack));
            }
        }

        private static Card AskForCard()
        {
            var color = "Chose a color from {0}".AskForDecision(Colors);
            if (color == Color.None) return null;
            if (color == Color.Flower) return new Card(color, Value.Flower);

            var value = "Chose a value from {0}".AskForDecision(Values);
            return new Card(color, value);
        }

        private static Card Print(Card card, IStack stack)
        {
            Console.WriteLine($"Adding {card} to {stack}");
            return card;
        }
    }
}
