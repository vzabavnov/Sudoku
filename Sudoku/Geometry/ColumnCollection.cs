using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Zabavnov.Sudoku
{
    public class ColumnCollection: IEnumerable<Column>
    {
        private readonly Cell[,] _storage;

        public ColumnCollection(Cell[,] storage)
        {
            Contract.Requires(storage != null);
            Contract.Requires(storage.GetLength(0) == Grid.LENGTH);
            Contract.Requires(storage.GetLength(1) == Grid.LENGTH);

            _storage = storage;
        }

        public IEnumerator<Column> GetEnumerator()
        {
            for (int i = 0; i < Grid.LENGTH; i++)
            {
                yield return new Column(_storage, i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Column this[int colIndex]
        {
            get
            {
                Contract.Requires(colIndex >= 0);
                Contract.Requires(colIndex < Grid.LENGTH);
                Contract.Ensures(Contract.Result<Column>() != null);

                return new Column(_storage, colIndex);
            }
        }

        internal bool IsValid  => this.All(column => column.IsValid());
    }
}
