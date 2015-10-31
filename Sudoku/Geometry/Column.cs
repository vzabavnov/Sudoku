using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Zabavnov.Sudoku
{
    public class Column: ICellCollection
    {
        private readonly Cell[,] _storage;

        public int ColumnIndex { get; }

        public Column(Cell[,] storage, int columnIndex)
        {
            Contract.Requires(storage != null);
            Contract.Requires(columnIndex >= 0);
            Contract.Requires(columnIndex < Grid.LENGTH);

            _storage = storage;
            ColumnIndex = columnIndex;
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (int i = 0; i < Grid.LENGTH; i++)
            {
                yield return _storage[i, ColumnIndex];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Cell this[int index]
        {
            get
            {
                Contract.Requires(index >= 0);
                Contract.Requires(index < Grid.LENGTH);
                Contract.Ensures(Contract.Result<Cell>() != null);

                var ret = _storage[index, ColumnIndex];
                Contract.Assume(ret != null);
                return ret;
            }
        }

        [Pure]
        public bool IsCompleted => this.All(z => z.IsCompleted);

        [Pure]
        internal bool IsValid => this.IsValid();

        public override string ToString()
        {
            return "{" + this.ToString(z => $"{{{z}}}", "") + "}";
        }

    }
}
