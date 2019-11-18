using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Solitaire.Game
{
    public enum Color { None, Black, Green, Red, Flower }

    public enum Value
    {
        None,
        [Description("1")] N1,
        [Description("2")] N2,
        [Description("3")] N3,
        [Description("4")] N4,
        [Description("5")] N5,
        [Description("6")] N6,
        [Description("7")] N7,
        [Description("8")] N8,
        [Description("9")] N9,
        Dragon, Flower
    }

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
            .Concat(BaseColors.SelectMany(color => Enumerable.Range(0, 4).Select(_ => new Card(color, Value.Dragon))))
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

        private static readonly Dictionary<Color, AnsiColor> AnsiColors = new Dictionary<Color, AnsiColor>
        {
            {Color.Black, AnsiColor.None},
            {Color.Red, AnsiColor.ForegroundRed},
            {Color.Green, AnsiColor.ForegroundGreen},
            {Color.Flower, AnsiColor.ForegroundYellow},
        };

        public override string ToString() => 
            $"{Color.ToDescription()} {Value.ToDescription()}".Colorize(AnsiColors[Color]);
    }
}