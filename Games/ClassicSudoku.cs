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

            List<int> values = Enumerable.Range(0, 9).ToList();
            values.Shuffle(rnd);

            for (int x = 0; x < regionWidth; x++)
            {
                for (int y = 0; y < regionHeight; y++)
                {
                    matrix[x, y] = 1 << values[x * 3 + y];
                }
            }

            values.Shuffle(rnd);

            for (int x = 0; x < regionWidth; x++)
            {
                for (int y = 0; y < regionHeight; y++)
                {
                    matrix[ColumnCount - x - 1, RowCount - y - 1] = 1 << values[x * 3 + y];
                }
            }

            if (!GenerateFromPartial(matrix)) throw new Exception("Logic error, we screwed.");
        }

        /// <summary>
        /// Generates a complete puzzle from the provided partially filled matrix.
        /// </summary>
        /// <param name="m">The partially filled matrix to create a proper puzzle from.</param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the algorithm was successful. The operation may fail if the matrix is overfilled
        /// or isn't valid as a puzzle with the already filled values.
        /// </returns>
        public static bool GenerateFromPartial(Bit32Matrix m)
        {
            //all caches should be the same size, the only way this isn't generateable from say, an empty matrix
            //is if we get an exceptionally stupid size argument or a prime MaxVal.
            int[] caches = new int[m.Width * 3];
            int cCOffset = m.Width, cRoffset = m.Width * 2;

            for (int x = 0; x < m.Width; x++)
            {
                for (int y = 0; y < m.Height; y++)
                {
                    caches[x] |= m[x, y];
                    caches[cCOffset + y] |= m[x, y];
                    caches[cRoffset + x / 3]  = m[x, y];
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
            throw new NotImplementedException();
        }

        public bool IsValidTile(int x, int y)
        {
            return x >= 0 && x < ColumnCount && y >= 0 && y < RowCount;
        }
    }
}
