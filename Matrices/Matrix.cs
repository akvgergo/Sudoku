using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Matrices
{
    /// <summary>
    /// A simple variable size <see cref="int"/> matrix that can be a backend for any simple logic, mostly display.
    /// </summary>
    sealed class Matrix : MatrixBase<int>
    {
        /// <summary>
        /// Create a new <see cref="Matrix"/> with the specified size.
        /// </summary>
        /// <param name="width">The width of the matrix.</param>
        /// <param name="height">The height of the matrix.</param>
        public Matrix(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new <see cref="Matrix"/> with the given backing array.
        /// </summary>
        /// <param name="fields">The two dimensional array for the matrix.</param>
        public Matrix(int[,] fields) : base(fields) { }

        /// <summary>
        /// Compresses the current matrix into a <see cref="ByteMatrix"/>.
        /// </summary>
        /// <returns>A new <see cref="ByteMatrix"/> filled with the values of the currrent <see cref="Matrix"/>.</returns>
        public ByteMatrix ToByteMatrix()
        {
            byte[,] newArray = new byte[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newArray[x, y] = (byte)_fields[x, y];
                }
            }

            return new ByteMatrix(newArray);
        }

        /// <summary>
        /// Creates a <see cref="Bit32Matrix"/> from the current <see cref="Matrix"/>.
        /// </summary>
        /// <returns>A new <see cref="Bit32Matrix"/> filled with the values of the currrent <see cref="Matrix"/>.</returns>
        public Bit32Matrix ToBit32Matrix()
        {
            int[,] newArray = new int[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newArray[x, y] = _fields[x, y] == 0 ? 0 : 1 << (_fields[x, y] - 1);
                }
            }

            return new Bit32Matrix(newArray);
        }

        /// <summary>
        /// Creates a <see cref="Bit64Matrix"/> from the current <see cref="Matrix"/>.
        /// </summary>
        /// <returns>A new <see cref="Bit64Matrix"/> filled with the values of the currrent <see cref="Matrix"/>.</returns>
        public Bit64Matrix ToBit64Matrix()
        {
            long[,] newArray = new long[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newArray[x, y] = _fields[x, y] == 0 ? 0 : 1L << (_fields[x, y] - 1);
                }
            }

            return new Bit64Matrix(newArray);
        }
    }
}
