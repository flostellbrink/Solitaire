using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Solitaire
{
    public enum AnsiColor
    {
        None = 0,

        ForegroundBlack = 30,
        ForegroundRed = 31,
        ForegroundGreen = 32,
        ForegroundYellow = 33,
        ForegroundBlue = 34,
        ForegroundMagenta = 35,
        ForegroundCyan = 36,
        ForegroundWhite = 37,

        BackgroundBlack = 40,
        BackgroundRed = 41,
        BackgroundGreen = 42,
        BackgroundYellow = 43,
        BackgroundBlue = 44,
        BackgroundMagenta = 45,
        BackgroundCyan = 46,
        BackgroundWhite = 47,
    }

    public static class AnsiColorHelper
    {
        public static bool Enabled = true;

        public static string AnsiColorEscapeCode(this AnsiColor color) => $"\u001b[{(int)color}m";

        public static string Colorize(this string value, AnsiColor color) =>
            Enabled ? $"{color.AnsiColorEscapeCode()}{value}{AnsiColor.None.AnsiColorEscapeCode()}" : value;

        private static readonly Regex AnsiRegex = new("\u001b[^m]+m", RegexOptions.Compiled);

        public static int AnsiInvisibleLength(this string value) =>
            AnsiRegex.Matches(value).Sum(match => match.Length);

        public static string PadRightAnsi(this string value, int totalWidth) =>
            value.PadRight(totalWidth + value.AnsiInvisibleLength());

        public static StringBuilder EndColor(this StringBuilder builder) =>
            Enabled ? builder.BeginColor(AnsiColor.None) : builder;

        public static StringBuilder BeginColor(this StringBuilder builder, AnsiColor color) =>
            Enabled ? builder.Append(color.AnsiColorEscapeCode()) : builder;
    }
}