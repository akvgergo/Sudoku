using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sudoku.Util;

namespace Sudoku.Matrices
{
    /// <summary>
    /// A matrix that stores it's values as powers of 2, with a singe offset. The backbone of most Sudoku logic
    /// that are faster to do as bit operations. Used for larger Sudokus.
    /// </summary>
    public sealed class Bit64Matrix : MatrixBase<long>
    {

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
        public Matrix ToMatrix()
        {
            int[,] newArray = new int[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newArray[x, y] = BinaryMath.GetBitPos(_fields[x, y]);
                }
            }

            return new Matrix(newArray);

        }
    }
}
