using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Matrices
{
    /// <summary>
    /// A matrix that stores it's values as powers of 2, with a singe offset. The backbone of most Sudoku logic
    /// that are faster to do as bit operations.
    /// </summary>
    sealed class Bit32Matrix : MatrixBase<int>
    {
        /// <summary>
        /// Used by the <see cref="ToMatrix()"/> method. Lazy initialized and uses bytes for less memory overhead.
        /// </summary>
        static byte[] ConversionMap;

        /// <summary>
        /// Create a new <see cref="Bit32Matrix"/> with the specified size.
        /// </summary>
        /// <param name="width">The width of the matrix.</param>
        /// <param name="height">The height of the matrix.</param>
        public Bit32Matrix(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new <see cref="Bit32Matrix"/> with the given backing array.
        /// </summary>
        /// <param name="fields">The two dimensional array for the matrix.</param>
        public Bit32Matrix(int[,] fields) : base(fields) { }

        /// <summary>
        /// Converts the current <see cref="Bit32Matrix"/> to a standard <see cref="Matrix"/>.
        /// </summary>
        /// <returns>A <see cref="Matrix"/> with the values of the current instance.</returns>
        /// <remarks>
        /// The obvious solution here would be logarithm, since mathematically,
        /// we would like to know the current power of 2 a value represents.
        /// 
        /// That's dog slow.
        /// 
        /// We're making the assumption that all values have a Hamming weight of 1 or 0, since only then is it possible to
        /// do the conversion backwards. In that case, a binary search algorithm on the bits would be ideal, but let's assume
        /// we have the memory space for mapping instead. If we do have the cache space that'll be a lot faster, since we might have
        /// matrixes with hundreds of fields.
        /// 
        /// Of course, we can't just make a map with 32 bit integers, that would be huge. So we need a way to get a small and unique
        /// value for all the 33 possible cases.
        /// 
        /// Basically, a pseudo-hashmap.
        /// Doing modulo with a prime that's larger than 33 will give small and unique values for each case. 37 will do.
        /// 
        /// We are not using a conventional Dictionary to avoid overhead, since that's a lot more complex 
        /// and the function calls and memory space for "buckets" together would make the binary search preferable.
        /// 
        /// Yes, it would be easier. Yes, I could've typed out a switch with 33 cases by the time I figured and typed this out.
        /// But where's the fun in that?
        /// </remarks>
        public Matrix ToMatrix()
        {
            //Lazy init
            if (ConversionMap == null)
            {
                ConversionMap = new byte[37];

                for (int i = 0; i < 32; i++)
                {
                    ConversionMap[(1u << i) % 37] = (byte)(i + 1);
                }
            }

            int[,] newArray = new int[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newArray[x, y] = ConversionMap[(uint)_fields[x, y] % 37];
                }
            }

            return new Matrix(newArray);
        }
    }
}
