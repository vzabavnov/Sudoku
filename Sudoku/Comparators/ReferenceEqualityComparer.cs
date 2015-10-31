using System.Collections.Generic;

namespace Zabavnov.Sudoku
{
    class ReferenceEqualityComparer<T>: IEqualityComparer<T>
    {
        public static ReferenceEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();
        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return 0;
        }
    }
}
