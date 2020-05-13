using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sudoku.Matrices;
using Sudoku.Util;
using Sudoku.Games;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace Sudoku.UI
{

    /// <summary>
    /// Purely for UI decoration. Logic stolen from other stuff.
    /// </summary>
    public class DecoSudoku
    {
        Bit32Matrix Matrix = new Bit32Matrix(9, 9);
        Matrix Display = new Matrix(9, 9);
        QuickRand RNG = QuickRand.CreateSecure();
        int AnimationDelay;
        protected int[][] Caches { get; set; }
        int RegionWidth = 3, RegionHeight = 3, MaxValue = 9;

        public DecoSudoku(int animationDelay = 20)
        {
            AnimationDelay = animationDelay;
        }

        public Grid GetGrid()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            for (int x = 0; x < RegionWidth; x++)
            {
                for (int y = 0; y < RegionHeight; y++)
                {

                }
            }

            return grid;
        }

        /// <summary>
        /// Animates the generation and reduction of a 9x9 grid. Once done, it will randomly clear and refill tiles.
        /// </summary>
        async void RunAnimationLoop()
        {
            //Everything here is just copy-paste, including the necessary methods. Only change is some awaits and updating
            //the display grid.
            List<int> values = Enumerable.Range(0, MaxValue).ToList();

            for (int regionOffset = 0; regionOffset < RegionWidth; regionOffset++)
            {
                values.Shuffle(RNG);
                for (int x = 0; x < RegionWidth; x++)
                {
                    for (int y = 0; y < RegionHeight; y++)
                    {
                        Matrix[x + RegionWidth * regionOffset, y + RegionHeight * regionOffset] = 1 << values[x * RegionHeight + y];
                        Display = Matrix.ToMatrix();
                        await Task.Delay(AnimationDelay);
                    }
                }
            }

            List<SolverTile> emptyFields = new List<SolverTile>(Matrix.FieldCount);

            for (int x = 0; x < Matrix.Height; x++)
            {
                for (int y = 0; y < Matrix.Width; y++)
                {
                    if (Matrix[x, y] == 0)
                    {
                        emptyFields.Add(new SolverTile(x, y, 0));
                    }
                }
            }

            int pointer = 0;
            SolverTile field = emptyFields[pointer];
            field.Value = GetTileCache(field.Location.X, field.Location.Y);

            while (true)
            {
                if (field.Value == 0)
                {
                    if (Matrix[field.Location] != 0)
                    {
                        RemoveTileCache(field.Location.X, field.Location.Y, Matrix[field.Location]);
                        Matrix[field.Location] = 0;
                    }
                    field = emptyFields[pointer];
                    continue;
                }

                RemoveTileCache(field.Location.X, field.Location.Y, Matrix[field.Location]);

                int newVal = field.Value & (field.Value - 1);
                Matrix[field.Location] = field.Value ^ newVal;
                field.Value = newVal;
                AddTileCache(field.Location.X, field.Location.Y, Matrix[field.Location]);
                emptyFields[pointer] = field;
                Display = Matrix.ToMatrix();
                await Task.Delay(AnimationDelay);
                if (++pointer == emptyFields.Count) break;
                field = emptyFields[pointer];
                field.Value = GetTileCache(field.Location.X, field.Location.Y);
            }

        }

        struct SolverTile
        {
            public Point16 Location { get; set; }
            public int Value { get; set; }

            public SolverTile(int x, int y, int value)
            {
                Location = new Point16(x, y);
                Value = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetTileCache(int x, int y)
        {
            return (~(Caches[0][x] | Caches[1][y] | Caches[2][x / RegionWidth + y / RegionHeight * RegionHeight])) & (int)(uint.MaxValue >> (32 - MaxValue));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddTileCache(int x, int y, int value)
        {
            Caches[0][x] |= value;
            Caches[1][y] |= value;
            Caches[2][x / RegionWidth + y / RegionHeight * RegionHeight] |= value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RemoveTileCache(int x, int y, int value)
        {
            Caches[0][x] -= value;
            Caches[1][y] -= value;
            Caches[2][x / RegionWidth + y / RegionHeight * RegionHeight] -= value;
        }

        void CreateCaches()
        {
            Caches = new int[3][];
            for (int i = 0; i < 3; i++) Caches[i] = new int[Matrix.Width];

            for (int x = 0; x < Matrix.Width; x++)
            {
                for (int y = 0; y < Matrix.Height; y++)
                {
                    Caches[0][x] |= Matrix[x, y];
                    Caches[1][y] |= Matrix[x, y];
                    Caches[2][x / RegionWidth + y / RegionHeight * RegionHeight] |= Matrix[x, y];
                }
            }
        }

    }
}
