using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Matrices
{
    /// <summary>
    /// A variable size matrix that stores each value as a <see cref="byte"/>. Slower, but useful when memory footprint is a concern.
    /// </summary>
    public sealed class ByteMatrix : MatrixBase<byte>
    {
        /// <summary>
        /// Access the field at the specified coordinates. Conversion is done automatically.
        /// </summary>
        /// <param name="x">The x component of the coordinate.</param>
        /// <param name="y">The y component of the coordinate.</param>
        /// <returns>The value of the field converted to an <see cref="int"/> for convenience.</returns>
        public new int this[int x, int y] {
            get { return _fields[x, y]; }
            set { _fields[x, y] = (byte)value; }
        }

        /// <summary>
        /// Access the field at the specified coordinates. Conversion is done automatically.
        /// </summary>
        /// <param name="x">The x component of the coordinate.</param>
        /// <param name="y">The y component of the coordinate.</param>
        /// <returns>The value of the field converted to an <see cref="int"/> for convenience.</returns>
        public new int this[Point64 p] {
            get { return _fields[p.X, p.Y]; }
            set { _fields[p.X, p.Y] = (byte)value; }
        }

        /// <summary>
        /// Access the field at the specified coordinates. Conversion is done automatically.
        /// </summary>
        /// <param name="x">The x component of the coordinate.</param>
        /// <param name="y">The y component of the coordinate.</param>
        /// <returns>The value of the field converted to an <see cref="int"/> for convenience.</returns>
        public new int this[Point32 p] {
            get { return _fields[p.X, p.Y]; }
            set { _fields[p.X, p.Y] = (byte)value; }
        }

        /// <summary>
        /// Access the field at the specified coordinates. Conversion is done automatically.
        /// </summary>
        /// <param name="x">The x component of the coordinate.</param>
        /// <param name="y">The y component of the coordinate.</param>
        /// <returns>The value of the field converted to an <see cref="int"/> for convenience.</returns>
        public new int this[Point16 p] {
            get { return _fields[p.X, p.Y]; }
            set { _fields[p.X, p.Y] = (byte)value; }
        }

        /// <summary>
        /// Create a new <see cref="ByteMatrix"/> with the specified size.
        /// </summary>
        /// <param name="width">The width of the matrix.</param>
        /// <param name="height">The height of the matrix.</param>
        public ByteMatrix(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new <see cref="ByteMatrix"/> with the given backing array.
        /// </summary>
        /// <param name="fields">The two dimensional array for the matrix.</param>
        public ByteMatrix(byte[,] fields) : base(fields) { }

        /// <summary>
        /// Creates a new <see cref="Matrix"/> with the values of the current one.
        /// </summary>
        /// <returns>A new uncompressed <see cref="Matrix"/>.</returns>
        public Matrix ToMatrix()
        {
            int[,] newArray = new int[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newArray[x, y] = _fields[x, y];
                }
            }

            return new Matrix(newArray);
        }
    }
}
