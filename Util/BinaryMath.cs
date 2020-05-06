using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Util
{
    /// <summary>
    /// Contains static helper methods to manipulate bit data on atomic types.
    /// </summary>
    static class BinaryMath
    {

        //Conversion cache for GetBitPos
        static byte[] ConversionMap32;
        static byte[] ConversionMap64;

        /// <summary>
        /// Yes, we dislike static constructors. But realistically, this is the best solution
        /// right after having the values be hardcoded.
        /// </summary>
        static BinaryMath()
        {
            ConversionMap32 = new byte[37];

            for (int i = 0; i < 32; i++)
            {
                ConversionMap32[(1u << i) % 37] = (byte)(i + 1);
            }

            ConversionMap64 = new byte[67];

            for (int i = 0; i < 64; i++)
            {
                ConversionMap64[(1ul << i) % 67] = (byte)(i + 1);
            }

        }

        /// <summary>
        /// Returns the Hamming Weight of the given <see cref="ulong"/>.
        /// Uses Wegner's tactic, faster if very few bits are expected to be set.
        /// </summary>
        /// <param name="N">The value to count the bits from.</param>
        /// <returns>The population count.</returns>
        public static int PopCnt(ulong N)
        {
            int value = 0;
            while (N != 0)
            {
                N = (N - 1) & N;
                value++;
            }

            return value;
        }

        /// <summary>
        /// Returns the Hamming Weight of the given <see cref="long"/>.
        /// Uses Wegner's tactic, faster if very few bits are expected to be set.
        /// </summary>
        /// <param name="N">The value to count the bits from.</param>
        /// <returns>The population count.</returns>
        public static unsafe int PopCnt(long N)
        {
            return PopCnt(*(ulong*)&N);
        }

        /// <summary>
        /// Returns the Hamming Weight of the given <see cref="int"/>.
        /// Uses Wegner's tactic, faster if very few bits are expected to be set.
        /// </summary>
        /// <param name="N">The value to count the bits from.</param>
        /// <returns>The population count.</returns>
        public static unsafe int PopCnt(int N)
        {
            return PopCnt(*(uint*)&N);
        }

        /// <summary>
        /// Returns the Hamming Weight of the given <see cref="uint"/>.
        /// Uses Wegner's tactic, faster if very few bits are expected to be set.
        /// </summary>
        /// <param name="N">The value to count the bits from.</param>
        /// <returns>The population count.</returns>
        public static int PopCnt(uint N)
        {
            return PopCnt((ulong)N);
        }

        /// <summary>
        /// Returns the Hamming Weight of the given <see cref="ulong"/>.
        /// Uses the fastest possible implementation that's not vectorized or a built in CPU instruction,
        /// should be used if the value is expected to be high.
        /// </summary>
        /// <param name="N">The value to count the bits from.</param>
        /// <returns>The population count.</returns>
        /// <remarks>
        /// Practically copied from wikipedia, see the details there.
        /// </remarks>
        public unsafe static int Q_PopCnt(ulong N)
        {
            //Bask in the unholy light of thy stillborn child
            //unlimited magic numbers
            N -= (N >> 1) & 0x5555555555555555;
            N = (N & 0x3333333333333333) + ((N >> 2) & 0x3333333333333333);
            N = (N + (N >> 4)) & 0x0f0f0f0f0f0f0f0f;
            N = (N * 0x0101010101010101) >> 56;
            return *(int*)&N;
        }

        /// <summary>
        /// Returns the Hamming Weight of the given <see cref="long"/>.
        /// Uses the fastest possible implementation that's not vectorized or a built in CPU instruction,
        /// should be used if the value is expected to be high.
        /// </summary>
        /// <param name="N">The value to count the bits from.</param>
        /// <returns>The population count.</returns>
        public unsafe static int Q_PopCnt(long N)
        {
            return Q_PopCnt(*(ulong*)&N);
        }

        /// <summary>
        /// Returns the Hamming Weight of the given <see cref="uint"/>.
        /// Uses the fastest possible implementation that's not vectorized or a built in CPU instruction,
        /// should be used if the value is expected to be high.
        /// </summary>
        /// <param name="N">The value to count the bits from.</param>
        /// <returns>The population count.</returns>
        public static int Q_PopCnt(uint N)
        {
            return Q_PopCnt((ulong)N);
        }

        /// <summary>
        /// Returns the Hamming Weight of the given <see cref="int"/>.
        /// Uses the fastest possible implementation that's not vectorized or a built in CPU instruction,
        /// should be used if the value is expected to be high.
        /// </summary>
        /// <param name="N">The value to count the bits from.</param>
        /// <returns>The population count.</returns>
        public unsafe static int Q_PopCnt(int N)
        {
            return Q_PopCnt(*(uint*)&N);
        }

        /// <summary>
        /// Gets the position of a single set bit in the given integer. 0 is no set bit, index starts at 1.
        /// That was also a warning, invalid values will return invalid results.
        /// </summary>
        /// <param name="N">The integer with a hamming weight of 1 or 0.</param>
        /// <returns>The 1-indexed position of the set bit, or 0 if there's no set bit.</returns>
        /// <remarks>
        /// Converting to the format we're using for performance, having a bit index represent a value is easy,
        /// but doing it backwards is a bit harder.
        /// 
        /// The obvious solution here would be logarithm, since mathematically,
        /// we would like to know the current power of 2 a value represents.
        /// 
        /// That's dog slow.
        /// 
        /// We're making the assumption that all values have a Hamming weight of 1 or 0, since only then is it possible to
        /// do the conversion backwards. In that case, a binary search algorithm on the bits would be ideal, but let's assume
        /// we have the memory space for mapping instead. If we do have the cache space that'll be a lot faster, since we might have
        /// matrices with hundreds of fields.
        /// 
        /// Of course, we can't just make a map with 32 bit integers, that would be huge. So we need a way to get a small and unique
        /// value for all the 33 possible cases.
        /// 
        /// Basically, a pseudo-hashmap.
        /// Doing modulo with a prime that's larger than 33 will give small and unique values for each case. 37 will do.
        /// 67 for 64 bits is also perfect.
        /// 
        /// We are not using a conventional Dictionary to avoid overhead, since that's a lot more complex 
        /// and the function calls and memory space for "buckets" together would make the binary search preferable.
        /// 
        /// Yes, it would be easier. Yes, I could've typed out a switch with 33 cases by the time I figured and typed this out.
        /// But where's the fun in that?
        /// </remarks>
        public static int GetBitPos(uint N)
        {
            return ConversionMap32[N % 37];
        }

        /// <summary>
        /// Gets the position of a single set bit in the given integer. 0 is no set bit, index starts at 1.
        /// That was also a warning, invalid values will return invalid results.
        /// </summary>
        /// <param name="N">The integer with a hamming weight of 1 or 0.</param>
        /// <returns>The 1-indexed position of the set bit, or 0 if there's no set bit.</returns>
        public static int GetBitPos(int N)
        {
            return ConversionMap32[((uint)N) % 37];
        }

        /// <summary>
        /// Gets the position of a single set bit in the given integer. 0 is no set bit, index starts at 1.
        /// That was also a warning, invalid values will return invalid results.
        /// </summary>
        /// <param name="N">The integer with a hamming weight of 1 or 0.</param>
        /// <returns>The 1-indexed position of the set bit, or 0 if there's no set bit.</returns>
        public static int GetBitPos(ulong N)
        {
            return ConversionMap64[N % 67];
        }

        /// <summary>
        /// Gets the position of a single set bit in the given integer. 0 is no set bit, index starts at 1.
        /// That was also a warning, invalid values will return invalid results.
        /// </summary>
        /// <param name="N">The integer with a hamming weight of 1 or 0.</param>
        /// <returns>The 1-indexed position of the set bit, or 0 if there's no set bit.</returns>
        public static int GetBitPos(long N)
        {
            return ConversionMap64[((ulong)N) % 67];
        }
    }
}
