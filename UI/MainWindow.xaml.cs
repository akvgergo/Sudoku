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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sudoku.Games;
using Sudoku.Matrices;
using Sudoku.Util;

namespace Sudoku.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            ClassicSudoku s = new ClassicSudoku("tjULAm7+9Zk4hs29GJ8o/Q==", regionWidth:3, regionHeight:2);

        }
    }
}
