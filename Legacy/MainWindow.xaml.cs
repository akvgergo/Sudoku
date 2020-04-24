using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SudokuBeta
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Sudoku s;
        Border[] Regions;
        Border[,] Cells;


        public MainWindow()
        {
            InitializeComponent();

            s = new Sudoku(Difficulty.Easy, Environment.TickCount);



            Regions = new Border[9];
            for (int i = 0; i < 9; i++)
            {
                Border b = new Border() {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Black
                };
                Regions[i] = b;

                SudokuGrid.Children.Add(b);
                Grid.SetRow(b, i / 3);
                Grid.SetColumn(b, i % 3);
            }


            foreach (var region in Regions)
            {
                region.Child = GetGrid();
            }

            Cells = new Border[9, 9];
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Border b = new Border()
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = Brushes.Black
                    };

                    Cells[x, y] = b;
                    ((Grid)Regions[y / 3 * 3+ x / 3].Child).Children.Add(b);
                    
                    Grid.SetRow(b, y % 3);
                    Grid.SetColumn(b, x % 3);
                }
            }

            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Label l = new Label() { Content = s.Solution[x, y] == 0 ? "" : s.Solution[x, y].ToString(), Width = double.NaN, Height = double.NaN, Padding = new Thickness(0) };
                    Cells[x, y].Child = new Viewbox() { Child = l, Stretch = Stretch.Uniform };
                }
            }
        }


        private Grid GetGrid()
        {
            Grid grid = new Grid();
            for (int i = 0; i < 3; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
            }

            return grid;
        }
    }
}
