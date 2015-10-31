using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Zabavnov.Sudoku
{
    public class RowCollection: IEnumerable<Row>
    {
        private readonly Cell[,] _storage;

        public RowCollection(Cell[,] storage)
        {
            Contract.Requires(storage != null);
            Contract.Requires(storage.GetLength(0) == Grid.LENGTH);
            Contract.Requires(storage.GetLength(1) == Grid.LENGTH);

            _storage = storage;
        }

        public IEnumerator<Row> GetEnumerator()
        {
            for (int i = 0; i < Grid.LENGTH; i++)
            {
                yield return new Row(_storage, i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Row this[int rowIndex]
        {
            get
            {
                Contract.Requires(rowIndex >= 0);
                Contract.Requires(rowIndex < Grid.LENGTH);
                Contract.Ensures(Contract.Result<Row>() != null);

                return new Row(_storage, rowIndex);
            }
        }

        internal bool IsValid => this.All(row => row.IsValid());
    }
}
