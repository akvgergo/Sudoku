﻿using System;
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

namespace Sudoku.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DoubleAnimation fadeOut = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 1)));
        DecoSudoku deco = new DecoSudoku();

        public MainWindow()
        {
            InitializeComponent();

            BackgroudView.Child = deco.grid;
            deco.grid.Width = 1000;
            deco.grid.Height = 1000;
            deco.RunAnimationLoop();
        }

        private void TriviaButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://en.wikipedia.org/wiki/Sudoku");
        }
    }
}
