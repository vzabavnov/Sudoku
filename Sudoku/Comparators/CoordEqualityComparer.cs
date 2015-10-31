using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Zabavnov.Sudoku
{
    class CoordEqualityComparer: IEqualityComparer<Cell>
    {
        public bool Equals(Cell x, Cell y)
        {
            Contract.Assume(x != null);
            Contract.Assume(y != null);

            return x.Row == y.Row && x.Column == y.Column;
        }

        public int GetHashCode(Cell cell)
        {
            Contract.Assume(cell != null);

            return cell.Row ^ 397 ^ cell.Column;
        }
    }
}
