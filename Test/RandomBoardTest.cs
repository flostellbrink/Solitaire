using System.Collections.Generic;
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

        private class SolvableBoard : Board
        {
            public SolvableBoard()
            {
                Stacks.ElementAt(0).Add(new Unit(new List<Card>
                {
                    new(Color.Black, Value.N1),
                    new(Color.Green, Value.N1),
                    new(Color.Red, Value.N1),
                    new(Color.Black, Value.Dragon),
                    new(Color.Green, Value.N2),
                }));
                Stacks.ElementAt(1).Add(new Unit(new List<Card>
                {
                    new(Color.Black, Value.Dragon),
                    new(Color.Red, Value.Dragon),
                    new(Color.Green, Value.Dragon),
                }));
                Stacks.ElementAt(2).Add(new Unit(new List<Card>
                {
                    new(Color.Black, Value.Dragon),
                    new(Color.Red, Value.Dragon),
                    new(Color.Green, Value.Dragon),
                }));
                Stacks.ElementAt(3).Add(new Unit(new List<Card>
                {
                    new(Color.Black, Value.Dragon),
                    new(Color.Red, Value.Dragon),
                    new(Color.Green, Value.Dragon),
                    new(Color.Red, Value.N2),
                }));
                Stacks.ElementAt(4).Add(new Unit(new List<Card>
                {
                    new(Color.Red, Value.Dragon),
                    new(Color.Green, Value.Dragon),
                    new(Color.Black, Value.N2),
                    new(Color.Flower, Value.Flower),
                }));
                Stacks.ElementAt(5).Add(new Unit(new List<Card>
                {
                    new(Color.Black, Value.N9),
                    new(Color.Green, Value.N8),
                    new(Color.Red, Value.N7),
                    new(Color.Black, Value.N6),
                    new(Color.Green, Value.N5),
                    new(Color.Red, Value.N4),
                    new(Color.Black, Value.N3),
                }));
                Stacks.ElementAt(6).Add(new Unit(new List<Card>
                {
                    new(Color.Green, Value.N9),
                    new(Color.Red, Value.N8),
                    new(Color.Black, Value.N7),
                    new(Color.Green, Value.N6),
                    new(Color.Red, Value.N5),
                    new(Color.Black, Value.N4),
                    new(Color.Green, Value.N3),
                }));
                Stacks.ElementAt(7).Add(new Unit(new List<Card>
                {
                    new(Color.Red, Value.N9),
                    new(Color.Black, Value.N8),
                    new(Color.Green, Value.N7),
                    new(Color.Red, Value.N6),
                    new(Color.Black, Value.N5),
                    new(Color.Green, Value.N4),
                    new(Color.Red, Value.N3),
                }));
            }
        }

        [Fact]
        public void SpecificHardModeSolvable()
        {
            var board = new SolvableBoard();
            Assert.True(board.IsValid());
            Assert.True(board.HardModeSolvable);
            Assert.False(board.Solved);

            var solution = new Solver(board, Solver.Mode.Hard).Solve();
            Assert.NotNull(solution);
        }
    }
}