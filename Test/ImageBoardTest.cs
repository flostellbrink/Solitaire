using System;
using System.IO;
using System.Linq;
using Server;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Solitaire;
using Solitaire.Game;
using Xunit;
using Xunit.Abstractions;
using Color = Solitaire.Game.Color;

namespace Test
{
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

        [Theory]
        [InlineData("fresh")]
        [InlineData("full")]
        [InlineData("lockable")]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        [InlineData("5")]
        [InlineData("6")]
        [InlineData("7")]
        [InlineData("8")]
        [InlineData("9")]
        [InlineData("10")]
        [InlineData("11")]
        [InlineData("12")]
        [InlineData("13")]
        [InlineData("14")]
        [InlineData("15")]
        [InlineData("16")]
        [InlineData("17")]
        [InlineData("18")]
        [InlineData("19")]
        [InlineData("20")]
        [InlineData("21")]
        [InlineData("22")]
        [InlineData("23")]
        [InlineData("24")]
        [InlineData("25")]
        [InlineData("26")]
        [InlineData("27")]
        [InlineData("28")]
        [InlineData("29")]
        [InlineData("30")]
        [InlineData("31")]
        [InlineData("32")]
        [InlineData("33")]
        [InlineData("34")]
        public void ParsesValidBoard(string imageName)
        {
            var board = new ImageBoard(LoadImage(imageName));
            _output.WriteLine(board.ToString());
            Assert.True(board.IsValid());
        }

        [Theory]
        [InlineData("35")]
        public void ParsesSolvedBoard(string imageName)
        {
            var board = new ImageBoard(LoadImage(imageName));
            _output.WriteLine(board.ToString());
            Assert.True(board.Solved);
        }

        [Fact]
        public void GenerateTemplateCode()
        {
            var colors = Enum.GetValues(typeof(Color)).Cast<Color>().ToArray();
            var values = Enum.GetValues(typeof(Value)).Cast<Value>().ToArray();

            var templatePaths = Directory.GetFiles($"{AppContext.BaseDirectory}/../../../Templates");
            foreach (var templatePath in templatePaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(templatePath);
                if (string.IsNullOrWhiteSpace(fileName)) continue;

                var parts = fileName.Split(' ');
                Assert.Equal(2, parts.Length);

                var color = colors.Single(color =>
                    color.ToDescription().Equals(parts[0], StringComparison.OrdinalIgnoreCase));
                var value = values.Single(value =>
                    value.ToDescription().Equals(parts[1], StringComparison.OrdinalIgnoreCase));
                var card = new Card(color, value);
                var image = Image.Load<Rgba32>(templatePath);
                var pixelArray = new Rgba32[image.Width * image.Height];
                image.CopyPixelDataTo(pixelArray);
                Assert.Equal(18, image.Width);
                Assert.Equal(18, image.Height);

                var formattedCard = $"new Card(Color.{color}, Value.{value})";
                string FormatPixel(Rgba32 pixel) => $"{pixel.R}, {pixel.G}, {pixel.B}, {pixel.A}";
                var formattedPixels = string.Join(", ", pixelArray.Select(FormatPixel));
                var formattedImage = $"Image.LoadPixelData<Rgba32>(new byte[]{{{formattedPixels}}}, 18, 18)";
                _output.WriteLine($"({formattedCard}, {formattedImage}),");
            }
        }
    }
}