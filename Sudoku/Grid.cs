using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace Zabavnov.Sudoku
{
    public class Grid: IEnumerable<Cell>
    {
        public const int LENGTH = 9;
        private readonly Cell[,] _storage = new Cell[LENGTH, LENGTH];

        public Grid()
        {
            for (int row = 0; row < LENGTH; row++)
            {
                for (int col = 0; col < LENGTH; col++)
                {
                    var cell = new Cell(row, col);
                    cell.ValueChanged += OnCellValueChanged;
                    _storage[row, col] = cell;
                }
            }

            Cells = new CellCollection(_storage);
            Rows = new RowCollection(_storage);
            Columns = new ColumnCollection(_storage);
            Blocks = new BlockCollection(_storage);
        }

        private void OnCellValueChanged(Cell cell)
        {
            Contract.Requires(cell != null);
            Contract.Requires(!cell.IsEmpty);

            if (Disabled)
            {
                return;
            }

            CellChanged?.Invoke(cell);
        }

        public event Action<Cell> CellChanged;

        #region Cells/Columns/Rows/Blocks access

        public CellCollection Cells {[DebuggerStepThrough] get; }

        public Cell this[int row, int column]
        {
            [DebuggerStepThrough]
            get
            {
                Contract.Requires(row >= 0);
                Contract.Requires(row < LENGTH);
                Contract.Requires(column >= 0);
                Contract.Requires(column < LENGTH);
                Contract.Ensures(Contract.Result<Zabavnov.Sudoku.Cell>() != null);

                return Cells[row, column];
            }
        }

        public RowCollection Rows { [DebuggerStepThrough] get; }

        public ColumnCollection Columns {[DebuggerStepThrough] get; }

        public BlockCollection Blocks {[DebuggerStepThrough] get; }

        #endregion

        public bool IsCompleted => Cells.All(z => z.IsCompleted);

        internal bool IsValid
        {
            get
            {
                return Disabled || Rows.IsValid && Columns.IsValid && Blocks.IsValid;
            }
        }

        #region Load / Save

        public static Grid Load(TextReader textStream)
        {
            Contract.Requires(textStream != null);
            Contract.Ensures(Contract.Result<Grid>() != null);

            return Load(textStream.ReadToEnd());
        }

        public static Grid LoadFromFile(string fileName)
        {
            Contract.Requires(!String.IsNullOrEmpty(fileName));
            Contract.Ensures(Contract.Result<Grid>() != null);

            using (var file = File.OpenText(fileName))
            {
                return Load(file);
            }
        }

        public static Grid Load(string context)
        {
            Contract.Requires(!String.IsNullOrEmpty(context));
            Contract.Ensures(Contract.Result<Grid>() != null);

            var grid = new Grid {Disabled = true};

            var rows = context.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (rows.Length != LENGTH)
            {
                throw new IOException("Not enough rows");
            }

            for (int row = 0; row < LENGTH; row++)
            {
                var line = rows[row];
                if (line.Length < LENGTH)
                {
                    throw new IOException("Not enough columns");
                }

                for (int col = 0; col < LENGTH; col++)
                {
                    int value;
                    if (int.TryParse(line[col].ToString(), out value))
                    {
                        grid.Cells[row, col].Initialize(value);
                    }
                }
            }

            return grid;
        }

        public void Save(TextWriter textStream)
        {
            for (int r = 0; r < LENGTH; r++)
            {
                for (int c = 0; c < LENGTH; c++)
                {
                    textStream.Write(Cells[r, c].Value?.ToString() ?? ".");
                }
                textStream.WriteLine();
            }
        }

        public string Save()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            using (var sw = new StringWriter())
            {
                Save(sw);
                return sw.ToString();
            }
        }

        public void SaveToFile(string fileName)
        {
            using (var file = File.OpenWrite(fileName))
            using (var sw = new StreamWriter(file))
            {
                Save(sw);
            }
        }

        #endregion

        #region IEnumerable

        public IEnumerator<Cell> GetEnumerator()
        {
            Contract.Assume(Cells != null);

            return Cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var row in Rows)
            {
                sb.Append(row);
                sb.AppendLine();
            }
            return sb.ToString();
        }


        public string AsText
        {
            [DebuggerStepThrough]
            get
            {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

                return ToString();
            }
        }

        private const string line = "|-+-+-+-+-+-+-+-+-|";
        public string AsGrid
        {
            get
            {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

                var sb = new StringBuilder();

                sb.AppendLine(line);
                
                for (int i = 0; i < LENGTH; i++)
                {
                    var row = Rows[i];
                    sb.Append('|');

                    for (int c = 0; c < LENGTH; c++)
                    {
                        sb.Append(row[c].Value?.ToString() ?? " ");
                        sb.Append('|');
                    }
                    sb.AppendLine();
                    sb.AppendLine(line);
                }
                return sb.ToString();
            }
        }

        public bool Disabled { get; set; }

        public Grid Clone()
        {
            Contract.Ensures(Contract.Result<Grid>() != null);

            var grid = new Grid {Disabled = true};

            for (int row = 0; row < LENGTH; row++)
            {
                for (int col = 0; col < LENGTH; col++)
                {
                    grid.Cells[row, col].Initialize(Cells[row, col]);
                }
            }

            return grid;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(Blocks != null);
            Contract.Invariant(Rows != null);
            Contract.Invariant(Columns != null);

        }

        public void Promote(Grid grid)
        {
            for (int row = 0; row < LENGTH; row++)
            {
                for (int col = 0; col < LENGTH; col++)
                {
                    Cells[row, col].Initialize(grid.Cells[row, col]);
                }
            }
        }
    }
}
