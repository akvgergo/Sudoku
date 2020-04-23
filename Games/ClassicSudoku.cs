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
    class ClassicSudoku : ISudoku
    {
        Matrix innerMatrix;

        public int FieldCount { get; protected set; }

        public int RowCount { get; protected set; }

        public int ColumnCount { get; protected set; }

        public int MaxValue { get; protected set; }

        public bool IsGenerated { get; protected set; }

        public bool IsReduced { get; protected set; }

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
            QuickRand rnd = seed == null ? QuickRand.CreateSecure() : new QuickRand(seed);
            Seed = rnd.GetStateString();

            Console.WriteLine(Seed);

            ColumnCount = RowCount = MaxValue = regionWidth * regionHeight;
            FieldCount = ColumnCount * RowCount;

            if (MaxValue > 32) throw new ArgumentException("The provided values are too large.");

            Bit32Matrix matrix = new Bit32Matrix(MaxValue, MaxValue);

            List<int> values = Enumerable.Range(0, MaxValue).ToList();
            values.Shuffle(rnd);

            for (int x = 0; x < regionWidth; x++)
            {
                for (int y = 0; y < regionHeight; y++)
                {
                    matrix[x, y] = 1 << values[x * regionHeight + y];
                }
            }

            //values.Shuffle(rnd);

            //for (int x = 0; x < regionWidth; x++)
            //{
            //    for (int y = 0; y < regionHeight; y++)
            //    {
            //        matrix[ColumnCount - x - 1, RowCount - y - 1] = 1 << values[x * regionHeight + y];
            //    }
            //}



            matrix.Print();
            if (!GenerateFromPartial(matrix, rnd)) throw new Exception("Logic error, we screwed.");

            innerMatrix = matrix.ToMatrix();
        }

        /// <summary>
        /// Generates a complete puzzle from the provided partially filled matrix.
        /// </summary>
        /// <param name="m">The partially filled matrix to create a proper puzzle from.</param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the algorithm was successful. The operation may fail if the matrix is overfilled
        /// or isn't valid as a puzzle with the already filled values.
        /// </returns>
        public static bool GenerateFromPartial(Bit32Matrix m, QuickRand rnd)
        {
            //all caches should be the same size, the only way this isn't generateable from say, an empty matrix
            //is if we get an exceptionally stupid size argument or a prime MaxVal.
            int[] caches = new int[m.Width * 3];
            int cCOffset = m.Width, cROffset = m.Width * 2;
            //Region sizes
            var regions = GetRegionSize(m.Width);
            int regionWidth = regions.width, regionHeight = regions.height;

            //Fill the caches with values.
            for (int x = 0; x < m.Width; x++)
            {
                for (int y = 0; y < m.Height; y++)
                {
                    caches[x] |= m[x, y];
                    caches[cCOffset + y] |= m[x, y];
                    caches[cROffset + x / regionWidth + y / regionHeight * regionWidth] |= m[x, y];
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
                        for (int y = region / regionWidth * regionHeight; y < region / regionWidth * regionHeight + regionHeight; y++)
                        {
                            if (((caches[cCOffset + y] & (1 << digit)) != 0) || (m[x, y] != 0)) continue;
                            m[x, y] = 1 << digit;
                            caches[x] |= m[x, y];
                            caches[cCOffset + y] |= m[x, y];
                            caches[cROffset + x / regionWidth + y / regionHeight * regionWidth] |= m[x, y];
                            goto regionfilled; // yes, this is satan's offspring, but I've done worse
                        }
                    }
                regionfilled:;
                }
            }

            m.ToMatrix().Print();

            return true;
        }

        public Grid CreateVisual()
        {
            throw new NotImplementedException();
        }

        public int GetConstraintCountForField(int x, int y)
        {
            return 3;
        }

        public Matrix GetMatrix()
        {
            return innerMatrix;
        }

        public bool IsValidTile(int x, int y)
        {
            return x >= 0 && x < ColumnCount && y >= 0 && y < RowCount;
        }

        /// <summary>
        /// Returns the width and height of a Sudoku, calculated from the maximum field values.
        /// </summary>
        /// <param name="MaxValue">The highest value a field can have.</param>
        /// <returns>A tuple containing the width and height of a single region.</returns>
        /// <remarks>
        /// Side effect of making the generator self-contained. Note that this returns the value that
        /// "makes the most sense". The method asks for a Maxval, but it could also ask for width and height, or
        /// a single dimension if we always assume a perfect square, but with more flexibility comes
        /// a few logic issues, and some decisions to make.
        /// 
        /// If we asked for dimensions instead, we could make the argument that an 8x8 Sudoku with 4 4x4 regions is perfectly
        /// legal, since of course we could choose from 16 values to fit into that 8 for rows and columns, and there would be no
        /// need to use the same value twice. There are a few configurations where this issue could arise and not just with squares,
        /// so better to lay down the rule that we *need* to use all numbers.
        /// 
        /// So MaxVal is better in that regard. But is it better to make a say 12x12 grid into 12 3x4, or 12 2x6.
        /// The former of course makes more sense, but the better question is, what if we *want* a 2x6 RegionSize?
        /// It could be perfect for a multi-Sudoku later, or some other interesting game variant.
        /// 
        /// When we make things more abstract, easier to use, and above all flexible, we may run into similar issues.
        /// Multiple solutions that are better for some cases than others. The idea of making GeneratorArgs a thing was born
        /// while writing this method.
        /// 
        /// So yes, there is sometimes no cover-all answer, or we just don't want it, while a default behaviour makes sense.
        /// This method should return the answer that is the most logical, in our case the two least further apart numbers that when
        /// multiplied together give MaxVal. I might never use it, but it helped think to write it, so here it is.
        /// 
        /// For the love of all that is holy, prime check the input.
        /// </remarks>
        public static (int width, int height) GetRegionSize(int MaxValue)
        {
            int sqrt = (int)Math.Floor(Math.Sqrt(MaxValue));
            //a whole sqrt means we can abide traditional rules, best case
            if (sqrt * sqrt == MaxValue) return (sqrt, sqrt);

            //the least deviation from the sqrt will return the least further apart values.
            int width = sqrt + 1;
            while (width * sqrt != MaxValue)
            {
                if (width * sqrt > MaxValue)
                    sqrt--;
                else
                    width++;
            }
            return (width, sqrt);
        }
    }
}
