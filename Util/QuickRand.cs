﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Sudoku.Util
{
    /// <summary>
    /// A xorshift PRNG, specifically xorshift+. Full range 32 or 64 bits, watch out for negative values.
    /// </summary>
    /// <remarks>
    /// The traditional .NET <see cref="Random"/> is both slow and has a relatively small range (2^31).
    /// This is a faster and more ideal implementation, since security is no concern, we just want speed and range.
    /// It's just a xorshift lfsr, using the first algorithm that seemed good enough both looking at it and after
    /// some console output tests.
    /// 
    /// The reality is, we basically want to generate matrixes and I could go insane searching for an algorithm
    /// that's fast and good for that. Let's just go for speed and simplicity instead.
    /// 
    /// We also prefer pointer math over built in functions or converting, because conversion methods are slow
    /// and things like math.abs needs branch prediction, which would be prone to have a 50% fail...
    /// 
    /// Pretty much like everything in the project, it's not stupid proofed, use your head.
    /// </remarks>
    public sealed class QuickRand
    {
        /// <summary>
        /// Basically the only way to properly get 16 random bytes that doesn't run into pigeon hole.
        /// </summary>
        static RNGCryptoServiceProvider cryptoProv;

        /// <summary>
        /// The seeds.
        /// </summary>
        ulong store1, store2;

        /// <summary>
        /// Creates a new <see cref="QuickRand"/>, using the current time as a seed.
        /// It is recommended to use <see cref="CreateSecure()"/> instead for better seeding.
        /// </summary>
        public QuickRand()
        {
            store1 = (ulong)Environment.TickCount;
            store2 = (ulong)Environment.TickCount;

            //we need a bit of shuffling since TickCount is a bit low
            Getulong();
            Getulong();
            Getulong();
        }

        /// <summary>
        /// Creates a new <see cref="QuickRand"/> with the specified seed pair.
        /// </summary>
        public QuickRand(ulong lowerSeed, ulong higherSeed)
        {
            store1 = lowerSeed;
            store2 = higherSeed;
        }

        /// <summary>
        /// Creates a new <see cref="QuickRand"/> using the given string as a seed.
        /// </summary>
        /// <param name="seed">A base64 encoded string.</param>
        public QuickRand(string seed) : this(Convert.FromBase64String(seed)) { }

        /// <summary>
        /// Creates a new <see cref="QuickRand"/> and uses the given <see cref="byte"/>[] as a seed.
        /// </summary>
        /// <param name="seed">The seed data. Must be 16 bytes.</param>
        public unsafe QuickRand(byte[] seed)
        {
            fixed (byte* p = seed)
            {
                var p2 = (ulong*)p;
                store1 = *p2;
                ++p2;
                store2 = *p2;
            }
        }

        /// <summary>
        /// Creates a new instance using bytes generated by <see cref="RNGCryptoServiceProvider"/>.
        /// </summary>
        public static QuickRand CreateSecure()
        {
            if (cryptoProv == null)
            {
                cryptoProv = new RNGCryptoServiceProvider();
            }

            byte[] seed = new byte[16];
            cryptoProv.GetBytes(seed);
            return new QuickRand(seed);
        }

        /// <summary>
        /// Generates a random <see cref="ulong"/>.
        /// </summary>
        public ulong Getulong()
        {
            ulong N1 = store1, N2 = store2;
            store1 = store2;
            N1 ^= N1 << 23;
            N1 ^= N1 >> 17;
            N1 ^= N2 ^ (N2 >> 26);
            store2 = N1;

            return N1 + N2;
        }

        /// <summary>
        /// Generates a random <see cref="uint"/>.
        /// </summary>
        public unsafe uint Getuint()
        {
            //Supposedly, the low order bits are a bit linear so we take the higher ones instead
            ulong N = Getulong();
            return *(((uint*)&N) + 1);
        }

        /// <summary>
        /// Generates a random <see cref="long"/>.
        /// </summary>
        public unsafe long Getlong()
        {
            var N = Getulong();
            return *(long*)&N;
        }

        /// <summary>
        /// Generates a random <see cref="int"/>.
        /// </summary>
        public unsafe int Getint()
        {
            ulong N = Getulong();
            return *(((int*)&N) + 1);
        }

        /// <summary>
        /// Generates a random <see cref="ulong"/> in the specified range.
        /// </summary>
        /// <param name="min">The inclusive lower bound.</param>
        /// <param name="max">The exclusive higher bound.</param>
        public unsafe ulong GetRange(ulong min, ulong max)
        {
            return Getulong() % (max - min) + min;
        }

        /// <summary>
        /// Generates a random <see cref="long"/> in the specified range.
        /// </summary>
        /// <param name="min">The inclusive lower bound.</param>
        /// <param name="max">The exclusive higher bound.</param>
        public unsafe long GetRange(long min, long max)
        {
            return (long)(Getulong() % (ulong)(max - min)) + min;
        }

        /// <summary>
        /// Generates a random <see cref="uint"/> in the specified range.
        /// </summary>
        /// <param name="min">The inclusive lower bound.</param>
        /// <param name="max">The exclusive higher bound.</param>
        public unsafe uint GetRange(uint min, uint max)
        {
            return Getuint() % (max - min) + min;
        }

        /// <summary>
        /// Generates a random <see cref="int"/> in the specified range.
        /// </summary>
        /// <param name="min">The inclusive lower bound.</param>
        /// <param name="max">The exclusive higher bound.</param>
        public unsafe int GetRange(int min, int max)
        {
            return (int)(Getuint() % (max - min)) + min;
        }

        /// <summary>
        /// Gets the seed values of the current instance.
        /// </summary>
        public (ulong store1, ulong store2) GetState()
        {
            return (store1, store2);
        }

        /// <summary>
        /// Returns a base64 encoded string that stores the seeds of the current instance
        /// </summary>
        public unsafe string GetStateString()
        {
            byte[] buffer = new byte[16];
            fixed (byte* b = buffer)
            {
                var p = (ulong*)b;
                *p = store1;
                p++;
                *p = store2;
            }
            return Convert.ToBase64String(buffer);
        }
    }
}
