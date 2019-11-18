using System;
using System.Linq;

namespace Solitaire
{
    public static class CliHelper
    {
        private static ConsoleKey CharacterToConsoleKey(char character)
        {
            var value = character.ToString();
            if (int.TryParse(value, out var _)) value = $"D{value}";
            return Enum.Parse<ConsoleKey>(value);
        }

        public static T AskForDecision<T>(this string format, params T[] values)
        {
            var valueStrings = values.Select(value => value.ToDescription()).ToList();
            var options = valueStrings.Select(value => $"({value[0]}){value.Substring(1)}");
            var question = string.Format(format, string.Join(", ", options));
            var keys = valueStrings.Select(value => CharacterToConsoleKey(value[0])).ToArray();
            var key = question.AskForDecision(keys);
            return values[keys.ToList().IndexOf(key)];
        }

        public static ConsoleKey AskForDecision(this string question, params ConsoleKey[] validAnswers)
        {
            Console.Write(question);
            while (true)
            {
                var decision = Console.ReadKey(true).Key;
                Console.Write("\r".PadRight(Console.WindowWidth));
                Console.Write("\r");
                if (validAnswers.Contains(decision)) return decision;
                Console.Write($"{question} - {decision} is not part of {string.Join(", ", validAnswers)}");
            }
        }

        public static T AskForIndexedDecision<T>(this string question, params T[] values)
        {
            var maxWidth = values.Length.ToString().Length;
            var numberedList = values.Select((value, index) => $"{(index + 1).ToString().PadRight(maxWidth)}: {value}");
            Console.WriteLine(string.Join("\n", numberedList));
            Console.WriteLine();

            var key = question.AskForNumber(1, values.Length);
            return values[key - 1];
        }

        public static int AskForNumber(this string question, int min, int max)
        {
            Console.Write($"{question} ({min}-{max}): ");
            while (true)
            {
                var line = string.Empty;
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Escape) break; ;
                    line += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
                Console.Write("\r".PadRight(Console.WindowWidth));
                Console.Write("\r");
                if (int.TryParse(line, out var index) && index >= min && index <= max) return index;
                Console.Write($"{question} ({min}-{max}) - {line} is not a number between {min} and {max}: ");
            }
        }
    }
}
