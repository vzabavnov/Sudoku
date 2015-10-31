using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Zabavnov.Sudoku;

namespace Sudoku.Tests
{
    public class GridTests
    {
        [Theory]
        [MemberData("TestLoadSource")]
        public void TestLoad(string text)
        {
            var grid = Grid.Load(text);

            var rows = text.Split('\n');
            for (int r = 0; r < 9; r++)
            {
                var row = rows[r];
                Assert.True(row.Length >= 9);

                for (int c = 0; c < 9; c++)
                {
                    int value;
                    if (int.TryParse(row[c].ToString(), out value))
                    {
                        Assert.Equal(value, grid.Cells[r, c].Value);
                    }
                    else
                    {
                        Assert.Null(grid.Cells[r, c].Value);
                    }
                }
            }
        }

        public static IEnumerable<object[]> TestLoadSource()
        {
            var files = new[] { "easy.txt", "Hard.txt", "Medium.txt", "Samurai.txt" };

            return files.Select(File.ReadAllText).Select(text => new object[] {text});
        }
    }
}
