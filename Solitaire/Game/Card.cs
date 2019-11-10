using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.Game
{
    public enum Color { Black, Green, Red, Flower }

    public enum Value { N1, N2, N3, N4, N5, N6, N7, N8, N9, Symbol, Flower }

    public class Card : IEquatable<Card>
    {
        public readonly Color Color;

        public readonly Value Value;

        public Card(Color color, Value value)
        {
            Color = color;
            Value = value;
        }

        internal static readonly Color[] BaseColors =
            {Color.Black, Color.Green, Color.Red};

        internal static readonly Value[] BaseValues =
            {Value.N1, Value.N2, Value.N3, Value.N4, Value.N5, Value.N6, Value.N7, Value.N8, Value.N9};

        public static IEnumerable<Card> FullSet => BaseColors
            .SelectMany(color => BaseValues.Select(value => new Card(color, value)))
            .Concat(BaseColors.SelectMany(color => Enumerable.Range(0, 4).Select(_ => new Card(color, Value.Symbol))))
            .Append(new Card(Color.Flower, Value.Flower));

        internal static HashSet<Value> NumericValues = new HashSet<Value>
            {Value.N1, Value.N2, Value.N3, Value.N4, Value.N5, Value.N6, Value.N7, Value.N8, Value.N9};

        public bool CanHold(Card other)
        {
            if (Color == Color.Flower || other.Color == Color.Flower || Color == other.Color) return false;
            if (!NumericValues.Contains(Value) || !NumericValues.Contains(other.Value)) return false;
            return Value - other.Value == 1;
        }

        public bool Equals(Card other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Color == other.Color && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Card) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Color * 397) ^ (int) Value;
            }
        }

        private static readonly Dictionary<Color, string> AnsiColors = new Dictionary<Color, string>
        {
            {Color.Black, "\u001b[0m"},
            {Color.Red, "\u001b[31m"},
            {Color.Green, "\u001b[32m"},
            {Color.Flower, "\u001b[33m"},
        };

        public override string ToString() => $"{AnsiColors[Color]}{Color} {Value}\u001b[0m";
    }
}