using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Zabavnov.Sudoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly TextBlock [,] _blocks = new TextBlock[Grid.LENGTH, Grid.LENGTH];

        Model Model => (Model)Resources["Model"];

        private Grid _dataGrid;

        public MainWindow()
        {
            InitializeComponent();
            var style = _grid.Resources["Cell"] as Style;

            for (int r = 0; r < Grid.LENGTH; r++)
            {
                for (int c = 0; c < Grid.LENGTH; c++)
                {
                    var block = new TextBlock { Style = style };
                    _blocks[r, c] = block;

                    var border = new Border
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = Brushes.Black,
                        Child = block
                    };
                    border.SetValue(System.Windows.Controls.Grid.RowProperty, r + 1);
                    border.SetValue(System.Windows.Controls.Grid.ColumnProperty, c + 1);
                    _grid.Children.Add(border);
                }
            }

            Model.AttachActionTo(nameof(Model.Grid), BindCells);
            _dataGrid = Model.Grid;
            BindCells();
        }

        private void BindCells()
        {
            if (_dataGrid != null)
            {
                _dataGrid.CellChanged -= CellChanged;
            }

            if (Model.Grid != null)
            {
                _dataGrid = Model.Grid;
                _dataGrid.CellChanged += CellChanged;

                for (int r = 0; r < Grid.LENGTH; r++)
                {
                    for (int c = 0; c < Grid.LENGTH; c++)
                    {
                        CellChanged(_dataGrid[r, c]);
                    }
                }
            }
        }

        private void CellChanged(Cell cell)
        {
            var textBlock = _blocks[cell.Row, cell.Column];
            Contract.Assume(textBlock != null);

            if (cell.IsCompleted)
            {
                textBlock.Text = cell.Value.Value.ToString();

                textBlock.Foreground = cell.IsSolved ? Brushes.DarkGreen : Brushes.DarkRed;
            }
            else
            {
                textBlock.Text = string.Empty;
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_blocks != null);
            Contract.Invariant(Model != null);
        }
    }
}
