using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuBeta
{
    /// <summary>
    /// Simple 9x9 classic Sudoku
    /// </summary>
    class Sudoku
    {
        /// <summary>
        /// The most basic Sudoku that we'll permutate to get a more interesting one. This is basically what one would get if they were to bruteforce solve an empty grid.
        /// As long as we use transmutations that don't break the rules, we can create any proper Sudoku from this.
        /// </summary>
        readonly static int[,] Base = new int[,] {
            { 1, 2, 3, 4, 5, 6, 7, 8, 9},
            { 4, 5, 6, 7, 8, 9, 1, 2, 3},
            { 7, 8, 9, 1, 2, 3, 4, 5, 6},
            { 2, 3, 1, 5, 6, 4, 8, 9, 7},
            { 5, 6, 4, 8, 9, 7, 2, 3, 1},
            { 8, 9, 7, 2, 3, 1, 5, 6, 4},
            { 3, 1, 2, 6, 4, 5, 9, 7, 8},
            { 6, 4, 5, 9, 7, 8, 3, 1, 2},
            { 9, 7, 8, 3, 1, 2, 6, 4, 5}
        };

        /// <summary>
        /// The Contents that the user sees and sets
        /// </summary>
        public int[,] Cells = new int[9, 9];
        /// <summary>
        /// The actual solution, or at least one of the possible solutions
        /// </summary>
        public int[,] Solution = new int[9, 9];
        
        public Difficulty Difficulty { get; }

        /// <summary>
        /// Creates a new Sudoku, and reduces it to the specified difficulty. May or may not have a single solution.
        /// </summary>
        /// <param name="diff"></param>
        public Sudoku(Difficulty diff, int seed)
        {
            Difficulty = diff;
            Random rnd = new Random(seed);

            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Solution[x, y] = Base[x, y];
                }
            }
            Validate();
            //step one: randomly transmute the base grid in different ways to get a solution
            
            //just some vars to reuse for the random operations
            int dice;
            //How many times I do the different shuffles is totally spitballed. May adjust later for performance but sufficient uniquity.
            int count;

            //row and column switches: swapping rows or columns inside their horizontal or vertical regions respectively is allowed
            count = rnd.Next(20, 30);
            for (int i = 0; i < count; i++)
            {
                //3 possible operations in 3 regions on 2 sides of the grid
                dice = rnd.Next(0, 18);
                if (dice < 9)
                {
                    //row switch
                    int y1 = dice, y2 = (dice % 3) == 2 ? dice - 2 : dice + 1;
                    for (int x = 0; x < 9; x++)
                    {
                        var buffer = Solution[x, y1];
                        Solution[x, y1] = Solution[x, y2];
                        Solution[x, y2] = buffer;
                    }
                } else
                {
                    //column switch
                    dice -= 9;
                    int x1 = dice, x2 = (dice % 3) == 2 ? dice - 2 : dice + 1;
                    for (int y = 0; y < 9; y++)
                    {
                        var buffer = Solution[x1, y];
                        Solution[x1, y] = Solution[x2, y];
                        Solution[x2, y] = buffer;
                    }

                }
            }
            Validate();
            ////Doing the same, but with stacks of regions
            //count = rnd.Next(5, 10);
            //for (int i = 0; i < count; i++)
            //{
            //    //3 possible operations on 2 sides
            //    dice = rnd.Next(0, 6);
            //    if (dice < 3)
            //    {
            //        int y1 = 3 * dice, y2 = 3 * (dice % 3) == 2 ? dice - 2 : dice + 1;
            //        for (int x = 0; x < 9; x++)
            //        {
            //            var buffer = Solution[x, y1];
            //            Solution[x, y1] = Solution[x, y2];
            //            Solution[x, y2] = buffer;
            //            buffer = Solution[x, y1 + 1];
            //            Solution[x, y1 + 1] = Solution[x, y2 + 1];
            //            Solution[x, y2 + 1] = buffer;
            //            buffer = Solution[x, y1 + 2];
            //            Solution[x, y1 + 2] = Solution[x, y2 + 2];
            //            Solution[x, y2 + 2] = buffer;

            //        }
            //    } else
            //    {
            //        dice -= 3;
            //        int x1 = 3 * dice, x2 = 3 * (dice % 3) == 2 ? dice - 2 : dice + 1;
            //        for (int y = 0; y < 9; y++)
            //        {
            //            var buffer = Solution[x1, y];
            //            Solution[x1, y] = Solution[x2, y];
            //            Solution[x2, y] = buffer;
            //            buffer = Solution[x1 + 1, y];
            //            Solution[x1 + 1, y] = Solution[x2 + 1, y];
            //            Solution[x2 + 1, y] = buffer;
            //            buffer = Solution[x1 + 2, y];
            //            Solution[x1 + 2, y] = Solution[x2 + 2, y];
            //            Solution[x2 + 2, y] = buffer;

            //        }
            //    }
            //}
            Validate();
            //Mirroring is perfectly legal
            if (rnd.Next(0, 2) == 0)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (x == 4) continue;
                    for (int y = 0; y < 9; y++)
                    {
                        var buffer = Solution[x, y];
                        Solution[x, y] = Solution[8 - x, y];
                        Solution[8 - x, y] = buffer;
                    }
                }
            }

            //both ways
            if (rnd.Next(0, 2) == 0)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (y == 4) continue;
                    for (int x = 0; x < 9; x++)
                    {
                        var buffer = Solution[x, y];
                        Solution[x, y] = Solution[x, 8 - y];
                        Solution[x, 8 - y] = buffer;
                    }
                }
            }

            ////So is rotating
            //dice = rnd.Next(0, 4);
            //for (int i = 0; i < dice; i++)
            //{
            //    for (int x = 0; x < 9; x++)
            //    {
            //        for (int y = 0; y < 9; y++)
            //        {
            //            var buffer = Solution[x, y];
            //            Solution[x, y] = Solution[y, 8 - x];
            //            Solution[y, 8 - x] = buffer;
            //        }
            //    }
            //}
            Validate();
            //and finally, switching digits is fine as long as we're switching across the entire grid
            count = rnd.Next(4, 8);
            for (int i = 0; i < count; i++)
            {
                int digit1 = rnd.Next(1, 10), digit2 = rnd.Next(1, 10);
                if (digit1 == digit2) continue;
                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        if (Solution[x, y] == digit1)
                        {
                            Solution[x, y] = digit2;
                        }
                        else if (Solution[x, y] == digit2)
                        {
                            Solution[x, y] = digit1;
                        }
                    }
                }
            }
            Validate();
            Reduce(rnd);
        }


        void Validate()
        {
            var check = 45;

            for (int i = 0; i < 9; i++)
            {
                int sumX = 0, sumY = 0;
                for (int j = 0; j < 9; j++)
                {
                    sumX += Solution[i, j];
                    sumY += Solution[j, i];
                }
                if (sumY != check || sumX != check)
                {
                    throw new Exception("Illegal move!");
                }
            }

            for (int i = 0; i < 9; i++)
            {
                int sumR = 0;
                (int X, int Y) coords = (i % 3, i / 3);
                Console.WriteLine(coords.X * 3 + 3);
                for (int x = coords.X * 3;  x < coords.X * 3 + 3; x++)
                {
                    for (int y = coords.Y * 3; y < coords.Y * 3 + 3; y++)
                    {
                        sumR += Solution[x, y];
                    }
                }

                if (sumR != check)
                {
                    throw new Exception("Illegal move!");
                }
            }
        }


        /// <summary>
        /// Remove a random amount of the visible numbers, depending on difficulty and trying to keep the Sudoku "proper",
        /// meaning trying to keep only one possible solution
        /// </summary>
        protected void Reduce(Random rnd)
        {
            int[,] bitCells = new int[9, 9];
            int[] caches = new int[27];

            //Create a randomly ordered list of all cells
            List<Tuple<int, int>> cellList = new List<Tuple<int, int>>(81);
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    bitCells[x, y] = 1 << (Solution[x, y] - 1);
                    cellList.Add(new Tuple<int, int>(x, y));
                }
            }

            for (int i = 0; i < 100; i++)
            {
                int first = rnd.Next(81), second = rnd.Next(81);
                var buffer = cellList[first];
                cellList[first] = cellList[second];
                cellList[second] = buffer;
            }

            for (int i = 0; i < caches.Length; i++)
            {
                caches[i] = 0x000001FF;
            }

            //iterate over the list until the Sudoku is minimal
            while (true)
            {
                bool anyRemoved = false;
                foreach (var item in cellList)
                {
                    int x = item.Item1, y = item.Item2;
                    if (bitCells[x, y] == 0) continue;

                    int deduction = ~(caches[x] | caches[9 + y] | caches[18 + x / 3 + y / 3 * 3]) & 0x000001FF;
                    if ((deduction - 1 & deduction) == 0)
                    {
                        caches[x] -= bitCells[x, y];
                        caches[9 + y] -= bitCells[x, y];
                        caches[18 + x / 3 + y / 3 * 3] -= bitCells[x, y];
                        bitCells[x, y] = 0;
                        anyRemoved = true;
                    }
                }

                if (!anyRemoved) break;
            }

            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cells[x, y] = bitCells[x, y] == 0 ? 0 : (int)Math.Log(bitCells[x, y], 2) + 1;
                }
            }

        }

        public override bool Equals(object obj)
        {
            Console.WriteLine("Full compare");
            var other = obj as Sudoku;
            if (other == null) return false;

            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (Solution[x, y] != other.Solution[x, y])
                        return false;
                    
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int diag1 = 0, diag2 = 0;
            unchecked
            {
                for (int i = 0; i < 9; i++)
                {
                    diag1 += Solution[i, i] << (3 * i);
                    diag2 += Solution[i, 8 - i] << (3 * i);
                }

                return diag1 * diag2;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    builder.Append(Cells[x, y]);
                    builder.Append(" ");
                }
                builder.Append('\n');
            }
            return builder.ToString();
        }
    }

    enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
}
