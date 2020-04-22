using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Matrices
{
    /// <summary>
    /// A matrix that stores it's values as powers of 2, with a singe offset. The backbone of most Sudoku logic
    /// that are faster to do as bit operations. Used for larger Sudokus.
    /// </summary>
    sealed class Bit64Matrix : MatrixBase<long>
    {
        /// <summary>
        /// Used by the <see cref="ToMatrix()"/> method. Lazy initialized and uses bytes for less memory overhead.
        /// </summary>
        static byte[] ConversionMap;

        /// <summary>
        /// Create a new <see cref="Bit64Matrix"/> with the specified size.
        /// </summary>
        /// <param name="width">The width of the matrix.</param>
        /// <param name="height">The height of the matrix.</param>
        public Bit64Matrix(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new <see cref="Bit64Matrix"/> with the given backing array.
        /// </summary>
        /// <param name="fields">The two dimensional array for the matrix.</param>
        public Bit64Matrix(long[,] fields) : base(fields) { }

        /// <summary>
        /// Converts the current <see cref="Bit64Matrix"/> to a standard <see cref="Matrix"/>.
        /// </summary>
        /// <returns>A <see cref="Matrix"/> with the values of the current instance.</returns>
        /// <remarks>See <see cref="Bit32Matrix.ToMatrix()"/> for implementation details.</remarks>
        public Matrix ToMatrix()
        {
            //Lazy init
            if (ConversionMap == null)
            {
                ConversionMap = new byte[67];

                for (int i = 0; i < 64; i++)
                {
                    ConversionMap[(1UL << i) % 67] = (byte)(i + 1);
                }
            }

            int[,] newArray = new int[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newArray[x, y] = ConversionMap[(ulong)_fields[x, y] % 67];
                }
            }

            return new Matrix(newArray);

        }
    }
}
