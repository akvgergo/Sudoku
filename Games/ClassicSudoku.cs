using Sudoku.Matrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Sudoku.Util;

namespace Sudoku.Games
{
    /// <summary>
    /// A sudoku puzzle with the classic row, column, and square region constraints. Can be a custom unconventional size.
    /// </summary>
    sealed class ClassicSudoku : SudokuBase32
    {
        Matrix innerMatrix;
        
        public override bool IsUnique {
            get { return true; }
        }

        public override int MinFieldConstraints {
            get { return 3; }
        }

        public override int MaxFieldConstraints {
            get { return 3; }
        }

        /// <summary>
        /// Generate a new Sudoku with the classic ruleset.
        /// </summary>
        /// <param name="seed">The seed to generate the puzzle with.</param>
        /// <param name="regionWidth">The width of a single region. The final size will be Width * Height.</param>
        /// <param name="regionHeight">The height of a single region. The final size will be Width * Height.</param>
        public ClassicSudoku(string seed = null, int regionWidth = 3, int regionHeight = 3)
        {
            RNG = seed == null ? QuickRand.CreateSecure() : new QuickRand(seed);

            Seed = RNG.GetStateString();

            Console.WriteLine(Seed);

            RegionWidth = regionWidth;
            RegionHeight = regionHeight;
            ColumnCount = RowCount = MaxValue = regionWidth * regionHeight;
            FieldCount = ColumnCount * RowCount;

            if (MaxValue > 32) throw new ArgumentException("The provided values are too large.");

            Matrix = new Bit32Matrix(MaxValue, MaxValue);

            List<int> values = Enumerable.Range(0, MaxValue).ToList();

            for (int regionOffset = 0; regionOffset < Math.Min(regionWidth, regionHeight); regionOffset++)
            {
                values.Shuffle(RNG);
                for (int x = 0; x < regionWidth; x++)
                {
                    for (int y = 0; y < regionHeight; y++)
                    {
                        Matrix[x + regionWidth * regionOffset, y + regionHeight * regionOffset] = 1 << values[x * regionHeight + y];
                    }
                }
            }

            BruteFill(true);
            innerMatrix = Matrix.ToMatrix();
        }

        public override int GetConstraintCountForField(int x, int y)
        {
            return 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override int GetTileCache(int x, int y)
        {
            return (~(Caches[0][x] | Caches[1][y] | Caches[2][x / RegionWidth + y / RegionHeight * RegionHeight])) & (int)(uint.MaxValue >> (32 - MaxValue));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override void AddTileCache(int x, int y, int value)
        {
            Caches[0][x] |= value;
            Caches[1][y] |= value;
            Caches[2][x / RegionWidth + y / RegionHeight * RegionHeight] |= value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override void RemoveTileCache(int x, int y, int value)
        {
            Caches[0][x] -= value;
            Caches[1][y] -= value;
            Caches[2][x / RegionWidth + y / RegionHeight * RegionHeight] -= value;
        }

        protected sealed override void CreateCaches()
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

        public override Matrix GetMatrix()
        {
            return innerMatrix;
        }

        public override Grid CreateVisual()
        {
            throw new NotImplementedException();
        }
    }
}
