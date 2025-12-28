using System.IO;
using Core;
using Core.Game;
using OpenCvSharp;
using Xunit;
using Xunit.Abstractions;

namespace Test;

public class ImageBoardTest
{
    private readonly ITestOutputHelper _output;

    public ImageBoardTest(ITestOutputHelper output)
    {
        _output = output;
        AnsiColorHelper.Enabled = false;
    }

    class ImagePathGenerator : TheoryData<string>
    {
        public ImagePathGenerator()
        {
            foreach (string path in Directory.GetFiles("Images"))
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrWhiteSpace(fileName))
                    Add(fileName);
            }
        }
    }

    [Theory]
    [ClassData(typeof(ImagePathGenerator))]
    public void ParsesValidBoard(string imageName)
    {
        using var mat = new Mat($"Images/{imageName}.png", ImreadModes.Color);
        var board = new ImageBoard(mat, _output.WriteLine);
        _output.WriteLine(board.ToString());

        if (imageName == "solved")
        {
            Assert.True(board.Solved);
            return;
        }

        Assert.True(board.IsValid(null));
    }
}
