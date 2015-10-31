using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Zabavnov.Sudoku.Properties;

namespace Zabavnov.Sudoku
{
    static class Processor
    {
        #region Arrange

        public static bool AdjustCells(IEnumerable<Cell> cells)
        {
            Contract.Requires(cells != null);

            var ret = false;
            var enumerable = cells as IList<Cell> ?? cells.ToList();

            bool repeat;
            do
            {
                repeat = false;
                var set = enumerable.Where(z => z.IsCompleted).Select(z => z.Value ?? 0).ToSet();
                foreach (var cell in enumerable.Where(z => z.IsEmpty))
                {
                    if (cell.RemoveVariants(set))
                    {
                        ret = true;
                    }

                    if (cell.IsSolved)
                    {
                        repeat = true;
                    }
                }
            } while (repeat);

            Contract.Assume(enumerable.IsValid());

            return ret;
        }

        #endregion

        #region Order

        /// <summary>
        /// Order rows by least complexity first
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static IEnumerable<Row> OrderRows(this Grid grid)
        {
            Contract.Requires(grid != null);

            return grid.Rows.Where(z => !z.IsCompleted).OrderBy(z => z.Count(zz => zz.IsEmpty));
        }

        public static IEnumerable<Column> OrderColumns(this Grid grid)
        {
            Contract.Requires(grid != null);

            return grid.Columns.Where(z => !z.IsCompleted).OrderBy(z => z.Count(zz => zz.IsEmpty));
        }

        public static IEnumerable<Block> OrderBlocks(this Grid grid)
        {
            Contract.Requires(grid != null);

            return grid.Blocks.Where(z => !z.IsCompleted).OrderBy(z => z.Count(zz => zz.IsEmpty));
        }

        #endregion

        /// <summary>
        /// check for cell with unique candidate 
        /// </summary>
        /// <param name="cells"></param>
        /// <returns></returns>
        public static bool LastOneRule(IEnumerable<Cell> cells)
        {
            Contract.Requires(cells != null);

            var enumerable = cells as IList<Cell> ?? cells.ToList();
            Contract.Assume(enumerable.IsValid());

            var item = enumerable.Where(z => z.IsEmpty).SelectMany(z => z.Select(zz => new { Key = zz, Cell = z })).GroupBy(z => z.Key, z => z.Cell)
                .FirstOrDefault(z => z.Count() == 1);

            var ret = false;

            if(item != null)
            {
                var cell = item.First();

                Contract.Assert(enumerable.Where(z => z.IsCompleted).All(z => z.Value != item.Key));

                cell.PromoteValue(item.Key);
                Contract.Assume(enumerable.IsValid());

                ret = true;
            }

            return ret;
        }

        public static bool SetRules(IEnumerable<Cell> cells, Func<IEnumerable<Cell>, IEnumerable<KeyValuePair<ISet<int>, IList<Cell>>>> provider)
        {
            Contract.Requires(cells != null);
            Contract.Requires(provider != null);

            var ret = false;

            var enumerable = cells as IList<Cell> ?? cells.ToList();

            var en = provider(enumerable);
            Contract.Assume(en != null);
            var k = en.FirstOrDefault();
            if (k.Key != null)
            {
                foreach (var cell in enumerable.Where(z => z.IsEmpty && !k.Value.Contains(z, ReferenceEqualityComparer<Cell>.Default)))
                {
                    if (cell.RemoveVariants(k.Key))
                    {
                        ret = true;
                    }
                }
            }

            return ret;
        }

        public static bool HiddenSetRules(IEnumerable<Cell> cells, Func<IEnumerable<Cell>, IEnumerable<KeyValuePair<ISet<int>, IList<Cell>>>> provider)
        {
            Contract.Requires(cells != null);
            Contract.Requires(provider != null);

            var enumerable = cells as IList<Cell> ?? cells.ToList();
            var ret = false;

            var k = provider(enumerable).FirstOrDefault();
            if(k.Key != null)
            {
                var value = k.Value;
                Contract.Assume(value != null);
                foreach (var cell in value)
                {
                    if (cell.IntersectVariants(k.Key))
                    {
                        ret = true;
                    }
                }
            }

            Contract.Assume(enumerable.IsValid());

            return ret;
        }

        internal static bool  UseRules(Grid grid, Func<IEnumerable<Cell>, bool> rule)
        {
            Contract.Requires(grid != null);

            var changes = false;

            var old = grid.Disabled;

            grid.Disabled = false;

            try
            {
                // arrange blocks
                foreach (var block in grid.OrderBlocks())
                {
                    if (rule(block))
                    {
                        changes = true;
                    }
                }

                if (changes)
                {
                    return true;
                }

                // arrange columns
                foreach (var col in grid.OrderColumns())
                {
                    if (rule(col))
                    {
                        changes = true;
                    }
                }

                if (changes)
                {
                    return true;
                }

                // arrange rows
                foreach (var row in grid.OrderRows())
                {
                    if (rule(row))
                    {
                        changes = true;
                    }
                }

                if (changes)
                {
                    return true;
                }

            }
            finally
            {
                grid.Disabled = old;
            }

            return changes;
        }

        static void UpliftLevel(ref ComplexityLevel level, ComplexityLevel newValue)
        {
            if (level != ComplexityLevel.Samurai && (int)newValue > (int)level)
            {
                level = newValue;
            }
        }

        public static bool UsePredictableRules(Grid grid, ref ComplexityLevel complexityLevel)
        {
            bool changed;

            do
            {
                // easy level
                changed = UseRules(grid, AdjustCells);

                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                grid.Disabled = false;

                Contract.Assert(grid.IsValid);

                // middle level
                UpliftLevel(ref complexityLevel, ComplexityLevel.Medium);

                changed = UseRules(grid, LastOneRule);
                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                Contract.Assert(grid.IsValid);

                changed = UseRules(grid, cells => SetRules(cells, FindPairs));
                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                Contract.Assert(grid.IsValid);

                // hard level
                UpliftLevel(ref complexityLevel, ComplexityLevel.Hard);

                changed = UseRules(grid, cells => SetRules(cells, FindTriads));
                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                Contract.Assert(grid.IsValid);

                changed = UseRules(grid, cells => SetRules(cells, FindQuads));
                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                Contract.Assert(grid.IsValid);

                changed = UseRules(grid, cells => HiddenSetRules(cells, FindHiddenPairs));
                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                Contract.Assert(grid.IsValid);

                changed = UseRules(grid, cells => HiddenSetRules(cells, FindHiddenTriads));
                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                Contract.Assert(grid.IsValid);

                changed = UseRules(grid, cells => HiddenSetRules(cells, FindHiddenQuads));
                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                Contract.Assert(grid.IsValid);

                // impossible level
                UpliftLevel(ref complexityLevel, ComplexityLevel.Samurai);

                changed = BlockJunctions(grid);
                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                Contract.Assert(grid.IsValid);

                changed = ColumnJunction(grid);
                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                changed = RowJunction(grid);
                if (changed)
                {
                    if (grid.IsCompleted)
                    {
                        return true;
                    }
                    continue;
                }

                Contract.Assert(grid.IsValid);

            } while (changed);

            return false;
        }

        public static bool Solve(Grid grid, Action<Grid> progressAction, out ComplexityLevel complexityLevel, out int tryCount)
        {
            complexityLevel = ComplexityLevel.Easy;

            if (UsePredictableRules(grid, ref complexityLevel))
            {
                tryCount = 0;
                return true;
            }

            var ret = Examination(grid, progressAction, out tryCount);
            if (ret)
            {
                complexityLevel = tryCount < 2 ? ComplexityLevel.Hard : ComplexityLevel.Samurai;
            }
            else
            {
                complexityLevel = ComplexityLevel.Samurai;
            }

            return ret;
        }

        private static bool Examination(Grid grid, Action<Grid> progressAction, out int tryCount)
        {
            Contract.Requires(grid != null);
            Contract.Requires(grid.IsValid);

            tryCount = 0;
            return Examination(grid, progressAction, 0, ref tryCount);
        }

        private static bool Examination(Grid grid, Action<Grid> progressAction, int depth, ref int tryCount)
        {
            if (depth >= Settings.Default.MaxDepth)
            {
                return false;
            }

            var cells = grid.Where(z => z.IsEmpty).OrderBy(z => z.Count())
                .Select(z => new
                {
                    Keys = z.ToSet(),
                    z.Row,
                    z.Column
                }).ToArray();

            foreach (var cell in cells)
            {
                foreach (var key in cell.Keys)
                {
                    tryCount++;

                    var newGrid = grid.Clone();
                    progressAction?.Invoke(newGrid);

                    newGrid[cell.Row, cell.Column].PromoteValue(key);

                    ComplexityLevel level = ComplexityLevel.Samurai;

                    try
                    {
                        if (UsePredictableRules(newGrid, ref level))
                        {
                            grid.Promote(newGrid);
                            progressAction?.Invoke(grid);
                            return true;
                        }
                    }
                    catch (SudokuException)
                    {
                        continue;    
                    }

                    if (Examination(newGrid, progressAction, depth + 1, ref tryCount))
                    {
                        grid.Promote(newGrid);
                        progressAction?.Invoke(grid);
                        return true;
                    }
                }
            }

            progressAction?.Invoke(grid);

            return false;
        }

        #region Find Hidden Set

        public static IEnumerable<KeyValuePair<ISet<int>, IList<Cell>>> FindHiddenPairs(IEnumerable<Cell> cells)
        {
            Contract.Requires(cells != null);

            var en = cells.Where(z => z.IsEmpty).ToList();
            var set = new HashSet<ISet<int>>(new SetEqualityComparer<int>());
            foreach (var cell in en)
            {
                foreach (var inCell in en.Where(z => !ReferenceEquals(z, cell)))
                {
                    var keys = cell.Intersect(inCell)
                            .Except(en.Where(z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell)).SelectMany(z => z))
                            .ToSet();

                    if (keys.Count == 2)
                    {
                        if (!set.Contains(keys) && en.Where(z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell)).All(z => !z.Intersect(keys).Any()))
                        {
                            yield return new KeyValuePair<ISet<int>, IList<Cell>>(keys, new [] { cell, inCell });
                            set.Add(keys);
                        }
                    }
                }
            }
        }

        public static IEnumerable<KeyValuePair<ISet<int>, IList<Cell>>> FindHiddenTriads(IEnumerable<Cell> cells)
        {
            Contract.Requires(cells != null);

            var en = cells.Where(z => z.IsEmpty).ToList();
            var set = new HashSet<ISet<int>>(new SetEqualityComparer<int>());
            foreach (var cell in en)
            {
                foreach (var inCell in en.Where(z => !ReferenceEquals(z, cell)).Where(z => z.Intersect(cell).Any()))
                {
                    var union = cell.Union(inCell).ToSet();
                    foreach (var deepCell in en.Where(z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell)).Where(z => z.Intersect(union).Any()))
                    {
                        var union2 = union.Union(deepCell);

                        var keys =
                            union2.Except(en.Where(z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell) && !ReferenceEquals(z, deepCell)).SelectMany(z => z))
                                  .ToSet();

                        if (keys.Count == 3)
                        {
                            if (!set.Contains(keys) &&
                                en.Where(z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell) && !ReferenceEquals(z, deepCell))
                                  .All(z => !z.Intersect(keys).Any()))
                            {
                                yield return new KeyValuePair<ISet<int>, IList<Cell>>(keys, new [] { cell, inCell, deepCell });
                                set.Add(keys);
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<KeyValuePair<ISet<int>, IList<Cell>>> FindHiddenQuads(IEnumerable<Cell> cells)
        {
            Contract.Requires(cells != null);

            var en = cells.Where(z => z.IsEmpty).ToList();
            var set = new HashSet<ISet<int>>(new SetEqualityComparer<int>());
            foreach (var cell in en)
            {
                foreach (var inCell in en.Where(z => !ReferenceEquals(z, cell)).Where(z => z.Intersect(cell).Any()))
                {
                    var union = cell.Union(inCell).ToSet();

                    foreach (var deepCell in en.Where(z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell)).Where(z => z.Intersect(union).Any()))
                    {
                        var union2 = union.Union(deepCell).ToSet();

                        foreach (
                            var veryDeepCell in
                                en.Where(z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell) && !ReferenceEquals(z, deepCell))
                                  .Where(z => z.Intersect(union2).Any()))
                        {
                            var keys =
                                union2.Union(veryDeepCell)
                                      .Except(
                                          en.Where(
                                              z =>
                                                  !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell) && !ReferenceEquals(z, deepCell) &&
                                                  !ReferenceEquals(z, veryDeepCell)).SelectMany(z => z))
                                      .ToSet();
                            if (keys.Count == 4)
                            {
                                if (!set.Contains(keys) &&
                                    en.Where(
                                        z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell) && !ReferenceEquals(z, deepCell) && !ReferenceEquals(z, veryDeepCell))
                                      .All(z => !z.Intersect(keys).Any()))
                                {
                                    yield return new KeyValuePair<ISet<int>, IList<Cell>>(keys, new [] { cell, inCell, deepCell, veryDeepCell });
                                    set.Add(keys);
                                }
                            }
                        }
                    }
                }
            }
        }


        #endregion

        #region Find Set

        public static IEnumerable<KeyValuePair<ISet<int>, IList<Cell>>> FindPairs(IEnumerable<Cell> cells)
        {
            Contract.Requires(cells != null);

            return cells.Where(z => z.IsEmpty)
                     .Where(z => z.Count() == 2)
                     .GroupBy(z => z.ToSet(), (key, e) => new KeyValuePair<ISet<int>, IList<Cell>>(key, e.ToList()), SetEqualityComparer<int>.Default)
                     .Where(z => z.Value.Count == 2)
                     .Distinct(z => z.Key, SetEqualityComparer<int>.Default);
        }

        public static IEnumerable<KeyValuePair<ISet<int>, IList<Cell>>> FindTriads(IEnumerable<Cell> cells)
        {
            Contract.Requires(cells != null);

            var en = cells.Where(z => z.IsEmpty && z.Count().Between(2, 3)).ToList();
            var set = new HashSet<ISet<int>>(new SetEqualityComparer<int>());
            foreach (var cell in en)
            {
                foreach (var inCell in en.Where(z => !ReferenceEquals(z, cell)))
                {
                    if (inCell.HasVariants(cell))
                    {
                        var union = inCell.Union(cell).ToSet();

                        if (union.Count <= 3)
                        {
                            foreach (var deepCell in en.Where(z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell)))
                            {
                                if (deepCell.Intersect(union).Any())
                                {
                                    var keys = union.Union(deepCell).ToSet();
                                    if (keys.Count <= 3 && !set.Contains(keys))
                                    {
                                        yield return new KeyValuePair<ISet<int>, IList<Cell>>(keys, new [] { cell, inCell, deepCell });
                                        set.Add(keys);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<KeyValuePair<ISet<int>, IList<Cell>>> FindQuads(IEnumerable<Cell> cells)
        {
            Contract.Requires(cells != null);

            var en = cells.Where(z => z.IsEmpty && z.Count().Between(2, 4)).ToList();
            var set = new HashSet<ISet<int>>(new SetEqualityComparer<int>());
            foreach (var cell in en)
            {
                foreach (var inCell in en.Where(z => !ReferenceEquals(z, cell)))
                {
                    if (inCell.Intersect(cell).Any())
                    {
                        var union = inCell.Union(cell).ToSet();

                        if (union.Count <= 4)
                        {
                            foreach (var deepCell in en.Where(z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell)))
                            {
                                if (deepCell.Intersect(union).Any())
                                {
                                    var union2 = union.Union(deepCell).ToSet();
                                    if (union2.Count <= 4)
                                    {
                                        foreach (var veryDeepCell in en.Where(z => !ReferenceEquals(z, cell) && !ReferenceEquals(z, inCell) && !ReferenceEquals(z, deepCell)))
                                        {
                                            if (veryDeepCell.Intersect(union2).Any())
                                            {
                                                var keys = union2.Union(veryDeepCell).ToSet();

                                                if (keys.Count <= 4 && !set.Contains(keys))
                                                {
                                                    yield return new KeyValuePair<ISet<int>, IList<Cell>>(keys, new [] { cell, inCell, deepCell, veryDeepCell });
                                                    set.Add(keys);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        #endregion

        #region Junctions

        public static bool BlockJunctions(Grid grid)
        {
            Contract.Requires(grid != null);

            var ret = false;

            foreach (var area in grid.Blocks)
            {
                var blocks = FindPairs(area).Union(FindTriads(area));

                foreach (var block in blocks)
                {
                    int index;
                    if (block.Value.AllEqual(z => z.Column, out index))
                    {
                        var column = grid.Columns[index];
                        foreach (var cell in column.Where(z => z.IsEmpty).Where(z => !block.Value.Contains(z, ReferenceEqualityComparer<Cell>.Default)))
                        {
                            if (cell.RemoveVariants(block.Key))
                            {
                                ret = true;
                            }
                        }
                    }
                    else if (block.Value.AllEqual(z => z.Row, out index))
                    {
                        var row = grid.Rows[index];
                        foreach (var cell in row.Where(z => z.IsEmpty).Where(z => !block.Value.Contains(z, ReferenceEqualityComparer<Cell>.Default)))
                        {
                            if (cell.RemoveVariants(block.Key))
                            {
                                ret = true;
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public static bool ColumnJunction(Grid grid)
        {
            Contract.Requires(grid != null);

            var ret = false;
            for (int i = 0; i < Grid.LENGTH; i++)
            {
                var column = grid.Columns[i];
                var bloks = FindPairs(column).Union(FindTriads(column));

                foreach (var block in bloks)
                {
                    int row;
                    if (block.Value.Select(z => (z.Row / 3) * 3).AllEqual(out row))
                    {
                        foreach (var cell in grid.Blocks[row, i].Where(z => z.IsEmpty).Where(z => !block.Value.Contains(z, ReferenceEqualityComparer<Cell>.Default)))
                        {
                            if (cell.RemoveVariants(block.Key))
                            {
                                ret = true;
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public static bool RowJunction(Grid grid)
        {
            Contract.Requires(grid != null);

            var ret = false;
            for (int i = 0; i < Grid.LENGTH; i++)
            {
                var row = grid.Rows[i];
                var blocks = FindPairs(row).Union(FindTriads(row));

                foreach (var block in blocks)
                {
                    int column;
                    if (block.Value.Select(z => (z.Column / 3) * 3).AllEqual(out column))
                    {
                        foreach (var cell in grid.Blocks[i, column].Where(z => z.IsEmpty).Where(z => !block.Value.Contains(z, ReferenceEqualityComparer<Cell>.Default)))
                        {
                            if (cell.RemoveVariants(block.Key))
                            {
                                ret = true;
                            }
                        }
                    }
                }
            }

            return ret;
        }

        #endregion
    }
}
