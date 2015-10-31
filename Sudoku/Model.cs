using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using Zabavnov.Sudoku.Properties;

namespace Zabavnov.Sudoku
{
    public class Model: INotifyPropertyChanged
    {
        private Grid _grid;
        private string _status = "Ready";

        public Model()
        {
            SaveCommand = new Command(OnSaveCommand, CanExecuteSaveCommand);
            SolveCommand = new Command(OnSolveCommand, CanExecuteSolveCommand);
            OpenCommand = new Command(OnOpenCommand);
            NewCommand = new Command(OnGenerateCommand, () => false);
        }

        private bool CanExecuteSaveCommand()
        {
            return _grid != null && _grid.Cells.Any(z => z.IsCompleted);
        }

        private bool CanExecuteSolveCommand()
        {
            return _grid != null && _grid.Cells.Any(z => z.IsEmpty);
        }

        private void OnGenerateCommand()
        {
            
        }

        private void OnOpenCommand()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Sudoku files (*.sdk)|*.sdk|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FilterIndex = Settings.Default.OpenFileIndex
            };

            if (dlg.ShowDialog() == true)
            {
                Settings.Default.OpenFileIndex = dlg.FilterIndex;
                Settings.Default.Save();

                Grid = Grid.LoadFromFile(dlg.FileName);

                SolveCommand.CanExecute(null);
            }
        }

        private void OnSaveCommand()
        {
            Contract.Requires(Grid != null);

            var dlg = new SaveFileDialog
            {
                Filter = "Sudoku files (*.sdk)|*.sdk|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = "*.sdk",
                FilterIndex = Settings.Default.OpenFileIndex
            };
            if (dlg.ShowDialog() == true)
            {
                Settings.Default.OpenFileIndex = dlg.FilterIndex;
                Settings.Default.Save();

                Grid.SaveToFile(dlg.FileName);
            }
        }

        private void OnSolveCommand()
        {
            if (!_grid.IsCompleted)
            {
                ComplexityLevel level;
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                int tryCount;
                var rc = Processor.Solve(_grid, grid => Grid = grid, out level, out tryCount);
                stopWatch.Stop();

                if (rc)
                {
                    if (stopWatch.Elapsed.Seconds == 0)
                    {
                        Status = $"Solved with {level} level in {stopWatch.Elapsed.Milliseconds} milliseconds";
                    }
                    else
                    {
                        Status = $"Solved with {level} level in {stopWatch.Elapsed.Seconds} seconds";
                    }

                    if (tryCount > 0)
                    {
                        Status += $" in {tryCount} tries";
                    }
                }
                else
                {
                    Status = "Unable to solve";
                }
            }
        }

        public Grid Grid
        {
            get { return _grid; }
            set
            {
                Set(ref _grid, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(property, value))
            {
                property = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }


        public ICommand SaveCommand { get; }

        public ICommand SolveCommand { get; }

        public ICommand OpenCommand { get; }

        public ICommand NewCommand { get; }
    }
}
