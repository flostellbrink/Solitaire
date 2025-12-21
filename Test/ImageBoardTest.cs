using System;
using System.IO;
using System.Linq;
using Core;
using Core.Game;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;
using Color = Core.Game.Color;

namespace Test;

public class ImageBoardTest
{
    private readonly ITestOutputHelper _output;

    public ImageBoardTest(ITestOutputHelper output)
    {
        _output = output;
        AnsiColorHelper.Enabled = false;
    }

    private static Image<Rgba32> LoadImage(string imageName) =>
        Image.Load<Rgba32>($"{AppContext.BaseDirectory}/../../../Images/{imageName}.png");

    class ImagePathGenerator : TheoryData<string>
    {
        public ImagePathGenerator()
        {
            foreach (
                string path in Directory.GetFiles($"{AppContext.BaseDirectory}/../../../Images")
            )
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                Console.WriteLine($"Adding image path: {fileName}");
                if (!string.IsNullOrWhiteSpace(fileName))
                    Add(fileName);
            }
        }
    }

    [Theory]
    [ClassData(typeof(ImagePathGenerator))]
    public void ParsesValidBoard(string imageName)
    {
        var board = new ImageBoard(LoadImage(imageName));
        _output.WriteLine(board.ToString());

        if (imageName == "solved")
        {
            Assert.True(board.Solved);
            return;
        }

        Assert.True(board.IsValid());
    }

    [Fact]
    public void GenerateTemplateCode()
    {
        var colors = Enum.GetValues<Color>().Cast<Color>().ToArray();
        var values = Enum.GetValues<Value>().Cast<Value>().ToArray();

        var templatePaths = Directory.GetFiles($"{AppContext.BaseDirectory}/../../../Templates");
        foreach (var templatePath in templatePaths)
        {
            var fileName = Path.GetFileNameWithoutExtension(templatePath);
            if (string.IsNullOrWhiteSpace(fileName))
                continue;

            var parts = fileName.Split(' ');
            Assert.Equal(2, parts.Length);

            var color = colors.Single(color =>
                color.ToDescription().Equals(parts[0], StringComparison.OrdinalIgnoreCase)
            );
            var value = values.Single(value =>
                value.ToDescription().Equals(parts[1], StringComparison.OrdinalIgnoreCase)
            );
            var card = new Card(color, value);
            var image = Image.Load<Rgba32>(templatePath);
            var pixelArray = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(pixelArray);
            Assert.Equal(18, image.Width);
            Assert.Equal(18, image.Height);

            var formattedCard = $"new Card(Color.{color}, Value.{value})";
            static string FormatPixel(Rgba32 pixel) =>
                $"{pixel.R}, {pixel.G}, {pixel.B}, {pixel.A}";
            var formattedPixels = string.Join(", ", pixelArray.Select(FormatPixel));
            var formattedImage =
                $"Image.LoadPixelData<Rgba32>(new byte[]{{{formattedPixels}}}, 18, 18)";
            _output.WriteLine($"({formattedCard}, {formattedImage}),");
        }
    }
}
