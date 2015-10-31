namespace Zabavnov.Sudoku
{
    public class UnsolvedException : SudokuException
    {
        public UnsolvedException(): base("Unsolvable")
        {
        }
    }
}