using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Zabavnov.Sudoku;

namespace Sudoku.Tests
{
    public class ProcessorTests
    {
        [Fact]
        public void TestArrange()
        {
            var cells = new[]
            {
                new Cell(0,0,4),
                new Cell(0,0,5, 9),
                new Cell(0,0),
                new Cell(0,0),
                new Cell(0,0),
                new Cell(0,0),
                new Cell(0,0,9),
                new Cell(0,0,3),
                new Cell(0,0,8),
            };

            var res = Processor.AdjustCells(cells);
            Assert.True(res);

            Assert.Equal(5, cells[1].Value);
            Assert.True(cells[2].EqualVariants(new [] { 1, 2, 6, 7 }));
            Assert.True(cells[3].EqualVariants(new [] { 1, 2, 6, 7 }));
            Assert.True(cells[4].EqualVariants(new [] { 1, 2, 6, 7 }));
            Assert.True(cells[5].EqualVariants(new [] { 1, 2, 6, 7 }));
        }

        [Fact]
        public void TestLastOneRule()
        {
            const string str =
"2...7..38\n" + 
".....6.7.\n" + 
"3...4.6..\n" + 
"..8.2.7..\n" + 
"1.......6\n" + 
"..7.3.4..\n" + 
"..4.8...9\n" + 
".6.4.....\n" +
"91..6...2\n";
            var grid = Grid.Load(str);

            Processor.AdjustCells(grid.Columns[0]);
            Processor.AdjustCells(grid.Columns[1]);
            Processor.AdjustCells(grid.Columns[2]);
            Processor.AdjustCells(grid.Rows[6]);
            Processor.AdjustCells(grid.Rows[7]);
            Processor.AdjustCells(grid.Rows[8]);


            var area = grid.Blocks[6];
            Processor.AdjustCells(area);

            var cell = grid.Cells[7, 0];
            Assert.True(cell.IsEmpty);
            var res = Processor.LastOneRule(area);
            Assert.True(res);
            Assert.True(cell.IsCompleted);
        }

        [Fact]
        public void TestSetRules2()
        {
            var cells = new[]
            {
                new Cell(0,0,4),
                new Cell(0,0,1, 6),
                new Cell(0,0,1, 6),
                new Cell(0,0,1, 2, 5),
                new Cell(0,0,1, 2, 5, 6, 7),
                new Cell(0,0,2, 5, 6, 7),
                new Cell(0,0,9),
                new Cell(0,0,3),
                new Cell(0,0,8),
            };

            var pairs = Processor.FindPairs(cells).ToArray();
            Assert.Equal(1, pairs.Length);
            var item = pairs[0];
            Assert.Equal(new HashSet<int> { 1, 6}, item.Key, new SetEqualityComparer<int>());
            Assert.Equal(2, item.Value.Count);

            var rc = Processor.SetRules(cells, Processor.FindPairs);
            Assert.True(rc);
            Assert.True(cells[3].EqualVariants(new[] { 2, 5 }));
            Assert.True(cells[4].EqualVariants(new[] { 2, 5, 7}));
            Assert.True(cells[5].EqualVariants(new[] { 2, 5, 7}));
        }

        [Fact]
        public void TestSetRules3()
        {
            var cells = new[]
            {
                new Cell(0,0,4, 5, 6, 7, 9),
                new Cell(0,0,2),
                new Cell(0,0,1, 5, 6, 7, 9),
                new Cell(0,0,5, 8, 9),
                new Cell(0,0,5, 8),
                new Cell(0,0,5, 9),
                new Cell(0,0,3, 5, 8, 9),
                new Cell(0,0,3, 4, 5, 8, 9),
                new Cell(0,0,1, 6),
            };

            var triads = Processor.FindTriads(cells).ToArray();
            Assert.Equal(1, triads.Length);
            Assert.Equal(new HashSet<int> { 5, 8, 9 }, triads[0].Key, new SetEqualityComparer<int>());
            Assert.Equal(3, triads[0].Value.Count);

            var rc = Processor.SetRules(cells, Processor.FindTriads);
            Assert.True(rc);

            Assert.True(cells[0].EqualVariants(new[] { 4, 6, 7 }));
            Assert.True(cells[2].EqualVariants(new[] { 1, 6, 7 }));
            Assert.Equal(3, cells[6].Value);
            Assert.True(cells[7].EqualVariants(new[] { 3, 4 }));
            Assert.True(cells[8].EqualVariants(new[] { 1, 6 }.ToSet()));
        }

        [Fact]
        public void TestSetRules4()
        {
            var cells = new[]
            {
                new Cell(0,0,1, 5),
                new Cell(0,0,1, 2, 4, 5),
                new Cell(0,0,2, 4, 5, 7),
                new Cell(0,0,1, 5, 6, 8),   
                new Cell(0,0,1, 5, 6, 8),   
                new Cell(0,0,3, 5, 6, 7, 8),   
                new Cell(0,0,1, 6),
                new Cell(0,0,9),
                new Cell(0,0,3, 4, 6) 
            };

            var quaed = Processor.FindQuads(cells).ToArray();
            Assert.Equal(1, quaed.Length);
            Assert.Equal(new HashSet<int> { 1 ,5, 6, 8 }, quaed[0].Key, new SetEqualityComparer<int>());
            Assert.Equal(4, quaed[0].Value.Count);

            var rc = Processor.SetRules(cells, Processor.FindQuads);
            Assert.True(rc);
            Assert.True(cells[0].EqualVariants(new[] { 1, 5 }));
            Assert.True(cells[1].EqualVariants(new[] { 2, 4 }));
            Assert.True(cells[2].EqualVariants(new[] { 2, 4, 7 }));
            Assert.True(cells[3].EqualVariants(new[] { 1, 5, 6, 8 }));
            Assert.True(cells[4].EqualVariants(new[] { 1, 5, 6, 8 }));
            Assert.True(cells[5].EqualVariants(new[] { 3, 7 }));
            Assert.True(cells[6].EqualVariants(new[] { 1, 6 }));
            Assert.True(cells[8].EqualVariants(new[] { 3, 4 }));
        }

        [Fact]
        public void TestHiddenPairs()
        {
            var cells = new[]
            {
                new Cell(0,0,4, 5, 8, 9),
                new Cell(0,0,2, 3, 4, 5, 6, 7, 9),
                new Cell(0,0,3, 4, 5, 6, 7, 9),
                new Cell(0,0,5, 8),
                new Cell(0,0,2, 3, 5),
                new Cell(0,0,3, 5),
                new Cell(0,0,1),
                new Cell(0,0,2, 3, 5, 9),
                new Cell(0,0,3, 5, 9),
            };

            var pairs = Processor.FindHiddenPairs(cells).ToArray();
            Assert.Equal(1, pairs.Length);
            Assert.Equal(new HashSet<int> { 6, 7 }, pairs[0].Key, new SetEqualityComparer<int>());
            Assert.Equal(2, pairs[0].Value.Count);

            var rc = Processor.HiddenSetRules(cells, Processor.FindHiddenPairs);
            Assert.True(rc);

            Assert.True(cells[0].EqualVariants(new[] { 4, 5, 8, 9 }));
            Assert.True(cells[1].EqualVariants(new[] { 7, 6 }));
            Assert.True(cells[2].EqualVariants(new[] { 7, 6 }));
            Assert.True(cells[3].EqualVariants(new[] { 5, 8 }));
            Assert.True(cells[4].EqualVariants(new[] { 2, 3, 5 }));

            cells = new[]
            {
                new Cell(0,0,5, 6),
                new Cell(0,0,3, 5, 6),
                new Cell(0,0,1),
                new Cell(0,0,2, 4, 5,6),
                new Cell(0,0,2, 3, 4, 6, 7),
                new Cell(0,0,3, 5, 7),
                new Cell(0,0,9),
                new Cell(0,0,5, 7),
                new Cell(0,0,8),
            };

            rc = Processor.HiddenSetRules(cells, Processor.FindHiddenPairs);
            Assert.True(rc);

            Assert.True(cells[3].EqualVariants(new[] { 2, 4 }));
            Assert.True(cells[4].EqualVariants(new[] { 2, 4 }));
       }

        [Fact]
        public void TestHiddenTriads()
        {
            var cells = new[]
            {
                new Cell(0,0,4, 7, 8, 9),
                new Cell(0,0, 4, 8, 9),
                new Cell(0,0,4, 7),
                new Cell(0,0,2,4,5,6,7,8),
                new Cell(0,0,4,7,8),
                new Cell(0,0,1),
                new Cell(0,0,2, 4, 6, 9),
                new Cell(0,0,3),
                new Cell(0,0,2,4,5,7,8,9)
            };

            var rc = Processor.HiddenSetRules(cells, Processor.FindHiddenTriads);
            Assert.True(rc);

            Assert.True(cells[3].EqualVariants(new[] { 2, 5, 6 }));
            Assert.True(cells[6].EqualVariants(new[] { 2, 6 }));
            Assert.True(cells[8].EqualVariants(new[] { 2, 5 }));

            cells = new[]
            {
                new Cell(0,0,2,5),
                new Cell(0,0,4,5,7,8),
                new Cell(0,0,2,4,7,8,9),
                new Cell(0,0,1,5),
                new Cell(0,0,6),
                new Cell(0,0,2,4,8,9),
                new Cell(0,0,1,2),
                new Cell(0,0,3),
                new Cell(0,0,1, 2, 9), 
            };

            rc = Processor.HiddenSetRules(cells, Processor.FindHiddenTriads);
            Assert.True(rc);
            Assert.True(cells[1].EqualVariants(new[] { 4, 7, 8 }));
            Assert.True(cells[2].EqualVariants(new[] { 4, 7, 8 }));
            Assert.True(cells[5].EqualVariants(new[] { 4, 8 }));
        }

        [Fact]
        public void TestHiddenQuad()
        {
            var cells = new[]
            {
                new Cell(0,0,1,3,4,6,7,8,9),
                new Cell(0,0,3, 7, 8),
                new Cell(0,0,3,4,6,7,8,9),
                new Cell(0,0,2, 3,7,8),
                new Cell(0,0,2,3,5,7,8),
                new Cell(0,0,2,3,5,7,8),
                new Cell(0,0,1,3,4,7,8,9),
                new Cell(0,0,3, 5,7,8),
                new Cell(0,0,3,4,5,7,8,9),
            };

            var rc = Processor.HiddenSetRules(cells, Processor.FindHiddenQuads);
            Assert.True(rc);

            Assert.True(cells[0].EqualVariants(new[] { 1,4,6,9 }));
            Assert.True(cells[2].EqualVariants(new[] { 4, 6, 9 }));
            Assert.True(cells[6].EqualVariants(new[] { 1, 4, 9 }));
            Assert.True(cells[8].EqualVariants(new[] { 4, 9 }));
        }

        [Theory]
        [MemberData("SolverDataSource")]
        public void TestSolver(string fileName, ComplexityLevel expectedLevel)
        {
            var grid = Grid.LoadFromFile(fileName);

            ComplexityLevel level;
            int tryCount;
            var rc = Processor.Solve(grid, null, out level, out tryCount);

            
            Assert.True(rc);
            Assert.True(grid.IsValid);
            Assert.Equal(expectedLevel, level);
        }

        public static IEnumerable<object[]> SolverDataSource
        {
            get
            {
                yield return new object[] { "easy.txt", ComplexityLevel.Easy };
                yield return new object[] { "medium.txt", ComplexityLevel.Medium };
                yield return new object[] { "Hard.txt", ComplexityLevel.Hard };
                yield return new object[] { "Samurai.txt", ComplexityLevel.Samurai };
            }
        }
    }
}
