using System;

namespace Zabavnov.Sudoku
{
    public class SudokuException: Exception
    {
        public SudokuException(string format, params object[] args) :base(string.Format(format, args))
        {
        }
    }
}