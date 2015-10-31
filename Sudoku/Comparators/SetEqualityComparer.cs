using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Zabavnov.Sudoku
{
    class SetEqualityComparer<T>: IEqualityComparer<ISet<T>>
    {
        public static IEqualityComparer<ISet<T>> Default = new SetEqualityComparer<T>();

        public bool Equals(ISet<T> x, ISet<T> y)
        {
            return x != null && y != null && x.SetEquals(y);
        }

        [DebuggerStepThrough]
        public int GetHashCode(ISet<T> obj)
        {
            Contract.Assume(obj != null);

            return obj.OrderBy(z => z).Aggregate(0, (i, arg2) => arg2.GetHashCode() ^ 397 ^ i.GetHashCode());
        }
    }
}
