using System;
using System.Linq;
using Solitaire;
using Solitaire.Game;
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
            var solutions = boards.Select(board => new Solver(board).Solve());
            var ratio = solutions.Count(solution => solution != null) / (double)boards.Count;
            Assert.InRange(ratio, 0.5, 1.0);
        }
    }
}