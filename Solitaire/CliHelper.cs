using System;
using System.Linq;

namespace Solitaire
{
    public static class CliHelper
    {
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
