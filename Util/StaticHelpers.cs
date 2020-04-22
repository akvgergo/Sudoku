using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Util
{
    static class StaticHelpers
    {
        /// <summary>
        /// Removes a random member of a collection and returns the removed element. The elements should ideally be unique.
        /// </summary>
        /// <param name="collection">The <see cref="ICollection{T}"/> to choose the element from.</param>
        /// <param name="rnd">The RNG to choose the element.</param>
        /// <returns></returns>
        public static MemberT PopRandom<MemberT>(this ICollection<MemberT> collection, QuickRand rnd)
        {
            var index = rnd.Getint() % collection.Count;
            var value = collection.ElementAt(index);
            collection.Remove(value);
            return value;
        }

        /// <summary>
        /// Shuffles the elements of the given <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="list">The list that should be shuffled.</param>
        /// <param name="rnd">The RNG to use for the operation.</param>
        /// <param name="iterations">The number of swaps to perform. Non-positive will shuffle for twice the list size.</param>
        public static void Shuffle<MemberT>(this IList<MemberT> list, QuickRand rnd, int iterations = 0)
        {
            if (iterations < 1)
                iterations = list.Count * 2;
            
            for (int i = 0; i < iterations; i++)
            {
                int index1 = Math.Abs(rnd.Getint() % list.Count), index2 = Math.Abs(rnd.Getint() % list.Count);
                var buffer = list[index1];
                list[index1] = list[index2];
                list[index2] = buffer;
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
    }
}
