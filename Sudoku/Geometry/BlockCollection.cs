using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Zabavnov.Sudoku
{
    public class BlockCollection: IEnumerable<Block>
    {
        private readonly Cell[,] _storage;

        public BlockCollection(Cell[,] storage)
        {
            Contract.Requires(storage != null);
            Contract.Requires(storage.GetLength(0) == Grid.LENGTH);
            Contract.Requires(storage.GetLength(1) == Grid.LENGTH);

            _storage = storage;
        }

        /// <summary>
        /// Gets <see cref="Block"/> by index from 0 to 8
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Block this[int index]
        {
            get
            {
                Contract.Requires(index >= 0);
                Contract.Requires(index < Grid.LENGTH);
                Contract.Ensures(Contract.Result<Block>() != null);

                int row = index / 3;
                int column = index % 3;

                return new Block(_storage, row * 3, column * 3);
            }
        }

        public Block this[int row, int column]
        {
            get
            {
                Contract.Requires(row >= 0);
                Contract.Requires(row < Grid.LENGTH );
                Contract.Requires(column >= 0);
                Contract.Requires(column < Grid.LENGTH );

                Contract.Ensures(Contract.Result<Block>() != null);
                var r = (row / 3) * 3;
                var c = (column / 3) * 3;
                return new Block(_storage, r, c);
            }
        }

        public IEnumerator<Block> GetEnumerator()
        {
            for (int i = 0; i < Grid.LENGTH; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal bool IsValid => this.All(block => block.IsValid());
    }
}
