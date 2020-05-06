using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Master file for all point structures, since there isn't much of a reason to separate them.
/// The only reason for multiple types for the same task is to try to optimize for memory/cpu performance.
/// Bigger lists of points may actually benefit from compression because of cpu cache hits.
/// </summary>
namespace Sudoku
{
    /// <summary>
    /// Two 32 bit integers representing a point.
    /// </summary>
    public struct Point64
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point64(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// A single 32 bit integer representing a point.
    /// </summary>
    public struct Point32
    {
        int p;
        
        public int X {
            get { return p & 0x0000ffff; }
            set { p |= value; }
        }

        public int Y {
            get { return p >> 16; }
            set { p |= value << 16; }
        }

        public Point32(int x, int y)
        {
            unchecked
            {
                p = x | (y << 16);
            }
        }
    }

    /// <summary>
    /// A 16 bit integer representing a point.
    /// </summary>
    public struct Point16
    {
        short p;

        public int X {
            get { return p & 0x00ff; }
            set { p |= (short)value; }
        }

        public int Y {
            get { return p >> 8; }
            set { p = (short)(p | (short)((short)value << 8)); }
        }

        public Point16(int x, int y)
        {
            unchecked
            {
                p = (short)(x | (y << 8));
            }
        }
    }
}
