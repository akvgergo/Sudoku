using Sudoku.Matrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Sudoku.Util;

namespace Sudoku.Games
{
    /// <summary>
    /// A sudoku puzzle with the classic row, column, and square region constraints. Can be a custom unconventional size.
    /// </summary>
    class ClassicSudoku : SudokuBase32
    {
        Matrix innerMatrix;

        public bool IsUnique {
            get { return true; }
        }

        public int MinFieldConstraints {
            get { return 3; }
        }

        public int MaxFieldConstraints {
            get { return 3; }
        }

        public string Seed { get; protected set; }

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

            //for (int regionOffset = 0; regionOffset < Math.Min(regionWidth, regionHeight); regionOffset++)
            //{
            //    values.Shuffle(RNG);
            //    for (int x = 0; x < regionWidth; x++)
            //    {
            //        for (int y = 0; y < regionHeight; y++)
            //        {
            //            Matrix[x + regionWidth * regionOffset, y + regionHeight * regionOffset] = 1 << values[x * regionHeight + y];
            //        }
            //    }
            //}

            Matrix.ToMatrix().Print();

            Console.WriteLine(BruteFill());

            Matrix.ToMatrix().Print();

            innerMatrix = Matrix.ToMatrix();
        }
        
        /// <summary>
        /// Generates a complete puzzle from the provided partially filled matrix.
        /// 
        /// This is baiscally a deterministic solver.
        /// </summary>
        /// <param name="m">
        /// The partially filled matrix to create a proper puzzle from. The most ideal is to have a
        /// certain number of regions completely filled.
        /// </param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the algorithm was successful. The operation may fail if the matrix is overfilled
        /// or isn't valid as a puzzle with the already filled values.
        /// </returns>
        public static bool GenerateFromPartial(Bit32Matrix m, int regionWidth, int regionHeight)
        {
            //all caches should be the same size, the only way this isn't generateable from say, an empty matrix
            //is if we get an exceptionally stupid size argument or a prime MaxVal.
            int[] caches = new int[m.Width * 3];
            int cCOffset = m.Width, cROffset = m.Width * 2;
            
            //Fill the caches with values.
            for (int x = 0; x < m.Width; x++)
            {
                for (int y = 0; y < m.Height; y++)
                {
                    caches[x] |= m[x, y];
                    caches[cCOffset + y] |= m[x, y];
                    caches[cROffset + x / regionWidth + y / regionHeight * regionHeight] |= m[x, y];
                }
            }

            //reminder: digit approach, region approach- constrained or unconstrained, refill approach
            //digit, filling per region for now
            for (int digit = 0; digit < m.Width; digit++)
            {
                for (int region = 0; region < m.Width; region++)
                {
                    if ((caches[cROffset + region] & (1 << digit)) != 0) continue;
                    for (int x = region % regionHeight * regionWidth; x < region % regionHeight * regionWidth + regionWidth; x++)
                    {
                        if ((caches[x] & (1 << digit)) != 0) continue;
                        for (int y = region / regionHeight * regionHeight; y < region / regionHeight * regionHeight + regionHeight; y++)
                        {
                            if (((caches[cCOffset + y] & (1 << digit)) != 0) || (m[x, y] != 0)) continue;
                            int fieldCache = (~(caches[x] | caches[cCOffset + y] | caches[cROffset + x / regionWidth + y / regionHeight * regionHeight]));
                            if (BinaryMath.Q_PopCnt(fieldCache >> (32 >> (regionWidth * regionHeight))) == 0) continue; //TODO: was here, not working
                            m[x, y] = 1 << digit;
                            //m.ToMatrix().Print();
                            caches[x] |= m[x, y];
                            caches[cCOffset + y] |= m[x, y];
                            caches[cROffset + x / regionWidth + y / regionHeight * regionHeight] |= m[x, y];
                            goto regionfilled; // yes, this is satan's offspring, but I've done worse
                        }
                    }

                regionfilled:;
                }
            }

            m.ToMatrix().Print();

            return true;
        }

        public override int GetConstraintCountForField(int x, int y)
        {
            return 3;
        }

        protected sealed override int GetTileCache(int x, int y)
        {
            return (~(Caches[0][x] | Caches[1][y] | Caches[2][x / RegionWidth + y / RegionHeight * RegionHeight])) & (int)(uint.MaxValue >> (32 - MaxValue));
        }

        protected sealed override void AddTileCache(int x, int y, int value)
        {
            Caches[0][x] |= value;
            Caches[1][y] |= value;
            Caches[2][x / RegionWidth + y / RegionHeight * RegionHeight] |= value;
        }

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
            throw new NotImplementedException();
        }

        public override Grid CreateVisual()
        {
            throw new NotImplementedException();
        }
    }
}
