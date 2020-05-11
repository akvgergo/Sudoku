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
    public sealed class ClassicSudoku : SudokuBase32
    {
        Matrix SolutionMatrix, GameMatrix;
        
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
        /// <remarks>
        /// The strategy is that it fills up the center first and then goes outwards to fill everything else,
        /// with random steps added in-between. The exact way differs if:
        /// -the center of the Sudoku is the center of a region
        /// -the center of the Sudoku is the wall between two regions
        /// -the center of the Sudoku is the corner between 4 regions
        /// 
        /// The logic behind this is that in order to avoid overfilling (randomly filling to a point
        /// where bruteforce would fail) we cannot fill more than half the regions that are affecting
        /// (in the same row of regions or column of regions) a given region. Filling outwards should never
        /// pose such a situation, and should give the most performance for rather large grids.
        /// </remarks>
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

            //special case, the Sudoku is just too small to fill any other way.
            if (MaxValue <= 4)
            {
                for (int x = 0; x < regionWidth; x++)
                {
                    for (int y = 0; y < RegionHeight; y++)
                    {
                        values.Shuffle(RNG);
                        Matrix[x, y] = 1 << values[x * regionHeight + y];
                        BruteFill(false);
                    }
                }
                SolutionMatrix = Matrix.ToMatrix();
                return;
            }

            //regardless of shape, we can always start by filling diagonally. Since the filled regions don't affect each other
            //we can be completely random
            for (int regionOffset = 0; regionOffset < Math.Min(regionWidth, regionHeight); regionOffset++)
            {
                values.Shuffle(RNG);
                for (int x = 0; x < regionWidth; x++)
                {
                    for (int y = 0; y < regionHeight; y++)
                    {
                        Matrix[x + regionWidth * regionOffset, y + RegionHeight * regionOffset] = 1 << values[x * regionHeight + y];
                    }
                }
            }

            //Matrix.ToMatrix().Print();

            int centerX = Matrix.Width / 2, centerY = Matrix.Height / 2;
            if (Matrix.Width == Matrix.Height) //if we have a regular Sudoku shape, we can start filling from the center
            {
                int start, finish;
                if (Matrix.Width % 2 == 0) //the center is a region corner
                {
                    start = Matrix.Width / 2 - regionWidth;
                    finish = Matrix.Width / 2 + regionWidth;
                }
                else //the center is a region
                {
                    start = Matrix.Width / 2 / regionWidth * regionWidth - regionWidth;
                    finish = Matrix.Width / 2 / regionWidth * regionWidth + regionWidth * 2;
                }
                Console.WriteLine(BruteFill(true, start, start, finish, finish)); // we do one random bruteFill for some extra entropy
                start -= regionWidth;
                finish += regionWidth;
                while (start >= 0)
                {
                    BruteFill(true, start, start, finish, finish);
                    start -= regionWidth;
                    finish += regionWidth;
                }
            }
            else //irregular is a bit problematic, we can randomfill with the solver, then figure the rest out
            {
                for (int regionOffset = 0; regionOffset < Math.Max(regionWidth, RegionHeight) / 2; regionOffset++)
                {

                }
            }

            SolutionMatrix = Matrix.ToMatrix();
            SolutionMatrix.Print();
            Reduce();
            GameMatrix = Matrix.ToMatrix();
            GameMatrix.Print();
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
            return SolutionMatrix;
        }

        public override Grid CreateVisual()
        {
            throw new NotImplementedException();
        }
    }
}
