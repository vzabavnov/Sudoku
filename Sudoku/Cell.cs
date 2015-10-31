using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Zabavnov.Sudoku
{
    public class Cell: IEquatable<Cell>, IEnumerable<int>
    {
        private int? _value;
        private ISet<int> _variants = new HashSet<int>();

        public int? Value => _value ?? (_variants.Count == 1 ? _variants.First() : (int?)null);

        public bool IsEmpty => !Value.HasValue;

        public bool IsCompleted => Value.HasValue;

        public bool IsSolved => _variants.Count == 1;

        public bool Equals(Cell other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof (Cell))
            {
                return false;
            }
            return Equals((Cell)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Row ^ 397 ^ Column;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static bool operator ==(Cell left, Cell right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Cell left, Cell right)
        {
            return !Equals(left, right);
        }

        public int Row { get; }

        public int Column { get; }

        #region constructors

        public Cell(int row, int column)
        {
            Contract.Requires(row.Between(0, Grid.LENGTH));
            Contract.Requires(column.Between(0, Grid.LENGTH));

            Row = row;
            Column = column;
            _variants.AddRange(Enumerable.Range(1, Grid.LENGTH));
        }

        public Cell(int row, int column, int value)
        {
            Contract.Requires(row.Between(0, Grid.LENGTH));
            Contract.Requires(column.Between(0, Grid.LENGTH));

            Row = row;
            Column = column;
            _value = value;
        }

        public Cell(int row, int column, int v1, int v2, params int[] values)
        {
            Contract.Requires(row.Between(0, Grid.LENGTH));
            Contract.Requires(column.Between(0, Grid.LENGTH));
            Contract.Requires(values != null);

            Row = row;
            Column = column;

            _variants.AddRange(new[] {v1, v2}.Union(values));
        }

        #endregion

        public event Action<Cell> ValueChanged;

        public IEnumerator<int> GetEnumerator()
        {
            return _variants.GetEnumerator();
        }

        public override string ToString()
        {
            if (Value.HasValue)
            {
                return Value.Value.ToString();
            }

            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var item in _variants.OrderBy(z => z))
            {
                sb.Append(item);
            }
            sb.Append("]");

            return sb.ToString();
        }

        public void Initialize(int value)
        {
            _value = value;
            _variants.Clear();
        }

        internal void Initialize(Cell cell)
        {
            Contract.Requires(cell != null);

            _value = cell.Value;
            _variants.Clear();
            _variants.AddRange(cell._variants);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_variants != null);
            Contract.Invariant((_value.HasValue && !_variants.Any()) || (!_value.HasValue && _variants.Any()));
        }

        [Pure]
        public bool HasVariants(IEnumerable<int> variants)
        {
            Contract.Requires(!IsCompleted);
            Contract.Requires(variants != null);

            return _variants.Intersect(variants).Any();
        }

        [Pure]
        public bool EqualVariants(IEnumerable<int> variants)
        {
            Contract.Requires(!IsCompleted);
            Contract.Requires(variants != null);

            return _variants.SetEquals(variants);
        }


        public bool RemoveVariants(IEnumerable<int> variants)
        {
            Contract.Requires(!IsCompleted);
            Contract.Requires(variants != null);
            Contract.Ensures(_variants.Count >= 1);

            var newSet = _variants.Except(variants).ToSet();

            if (newSet.Count == 0)
            {
                throw new SudokuException("zero variants left after removing of candidates");
            }

            var ret = newSet.Count != _variants.Count;

            _variants = newSet;

            if (_variants.Count == 1)
            {
                ValueChanged?.Invoke(this);
            }

            return ret;
        }

        public void PromoteValue(int value)
        {
            Contract.Requires(IsEmpty);
            Contract.Requires(value.Between(1, 9));
            Contract.Requires(this.Count() > 1);
            Contract.Ensures(_variants.Count >= 1);

            _variants.IntersectWith(new[] {value});

            ValueChanged?.Invoke(this);
       }

        public bool IntersectVariants(IEnumerable<int> variants)
        {
            Contract.Requires(!IsCompleted);
            Contract.Requires(variants != null);
            Contract.Ensures(_variants.Count >= 1);

            var newSet = _variants.Intersect(variants).ToSet();

            if (newSet.Count == 0)
            {
                throw new SudokuException("zero variants left after removing of candidates");
            }

            var ret = newSet.Count != _variants.Count;

            _variants = newSet;

            if (_variants.Count == 1)
            {
                ValueChanged?.Invoke(this);
            }

            return ret;
        }
    }
}
