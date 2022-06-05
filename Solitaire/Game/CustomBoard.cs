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
            Enum.GetValues(typeof(Value)).Cast<Value>().Except(new[] { Value.None, Value.Flower }).ToArray();

        private static readonly Value[] NumericValuesAndNone =
            { Value.None, Value.N1, Value.N2, Value.N3, Value.N4, Value.N5, Value.N6, Value.N7, Value.N8, Value.N9 };

        public CustomBoard() : base()
        {
            foreach (var lockableStack in LockableStacks)
                PopulateLockable(lockableStack);

            PopulateFlower(FlowerStack);

            for (var i = 0; i < 3; i++)
                PopulateFiling(FilingStacks.ElementAt(i), Color.Black + i);

            foreach (var stack in Stacks)
                PopulateStack(stack);
        }

        private static void PopulateLockable(LockableStack stack)
        {
            Console.WriteLine($"Cards for {stack}");
            if ($"Is {stack} locked in? {{0}}".AskForDecision(CliHelper.Yes, CliHelper.No) == CliHelper.Yes)
            {
                var color = "What color are the dragons? {0}".AskForDecision(Card.BaseColors);
                foreach (var _ in Enumerable.Range(0, 4))
                    stack.Cards.Add(Print(new Card(color, Value.Dragon)));
                stack.Locked = true;
            }
            else
            {
                var card = AskForCard();
                if (card != null) stack.Cards.Add(Print(card));
            }

            Console.WriteLine();
        }

        private static void PopulateFlower(FlowerStack stack)
        {
            Console.WriteLine($"Cards for {stack}");
            if ("Is the flower on its stack? {0}".AskForDecision(CliHelper.Yes, CliHelper.No) == CliHelper.Yes)
                stack.Cards.Add(Print(new Card(Color.Flower, Value.Flower)));
            Console.WriteLine();
        }

        private static void PopulateFiling(FilingStack stack, Color color)
        {
            Console.WriteLine($"Cards for {stack}");
            var value = $"What is the highest value of {stack} {color} {{0}}".AskForDecision(NumericValuesAndNone);

            foreach (var numeric in Card.NumericValues.TakeWhile(numeric => numeric <= value))
                stack.Cards.Add(Print(new Card(color, numeric)));
            Console.WriteLine();
        }

        private static void PopulateStack(Stack stack)
        {
            Console.WriteLine($"Cards for {stack}");
            while (true)
            {
                var card = AskForCard();
                if (card == null) break;
                stack.Cards.Add(Print(card));
            }

            Console.WriteLine();
        }

        private static Card AskForCard()
        {
            var color = "What is the card\'s color? {0}".AskForDecision(Colors);
            if (color == Color.None) return null;
            if (color == Color.Flower) return new Card(color, Value.Flower);

            var value = "What is the card\'s value? {0}".AskForDecision(Values);
            return new Card(color, value);
        }

        private static Card Print(Card card)
        {
            Console.WriteLine(card);
            return card;
        }
    }
}