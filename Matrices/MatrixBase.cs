using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Matrices
{
    /// <summary>
    /// Base for most matrices. Sole purpose is to eliminate code repetition. Inheritors should be marked sealed for performance.
    /// </summary>
    public abstract class MatrixBase<ValueT> where ValueT : struct
    {
        /// <summary>
        /// The values stored in the matrix.
        /// </summary>
        protected ValueT[,] _fields;

        /// <summary>
        /// The width of the matrix.
        /// </summary>
        public int Width { get; protected set; }
        /// <summary>
        /// The height of the matrix.
        /// </summary>
        public int Height { get; protected set; }
        /// <summary>
        /// The total field count of the matrix.
        /// </summary>
        public int FieldCount { get; protected set; }

        /// <summary>
        /// Access the field at the specified coordinates.
        /// </summary>
        /// <param name="x">The x component of the coordinate.</param>
        /// <param name="y">The y component of the coordinate.</param>
        /// <returns>The value of the field.</returns>
        public virtual ValueT this[int x, int y] {
            get { return _fields[x, y]; }
            set { _fields[x, y] = value; }
        }

        /// <summary>
        /// Access the field at the specified coordinate.
        /// </summary>
        /// <param name="p">The specified coordinate.</param>
        /// <returns>The value of the field.</returns>
        public virtual ValueT this[Point64 p] {
            get { return _fields[p.X, p.Y]; }
            set { _fields[p.X, p.Y] = value; }
        }

        /// <summary>
        /// Access the field at the specified coordinate.
        /// </summary>
        /// <param name="p">The specified coordinate.</param>
        /// <returns>The value of the field.</returns>
        public virtual ValueT this[Point32 p] {
            get { return _fields[p.X, p.Y]; }
            set { _fields[p.X, p.Y] = value; }
        }

        /// <summary>
        /// Access the field at the specified coordinate.
        /// </summary>
        /// <param name="p">The specified coordinate.</param>
        /// <returns>The value of the field.</returns>
        public virtual ValueT this[Point16 p] {
            get { return _fields[p.X, p.Y]; }
            set { _fields[p.X, p.Y] = value; }
        }

        /// <summary>
        /// Creates a new matrix with the specified size.
        /// </summary>
        public MatrixBase(int width, int height)
        {
            Width = width;
            Height = height;
            FieldCount = width * height;

            _fields = new ValueT[width, height];
        }

        /// <summary>
        /// Creates a new matrix from the given backing array. The array is not copied,
        /// if that is your use a method instead.
        /// </summary>
        public MatrixBase(ValueT[,] fields)
        {
            Width = fields.GetLength(0);
            Height = fields.GetLength(1);
            FieldCount = Width * Height;

            _fields = fields;
        }

        /// <summary>
        /// Returns a reference to the backing array for direct operations.
        /// </summary>
        /// <returns>The 2 dimensional array that stores the matrix's values.</returns>
        public ValueT[,] GetBackingArray()
        {
            return _fields;
        }

        /// <summary>
        /// [Debug] Prints the stored values to the output stream
        /// </summary>
        public void Print()
        {
            //Yes, this is mirrored diagonally from top left
            //will worry about it if it becomes a problem, last time I checked this changes nothing
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Console.Write(_fields[y, x].ToString() + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
