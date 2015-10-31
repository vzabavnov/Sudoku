using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Zabavnov.Sudoku
{
    public class Row: ICellCollection
    {
        private readonly Cell[,] _storage;

        public int RowIndex { get; }

        internal Row(Cell[,] storage, int rowIndex)
        {
            Contract.Requires(storage != null);
            Contract.Requires(rowIndex >= 0);
            Contract.Requires(rowIndex < Grid.LENGTH);

            _storage = storage;
            RowIndex = rowIndex;
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (int i = 0; i < Grid.LENGTH; i++)
            {
                yield return _storage[RowIndex, i];
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

                var ret = _storage[RowIndex, index];
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
