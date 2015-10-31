using System.Collections.Generic;

namespace Zabavnov.Sudoku
{
    public interface ICellCollection: IEnumerable<Cell>
    {
        bool IsCompleted { get; }
    }
}