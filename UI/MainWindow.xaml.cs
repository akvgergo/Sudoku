using System;
using System.Data;
using System.Data.Entity;
using System.Data.Sql;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sudoku.Games;
using Sudoku.Matrices;
using Sudoku.Util;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.IO;

namespace Sudoku.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string dataDir;

        ClassicSudoku sudoku;

        DoubleAnimation fadeOut = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 1)));
        DecoSudoku deco = new DecoSudoku();
        DataModelContainer dmc = new DataModelContainer();

        public MainWindow()
        {
            InitializeComponent();

            dataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Sudoku";
            Directory.CreateDirectory(dataDir);
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
            Closing += (o, e) => {
                deco.StopAnimation();
                if (Content != TitleScreenGrid)
                {
                    Console.WriteLine("saving!");
                    Save s = new Save();
                    s.Width = 9;
                    s.Height = 9;
                    s.GameType = sudoku.GetType().ToString();
                    s.MatrixData = sudoku.GameMatrix.ToByteMatrix().GetRaw();
                    dmc.Saves.Add(s);
                    dmc.SaveChanges();
                }
            };

            BackgroudView.Child = deco.grid;
            deco.grid.Width = 1000;
            deco.grid.Height = 1000;
            deco.RunAnimationLoop();

            dmc.Database.CreateIfNotExists();

            if (dmc.Saves.Count() == 0)
            {
                ContinueButton.IsEnabled = false;
            }
           
        }

        private void TriviaButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://en.wikipedia.org/wiki/Sudoku");
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            deco.StopAnimation();
            TitleScreenGrid.BeginAnimation(OpacityProperty, fadeOut);
            Grid gameGrid = new Grid();
            sudoku = new ClassicSudoku();

            var sudoGrid = sudoku.CreateVisual();
            sudoGrid.Width = 1000;
            sudoGrid.Height = 1000;
            gameGrid.Children.Add(new Viewbox() { Child = sudoGrid, Stretch = System.Windows.Media.Stretch.Uniform });
            await Task.Delay(1000);


            Content = gameGrid;
        }

        private async void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var save = dmc.Saves.First();

            deco.StopAnimation();
            TitleScreenGrid.BeginAnimation(OpacityProperty, fadeOut);
            Grid gameGrid = new Grid();
            sudoku = new ClassicSudoku(ByteMatrix.FromRaw(save.MatrixData));

            var sudoGrid = sudoku.CreateVisual();
            sudoGrid.Width = 1000;
            sudoGrid.Height = 1000;
            gameGrid.Children.Add(new Viewbox() { Child = sudoGrid, Stretch = System.Windows.Media.Stretch.Uniform });
            await Task.Delay(1000);

            Content = gameGrid;
        }
    }
}
