using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sudoku.Matrices;
using Sudoku.Util;
using Sudoku.Games;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media.Animation;
using System.Threading;


namespace Sudoku.UI
{

    /// <summary>
    /// Purely for UI decoration. Logic stolen from other stuff.
    /// </summary>
    public class DecoSudoku
    {
        Bit32Matrix Matrix = new Bit32Matrix(9, 9);
        Matrix Display;
        QuickRand RNG = QuickRand.CreateSecure();
        int AnimationDelay;
        protected int[][] Caches { get; set; }
        int RegionWidth = 3, RegionHeight = 3, MaxValue = 9;
        public Grid grid { get; private set; }
        List<Label> tiles;

        CancellationTokenSource cancelSource = new CancellationTokenSource();

        DoubleAnimation fadein = new DoubleAnimation(0, 1, new Duration(new TimeSpan(0, 0, 2)));
        DoubleAnimation fadeout = new DoubleAnimation(1, 0, new Duration(new TimeSpan(0, 0, 2)));

        public DecoSudoku(int animationDelay = 40)
        {
            AnimationDelay = animationDelay;
            var layout = StaticHelpers.CreateGrid(3, 3);
            grid = layout.grid;
            tiles = layout.contents;
        }

        public void StopAnimation()
        {
            cancelSource.Cancel();
        }

        void Update()
        {
            Display = Matrix.ToMatrix();

            foreach (var lbl in tiles)
            {
                Point32 coord = (Point32)lbl.GetValue(StaticHelpers.CoordProperty);
                lbl.Content = Display[coord] == 0 ? "" : Display[coord].ToString();
            }
        }

        /// <summary>
        /// Animates the generation and reduction of a 9x9 grid. Once done, it will randomly clear and refill tiles.
        /// </summary>
        public async void RunAnimationLoop()
        {
            var canceltoken = cancelSource.Token;
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
                        Update();
                        await Task.Delay(AnimationDelay * 2);
                    }
                }
            }

            if (canceltoken.IsCancellationRequested) return;

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

            if (canceltoken.IsCancellationRequested) return;
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

            if (canceltoken.IsCancellationRequested) return;

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
                    Update();
                    field = emptyFields[--pointer];
                    if (canceltoken.IsCancellationRequested) return;
                    continue;
                }

                RemoveTileCache(field.Location.X, field.Location.Y, Matrix[field.Location]);

                int newVal = field.Value & (field.Value - 1);
                Matrix[field.Location] = field.Value ^ newVal;
                field.Value = newVal;
                AddTileCache(field.Location.X, field.Location.Y, Matrix[field.Location]);
                emptyFields[pointer] = field;
                Update();
                await Task.Delay(AnimationDelay);
                if (canceltoken.IsCancellationRequested) return;
                if (++pointer == emptyFields.Count) break;
                field = emptyFields[pointer];
                field.Value = GetTileCache(field.Location.X, field.Location.Y);
            }

            List<Label> faded = new List<Label>();
            List<Label> visible = new List<Label>(tiles);
            
            while (!canceltoken.IsCancellationRequested)
            {
                await Task.Delay(AnimationDelay * 5);
                if (RNG.GetRange(0, 81) < visible.Count)
                {
                    var lbl = visible.PopRandom(RNG);
                    lbl.BeginAnimation(UIElement.OpacityProperty, fadeout);
                    faded.Add(lbl);
                }
                else
                {
                    var lbl = faded.PopRandom(RNG);
                    lbl.BeginAnimation(UIElement.OpacityProperty, fadein);
                    visible.Add(lbl);
                }
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
