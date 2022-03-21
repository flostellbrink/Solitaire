using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Solitaire;
using Solitaire.Game;
using Color = Solitaire.Game.Color;

namespace Server
{
    public class ImageBoard : Board
    {
        public ImageBoard(Image<Rgba32> image)
        {
            // Extract board
            image.Mutate(context =>
            {
                int? top = null;
                int? left = null;
                int? right = null;
                int? bottom = null;

                context.Configuration.MaxDegreeOfParallelism = 1;
                context.ProcessPixelRowsAsVector4((row, rowPosition) =>
                {
                    var rowHasValue = false;
                    for (var x = 0; x < row.Length; x++)
                    {
                        var pixel = row[x];
                        if (pixel.X > 0.01f || pixel.Y > 0.01f || pixel.Z > 0.01f)
                        {
                            rowHasValue = true;
                            left ??= x;
                        }
                        else if (rowHasValue)
                        {
                            right ??= x;
                        }
                    }

                    if (rowHasValue)
                    {
                        top ??= rowPosition.Y;
                    }
                    else if (top != null)
                    {
                        bottom ??= rowPosition.Y;
                    }
                });

                if (left == null || right == null || top == null || bottom == null)
                    throw new Exception("Could not find board.");

                context.Crop(new Rectangle(left.Value, top.Value, right.Value - left.Value, bottom.Value - top.Value));

                context.Resize(1280, 720);
            });

            // Extract cards
            var pixelArray = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(pixelArray);
            Rgba32 PixelAt(Vector2 position) => pixelArray[(int)position.Y * image.Width + (int)position.X];
            Rgba32 AsColor(long hex) => new Rgba32((byte)(hex >> 16), (byte)(hex >> 8), (byte)hex);

            Image<Rgba32> SubImage(Vector2 position, Vector2 size)
            {
                var buffer = new Rgba32[(int)size.X * (int)size.Y];
                for (var y = 0; y < size.Y; y++)
                for (var x = 0; x < size.X; x++)
                    buffer[y * (int)size.X + x] = PixelAt(position + new Vector2(x, y));

                return Image.LoadPixelData(buffer, (int)size.X, (int)size.Y);
            }

            float ColorDistance(Rgba32 a, Rgba32 b) =>
                (a.R - b.R) * (a.R - b.R) + (a.G - b.G) * (a.G - b.G) + (a.B - b.B) * (a.B - b.B);

            bool SimilarColor(Rgba32 a, Rgba32 b) => ColorDistance(a, b) < 300;

            float ImageDistance(Image<Rgba32> a, Image<Rgba32> b)
            {
                var size = a.Size();
                if (size != b.Size()) throw new Exception("Images are not the same size.");

                var bufferA = new Rgba32[size.Width * size.Height];
                var bufferB = new Rgba32[size.Width * size.Height];
                a.CopyPixelDataTo(bufferA);
                b.CopyPixelDataTo(bufferB);

                var distance = 0f;
                for (var i = 0; i < size.Width * size.Height; i++)
                    distance += ColorDistance(bufferA[i], bufferB[i]);

                return distance;
            }

            Card ClosestCard(Vector2 position)
            {
                var target = SubImage(position, new Vector2(18, 18));
                Card closestCard = null;
                var closestDistance = float.MaxValue;

                foreach (var (card, image) in CardTemplates.Templates)
                {
                    var distance = ImageDistance(target, image);
                    if (!(distance < closestDistance)) continue;
                    closestDistance = distance;
                    closestCard = card;
                }

                return closestCard ?? throw new Exception("Missing card templates");
            }

            var stackStart = new Vector2(67, 358);
            var lockableStart = new Vector2(67, 99);
            var cardStep = new Vector2(148.8f, 29);

            var stackXs = Enumerable.Range(0, 8).Select(i => stackStart.X + i * cardStep.X).ToArray();
            var stackYs = Enumerable.Range(0, 12).Select(i => stackStart.Y + i * cardStep.Y).ToArray();
            var stacksPositions = stackXs.Select(x => stackYs.Select(y => new Vector2(x, y)).ToArray()).ToArray();

            var lockableXs = Enumerable.Range(0, 3).Select(i => lockableStart.X + i * cardStep.X).ToArray();
            var lockablePositions = lockableXs.Select(x => new Vector2(x, lockableStart.Y)).ToArray();

            var flowerPosition = new Vector2(640, lockableStart.Y);

            var filingXs = Enumerable.Range(5, 3).Select(i => lockableStart.X + i * cardStep.X).ToArray();
            var filingPositions = filingXs.Select(x => new Vector2(x, lockableStart.Y)).ToArray();

            var expectedEdgeColor = AsColor(0xD2D5CA);
            var expectedCardColor = AsColor(0xC1C2B4);
            var expectedLockedColor = AsColor(0x587569);

            for (var i = 0; i < stacksPositions.Length; i++)
            {
                var stackPositions = stacksPositions[i];
                foreach (var stackPosition in stackPositions)
                {
                    var edgeColor = PixelAt(stackPosition - new Vector2(0, 8));
                    if (!SimilarColor(edgeColor, expectedEdgeColor)) break;

                    Stacks.ElementAt(i).Cards.Add(ClosestCard(stackPosition));
                }
            }

            for (var i = 0; i < lockablePositions.Length; i++)
            {
                var lockablePosition = lockablePositions[i];

                var cardColor = PixelAt(lockablePosition);
                if (SimilarColor(cardColor, expectedCardColor))
                    LockableStacks.ElementAt(i).Cards.Add(ClosestCard(lockablePosition));

                var lockedColor = PixelAt(lockablePosition + new Vector2(4, 4));
                if (SimilarColor(lockedColor, expectedLockedColor))
                    LockableStacks.ElementAt(i).Locked = true;
            }

            {
                var cardColor = PixelAt(flowerPosition);
                if (SimilarColor(cardColor, expectedCardColor))
                    FlowerStack.Cards.Add(new Card(Color.Flower, Value.Flower));
            }

            for (var i = 0; i < filingPositions.Length; i++)
            {
                var filingPosition = filingPositions[i];
                var cardColor = PixelAt(filingPosition);
                if (!SimilarColor(cardColor, expectedCardColor)) continue;

                var card = ClosestCard(filingPosition);
                for (var value = Value.N1; value <= card.Value; value++)
                    FilingStacks.ElementAt(i).Cards.Add(new Card(card.Color, value));
            }

            if (Solved) return;

            var allDragons = Stacks.SelectMany(stack => stack.Cards)
                .Concat(LockableStacks.SelectMany(stack => stack.Cards))
                .Where(card => card.Value == Value.Dragon)
                .ToList();
            var colorsWithMissingDragons = new List<Color> { Color.Red, Color.Green, Color.Black }
                .Where(color => allDragons.All(dragon => dragon.Color != color))
                .ToList();
            var lockedStacks = LockableStacks.Where(stack => stack.Locked).ToList();

            if (colorsWithMissingDragons.Count != lockedStacks.Count)
                throw new Exception("Missing dragons don't match locked stacks");

            foreach (var (color, stack) in colorsWithMissingDragons.Zip(lockedStacks))
            foreach (var _ in Enumerable.Range(0, 4))
                stack.Cards.Add(new Card(color, Value.Dragon));
        }
    }
}