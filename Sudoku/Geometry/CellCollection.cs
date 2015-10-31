using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Zabavnov.Sudoku
{
    public class CellCollection: IEnumerable<Cell>
    {
        private readonly Cell[,] _storage;

        internal CellCollection(Cell[,] storage)
        {
            Contract.Requires(storage != null);
            Contract.Requires(storage.GetLength(0) == Grid.LENGTH);
            Contract.Requires(storage.GetLength(1) == Grid.LENGTH);

            _storage = storage;
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (int row = 0; row < Grid.LENGTH; row++)
            {
                for (int col = 0; col < Grid.LENGTH; col++)
                {
                    yield return _storage[row, col];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Cell this[int row, int column] 
        {
            [DebuggerStepThrough]
            get
            {
                Contract.Requires(row >= 0);
                Contract.Requires(row < Grid.LENGTH);
                Contract.Requires(column >= 0);
                Contract.Requires(column < Grid.LENGTH);
                Contract.Ensures(Contract.Result<Cell>() != null);

                var ret = _storage[row, column];
                Contract.Assume(ret != null);
                return ret;
            }
        }
    }
}
