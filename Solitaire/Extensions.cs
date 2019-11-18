using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Solitaire
{
    public static class Extensions
    {
        public static string ToDescription<T>(this T value)
        {
            if (!typeof(T).IsEnum)
                return value.ToString();

            return typeof(T).GetField(value.ToString())
                       .GetCustomAttributes(typeof(DescriptionAttribute), false)
                       .Cast<DescriptionAttribute>()
                       .Select(attribute => attribute.Description)
                       .FirstOrDefault() ?? value.ToString();
        }

        public static IEnumerable<(T, T)> ConsecutivePairs<T>(this IEnumerable<T> list)
        {
            var previous = list.FirstOrDefault();
            foreach (var item in list.Skip(1))
            {
                yield return (previous, item);
                previous = item;
            }
        }

        private static readonly Regex AnsiRegex = new Regex("\u001b[^m]+m", RegexOptions.Compiled);

        public static string PadRightAnsi(this string value, int totalWidth)
        {
            var invisibleCharacters = AnsiRegex.Matches(value).Sum(match => match.Length);
            return value.PadRight(totalWidth + invisibleCharacters);
        }

        public static void AppendJoinPadded<T>(
            this StringBuilder builder,
            string separator,
            int totalWidth,
            IEnumerable<T> values)
        {
            builder.Append(string.Join(separator, values.Select(item => (item?.ToString() ?? string.Empty).PadRightAnsi(totalWidth))));
        }

        public static int GetCollectionHashCode<T>(this ICollection<T> collection)
        {
            unchecked
            {
                return collection
                    .Select(equatable => equatable != null ? equatable.GetHashCode() : 0)
                    .Aggregate(0, (result, hash) => (result * 397) ^ hash);
            }
        }

        public static int GetCollectionHashCodeUnordered<T>(this ICollection<T> collection)
        {
            unchecked
            {
                return collection
                    .Select(equatable => equatable != null ? equatable.GetHashCode() : 0)
                    .OrderBy(hash => hash)
                    .Aggregate(0, (result, hash) => (result * 397) ^ hash);
            }
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> by, params TKey[] initialKeys)
        {
            var keys = new HashSet<TKey>();
            foreach (var initialKey in initialKeys)
                keys.Add(initialKey);
            foreach (var value in enumerable)
                if (keys.Add(by(value))) yield return value;
        }

        // Like Except, but only removes one instance of T per item of except
        public static IEnumerable<T> ExceptQuantitative<T>(this IEnumerable<T> enumerable, IEnumerable<T> except) where T: IEquatable<T>
        {
            var exceptGroups = except
                .GroupBy(value => value)
                .ToLookup(group => group.Key, group => group.Count());
            return enumerable
                .GroupBy(value => value)
                .Select(group => (group.Key, Count: group.Count(), Other: exceptGroups[group.Key].FirstOrDefault()))
                .SelectMany(entry => Enumerable.Repeat(entry.Key, Math.Max(0, entry.Count - entry.Other)));
        }
    }
}
