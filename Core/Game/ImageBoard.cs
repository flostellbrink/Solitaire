using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenCvSharp;

namespace Core.Game;

public partial class ImageBoard : Board
{
    private Action<string> Log { get; }

    private static (Card card, Mat mat)[] Templates { get; }
    private static (Card card, Mat mat)[] TestTemplates { get; }
    private static (int Width, int Height) MinTemplateSize { get; }

    [GeneratedRegex("^[0-9]$")]
    private static partial Regex DigitRegex();

    private static Card ParseTemplateName(string fileName)
    {
        var parts = fileName.Split(' ');
        var color = Enum.Parse<Color>(parts[0], true);
        var isDigit = DigitRegex().IsMatch(parts[1]);
        var value = Enum.Parse<Value>(isDigit ? $"N{parts[1]}" : parts[1], true);
        return new Card(color, value);
    }

    static ImageBoard()
    {
        Templates =
        [
            .. Directory
                .GetFiles("Templates")
                .Select(static path =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    var card = ParseTemplateName(fileName);
                    var mat = new Mat(path, ImreadModes.Color);
                    return (card, mat);
                }),
        ];
        TestTemplates = [.. Templates.Where((template) => template.card.Value is Value.N5)];
        MinTemplateSize = (Templates.Min(t => t.mat.Width), Templates.Min(t => t.mat.Height));
    }

    private static Mat ResizeToScale(Mat image, double scale)
    {
        var width = (int)(image.Width * scale);
        var height = (int)(image.Height * scale);
        return image.Resize(new Size(width, height));
    }

    private static double AllDigitsScore(Mat resizedImage)
    {
        if (resizedImage.Width < MinTemplateSize.Width)
            return 0.0;
        if (resizedImage.Height < MinTemplateSize.Height)
            return 0.0;

        double totalScore = 0.0;
        foreach (var (card, template) in TestTemplates)
        {
            using var output = new Mat();
            Cv2.MatchTemplate(resizedImage, template, output, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(output, out _, out double maxVal, out _, out _);
            totalScore += maxVal;
        }

        return totalScore / TestTemplates.Length;
    }

    private double FindScale(Mat boardImage)
    {
        static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        var minScaleByWidth = (double)MinTemplateSize.Width / boardImage.Width;
        var minScaleByHeight = (double)MinTemplateSize.Height / boardImage.Height;
        var minScale = Math.Max(0.1, Math.Max(minScaleByWidth, minScaleByHeight));
        var maxScale = 2.0;

        var center = Clamp(0.7, minScale, maxScale);

        var stages = new (double Step, double Radius)[]
        {
            (0.10, 0.60),
            (0.03, 0.18),
            (0.01, 0.06),
            (0.003, 0.018),
        };

        foreach (var (step, radius) in stages)
        {
            var start = Clamp(center - radius, minScale, maxScale);
            var end = Clamp(center + radius, minScale, maxScale);

            var bestScale = center;
            var bestScore = double.NegativeInfinity;

            for (var scale = start; scale <= end + (step * 0.5); scale += step)
            {
                using var resized = ResizeToScale(boardImage, scale);
                var score = AllDigitsScore(resized);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestScale = scale;
                }
            }

            Log($"step={step:0.###} radius={radius:0.###}: bestScale={bestScale:0.###}");
            center = bestScale;
        }

        return center;
    }

    private static List<(Card card, double score, Rect rect)> FindCards(Mat resizedImage)
    {
        List<(Card card, double score, Rect rect)> foundCards = [];

        foreach (var (card, template) in Templates)
        {
            using var output = new Mat();
            Cv2.MatchTemplate(resizedImage, template, output, TemplateMatchModes.CCoeffNormed);

            double threshold = 0.8;
            while (true)
            {
                Cv2.MinMaxLoc(output, out _, out double maxVal, out _, out Point maxLoc);
                if (maxVal < threshold)
                    break;

                var rect = new Rect(maxLoc.X, maxLoc.Y, template.Width, template.Height);
                foundCards.Add((card, maxVal, rect));
                Cv2.Rectangle(output, rect, new Scalar(0), -1);
            }
        }

        return foundCards;
    }

    public ImageBoard(Mat boardImage, Action<string>? log = null)
    {
        Log = log ?? Console.WriteLine;

        var scale = FindScale(boardImage);
        using var resizedImage = ResizeToScale(boardImage, scale);

        var cards = FindCards(resizedImage).ToList();

        var topRow = (int)(resizedImage.Height * 0.155);
        var stackBase = (int)(resizedImage.Height * 0.49);
        var stackStep = (int)(resizedImage.Height * 0.038);
        var stacks = Enumerable
            .Range(0, 14)
            .Select(offset => stackBase + offset * stackStep)
            .ToList();

        var flowerColumn = (int)(resizedImage.Width * 0.495);
        var left = (int)(resizedImage.Width * 0.165);
        var columnStep = (int)(resizedImage.Width * 0.0887);
        var verticalLines = Enumerable.Range(0, 8).Select(i => left + (i * columnStep)).ToList();

        Card? GetCardAt(int x, int y)
        {
            var candidates = cards
                .Where(c => c.rect.Contains(new Point(x, y)))
                .OrderByDescending(c => c.score)
                .ToList();
            if (candidates.Count == 0)
                return null;

            var best = candidates[0];
            if (best.card.Value == Value.Flower || best.card.Value == Value.Dragon)
                return best.card;

            using var roi = new Mat(resizedImage, best.rect);
            using var beigeMask = new Mat();
            {
                const int tol = 25;
                var lower = new Scalar(
                    Math.Clamp(184 - tol, 0, 255),
                    Math.Clamp(196 - tol, 0, 255),
                    Math.Clamp(195 - tol, 0, 255)
                );
                var upper = new Scalar(
                    Math.Clamp(184 + tol, 0, 255),
                    Math.Clamp(196 + tol, 0, 255),
                    Math.Clamp(195 + tol, 0, 255)
                );
                Cv2.InRange(roi, lower, upper, beigeMask);
            }

            using var keepMask = new Mat();
            Cv2.BitwiseNot(beigeMask, keepMask);

            var average = Cv2.CountNonZero(keepMask) == 0 ? Cv2.Mean(roi) : Cv2.Mean(roi, keepMask);
            var color = average switch
            {
                var (b, g, r, _) when r > g * 1.02 && r > b * 1.02 => Color.Red,
                var (b, g, r, _) when g > r * 1.02 && g > b * 1.02 => Color.Green,
                _ => Color.Black,
            };
            return new Card(color, best.card.Value);
        }

        foreach (var (stack, x) in LockableStacks.Zip(verticalLines))
        {
            var card = GetCardAt(x, topRow);
            if (card == null)
                continue;
            stack.Add(card);
        }

        {
            var card = GetCardAt(flowerColumn, topRow);
            if (card != null)
                FlowerStack.Add(card);
        }

        foreach (var (stack, x) in FilingStacks.Zip(verticalLines.Skip(5)))
        {
            var card = GetCardAt(x, topRow);
            if (card == null)
                continue;
            if (card.Value < Value.N1 || card.Value > Value.N9)
                throw new InvalidDataException($"Invalid card {card} in filing stack");
            for (var value = Value.N1; value <= card.Value; value++)
            {
                stack.Add(new Card(card.Color, value));
            }
        }

        foreach (var (stack, x) in Stacks.Zip(verticalLines))
        {
            foreach (var y in stacks)
            {
                var card = GetCardAt(x, y);
                if (card == null)
                    break;
                stack.Add(card);
            }
        }

        var allDragons = Stacks
            .SelectMany(stack => stack.Cards)
            .Concat(LockableStacks.SelectMany(stack => stack.Cards))
            .Where(card => card.Value == Value.Dragon)
            .ToList();
        var colorsWithMissingDragons = new List<Color> { Color.Red, Color.Green, Color.Black }
            .Where(color => allDragons.All(dragon => dragon.Color != color))
            .ToList();
        var emptyLockables = LockableStacks.Where(stack => stack.Cards.Count == 0).ToList();

        if (colorsWithMissingDragons.Count <= emptyLockables.Count)
        {
            foreach (var (color, stack) in colorsWithMissingDragons.Zip(emptyLockables))
            foreach (var _ in Enumerable.Range(0, 4))
                stack.Cards.Add(new Card(color, Value.Dragon));
        }

        // MARK: Debug

        // Scalar GetColor(Card card)
        // {
        //     return card.Color switch
        //     {
        //         Color.Red => Scalar.Red,
        //         Color.Black => Scalar.Black,
        //         Color.Green => Scalar.Green,
        //         Color.Flower => Scalar.Yellow,
        //         _ => Scalar.Gray,
        //     };
        // }

        // void drawRow(int y)
        // {
        //     Cv2.Line(
        //         resizedImage,
        //         new Point(0, y),
        //         new Point(resizedImage.Width, y),
        //         new Scalar(255),
        //         1
        //     );
        // }
        // drawRow(topRow);
        // foreach (var row in stacks)
        //     drawRow(row);

        // void drawColumn(int x)
        // {
        //     Cv2.Line(
        //         resizedImage,
        //         new Point(x, 0),
        //         new Point(x, resizedImage.Height),
        //         new Scalar(255),
        //         1
        //     );
        // }

        // drawColumn(flowerColumn);
        // foreach (var x in verticalLines)
        //     drawColumn(x);

        // foreach (var (card, score, rect) in cards)
        //     Cv2.Rectangle(resizedImage, rect, GetColor(card), 1);

        // var name = Random.Shared.Next().ToString(CultureInfo.InvariantCulture);
        // resizedImage.SaveImage($"../../../{name}_resized.png");
    }
}
