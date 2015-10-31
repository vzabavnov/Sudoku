using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Zabavnov.Sudoku
{
    public class Block: ICellCollection
    {
        private readonly Cell[,] _storage;

        public int StartRow { get; }

        public int StartColumn { get; }

        [Pure]
        public bool IsCompleted => this.All(z => z.IsCompleted);

        [Pure]
        internal bool IsValid => this.IsValid();

        public Cell this[int rowIndex, int colIndex]
        {
            get
            {
                Contract.Requires(rowIndex >= 0);
                Contract.Requires(rowIndex < 3);
                Contract.Requires(colIndex >= 0);
                Contract.Requires(colIndex < 3);
                Contract.Ensures(Contract.Result<Cell>() != null);

                var ret = _storage[rowIndex + StartRow, colIndex + StartColumn];
                Contract.Assume(ret != null);
                return ret;
            }
        }

        internal Block(Cell[,] storage, int startRow, int startColumn)
        {
            _storage = storage;
            StartRow = startRow;
            StartColumn = startColumn;
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    yield return _storage[row + StartRow, col + StartColumn];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    var cell = _storage[row + StartRow, col + StartColumn];
                    Contract.Assume(cell != null);
                    sb.Append(cell.Value?.ToString() ?? ".");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string AsText
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return ToString();
            }
        }
    }
}
