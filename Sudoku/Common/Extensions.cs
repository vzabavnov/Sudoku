using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Zabavnov.Sudoku
{
    public static class Extensions
    {
        [Pure]
        public static bool Between<T>(this T value, T from, T to) where T: IComparable<T>
        {
            return value.CompareTo(from) >= 0 && value.CompareTo(to) <= 0;
        }

        public static int GetHashCode(this ISet<int> source)
        {
            Contract.Requires(source != null);

            return source.OrderBy(z => z).Aggregate(0, (i, arg2) => arg2.GetHashCode() ^ 397 ^ i.GetHashCode());
        }

        public static T Pop<T>(this IList<T> source)
        {
            Contract.Requires(source != null);

            if (source.Count == 0)
            {
                throw new IndexOutOfRangeException();
            }

            var value = source[source.Count - 1];
            source.RemoveAt(source.Count - 1);
            return value;
        }

        [Pure]
        public static bool IsValid(this IEnumerable<Cell> cells)
        {
            Contract.Requires(cells != null);

            IList<Cell> en = cells as IList<Cell> ?? cells.ToList();

            ISet<int> set = new HashSet<int>();

            // check duplicate values
            foreach (var cell1 in en.Where(z => z.IsCompleted))
            {
                Contract.Assume(cell1.Value != null);
                if(set.Contains(cell1.Value.Value))
                {
                    Trace.WriteLine($"Duplicate values in {{{en.ToString(", ")}}}");
                    return false;
                }
                set.Add(cell1.Value.Value);
            }

            if (set.Count != Grid.LENGTH)
            {
                // check when variants contains any cell's values
                var values = en.Where(z => z.IsCompleted).Select(z => z.Value ?? 0).ToSet();
                if (en.Where(z => z.IsEmpty).Any(z => z.HasVariants(values)))
                {
                    Trace.WriteLine($"The values presented in Variants in {{{en.ToString(", ")}}}");
                    return false;
                }
            }
            
            return true;
        }

        [DebuggerStepThrough]

        public static bool AllEqual<T>(this IEnumerable<T> source, out T value, IEqualityComparer<T> comparer = null)
        {
            Contract.Requires(source != null);

            return AllEqual(source, z => z, out value, comparer);
        }

        public static bool AllEqual<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, out TKey value, IEqualityComparer<TKey> comparer = null)
        {
            Contract.Requires(source != null);

            if (comparer == null)
            {
                comparer = EqualityComparer<TKey>.Default;
            }

            using (var en = source.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    value = selector(en.Current);

                    while (en.MoveNext())
                    {
                        if (!comparer.Equals(value, selector(en.Current)))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                value = default(TKey);
                return true;
            }
        }
        public static void AttachActionTo<T>(this T instance, string propertyName, Action action) where T: INotifyPropertyChanged
        {
            instance.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                {
                    action();
                }
            };
        }

        public static IEnumerable<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> source)
        {
            Contract.Requires(source != null);

            foreach (var item in source)
            {
                collection.Add(item);
            }

            return collection;
        }

        [DebuggerStepThrough]

        public static ISet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            Contract.Ensures(Contract.Result<ISet<T>>() != null);

            return comparer == null ? new HashSet<T>(source) : new HashSet<T>(source, comparer);
        }

        public static TResult Min<TSource, TKey, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TResult> resultSelector, IComparer<TKey> comparer = null)
        {
            Contract.Requires(source != null);

            if (comparer == null)
            {
                comparer = Comparer<TKey>.Default;
            }

            using (var en = source.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    var min = en.Current;

                    while (en.MoveNext())
                    {
                        if (comparer.Compare(keySelector(min), keySelector(en.Current)) > 0)
                        {
                            min = en.Current;
                        }
                    }

                    return resultSelector(min);
                }
            }

            return default(TResult);
        }

        [DebuggerStepThrough]
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            Contract.Requires(source != null);

            foreach (var item in source)
            {
                action(item);
            }
        }

        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TKey>.Default;
            }

            var set = new HashSet<TKey>(comparer);

            foreach (var item in source)
            {
                var key = keySelector(item);
                if (!set.Contains(key))
                {
                    yield return item;
                    set.Add(key);
                }
            }
        }

        public static string ToString<T>(this IEnumerable<T> source, string separator)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return ToString(source, z => z, separator);
        }

        public static string ToString<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, string separator)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var sb = new StringBuilder();

            using (var en = source.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    sb.Append(selector(en.Current));
                }

                while (en.MoveNext())
                {
                    sb.Append(separator);
                    sb.Append(selector(en.Current));
                }
            }
           
            return sb.ToString();
        }

        public static string AsString<T>(this ISet<T> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return source.ToString(", ");
        }
    }
}
