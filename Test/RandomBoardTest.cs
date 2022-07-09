using System.Linq;
using Core;
using Core.Game;
using Xunit;

namespace Test
{
    public class RandomBoardTest
    {
        public RandomBoardTest()
        {
            AnsiColorHelper.Enabled = false;
        }

        [Fact]
        public void BoardValid()
        {
            var boards = Enumerable.Range(0, 100).Select((_) => new RandomBoard());
            Assert.True(boards.All(board => board.IsValid()));
        }

        [Fact]
        public void MostlySolvable()
        {
            var boards = Enumerable.Range(0, 10).Select((_) => new RandomBoard()).ToList();
            var solutions = boards.Select(board => new Solver(board, Solver.Mode.Normal).Solve());
            var ratio = solutions.Count(solution => solution != null) / (double)boards.Count;
            Assert.InRange(ratio, 0.5, 1.0);
        }

        [Fact]
        public void RandomHardModeUnsolvable()
        {
            var boards = Enumerable.Range(0, 10).Select((_) => new RandomBoard()).ToList();
            var solutions = boards.Select(board => new Solver(board, Solver.Mode.Hard).Solve());
            var ratio = solutions.Count(solution => solution != null) / (double)boards.Count;
            Assert.Equal(0, ratio);
        }

        [Fact]
        public void SpecificHardModeSolvable()
        {
            var board = new RandomBoard(41009882);
            var solution = new Solver(board, Solver.Mode.Hard).Solve();
            Assert.NotNull(solution);
        }
    }
}