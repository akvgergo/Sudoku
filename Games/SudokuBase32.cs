using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Sudoku.Matrices;
using Sudoku.Util;

namespace Sudoku.Games
{
    /// <summary>
    /// Represents a game of Sudoku.
    /// </summary>
    public abstract class SudokuBase32
    {

        /// <summary>
        /// The backing matrix to store the field values for internal operations.
        /// </summary>
        protected Bit32Matrix Matrix { get; set; }

        /// <summary>
        /// The internal RNG to use for all randomized operations.
        /// </summary>
        protected QuickRand RNG { get; set; }

        /// <summary>
        /// Stores the possible values for generation and reduction.
        /// </summary>
        /// <remarks>
        /// Alright, if you just glanced at the classes and all the stuff that I wrote so far
        /// you may get the idea that a lot of it is unnecessary, but here is where it all gets pieced together.
        /// 
        /// When you generate a Sudoku puzzle, any variant for that matter, you'll have a bunch of tiles and constraints
        /// on those tiles, that define what value can go into that tile. Gross generalization, but should be true for every
        /// game variant not too exotic.
        /// 
        /// We would like to generate as many of the variants and valid games as possible. So randomly filling tiles and then solving it
        /// should give the most entropy for a game. The backdraw of this is that it requires a solver algorithm,
        /// but we would need one anyway for SudokuX for example, where generation with permutation is not possible.
        /// We also need this to be generic, unless we want to go mad for each Sudoku variant. Pretty much the only algorithm
        /// capable of this is brute force.
        /// 
        /// As the name suggests, that's a bit slow, but has many opportunities for optimization. What we'll be doing here
        /// is a bit odd, but makes sense.
        /// 
        /// Each value in the matrix will be represented by a bit in an integer. The only place where we have to convert will
        /// be for actual display. 0 is unknown, invalid value, whatever is necessary in the context, everything else is a number,
        /// specifically 2^(value - 1).
        /// 
        /// The caches will be storing what numbers we have so far in the given constraint, so we don't have to search the entire
        /// matrix for values. When we set a value in a matrix, the caches are also set. Later we can use these caches with some
        /// bit operations to determine what values can go into a tile. Nice and O(1), as long as we can fit into 32 or 64 bits,
        /// this will work well, the only exponential variable in the solving will be the actual size of the Sudoku. Can't really
        /// dodge filling all the digits in the grid, or at the very least I'm not smart enough for that kind of deduction,
        /// and it sure as hell wouldn't be this generic.
        /// 
        /// All the inheritors just have to define how the caches work for the fields, and that's it, the solver does the rest,
        /// but with a few caveats. When we are optimized to this level, virtual calls, regular calls, and inlining starts to
        /// matter a lot. Unsafe code shoould be generally avoided, since that limits what JIT can get rid of. Also, seal and mark
        /// for inlining everything that's possible or not stupid to do so.
        /// </remarks>
        protected int[][] Caches { get; set; }

        /// <summary>
        /// The total number of Fields or "tiles" this Sudoku contains. The size of the puzzle.
        /// </summary>
        public virtual int FieldCount { get; protected set; }

        /// <summary>
        /// The total number of Rows this Sudoku contains. The height of the puzzle.
        /// </summary>
        public virtual int RowCount { get; protected set; }

        /// <summary>
        /// The total number of Columns this Sudoku contains. The width of the the puzzle.
        /// </summary>
        public virtual int ColumnCount { get; protected set; }

        /// <summary>
        /// The highest value a single field may contain. For a classic Sudoku, this would be 9.
        /// </summary>
        public virtual int MaxValue { get; protected set; }

        /// <summary>
        /// Returns the width of a region in the current puzzle.
        /// </summary>
        public virtual int RegionWidth { get; protected set; }

        /// <summary>
        /// Returns the height of a region in the current puzzle.
        /// </summary>
        public virtual int RegionHeight { get; protected set; }

        /// <summary>
        /// Whether this Sudoku has been properly filled to completion by an algorithm
        /// </summary>
        public virtual bool IsGenerated { get; protected set; }

        /// <summary>
        /// Whether this Sudoku has been reduced, so that it is ready to be solved by a user
        /// </summary>
        public virtual bool IsReduced { get; protected set; }

        /// <summary>
        /// Whether this Sudoku is guaranteed to be unique, meaning it has only one solution after reduction.
        /// Ideally, this is always true.
        /// </summary>
        public virtual bool IsUnique { get; protected set; }

        /// <summary>
        /// The seed this Sudoku was created with.
        /// </summary>
        public virtual string Seed { get; protected set; }

        /// <summary>
        /// Returns the minimum count of field constraints a given tile has (Region, row, column, diagonal, etc). 
        /// </summary>
        public virtual int MinFieldConstraints { get; protected set; }

        /// <summary>
        /// Returns the maximum count of field constraints a given tile can have (Region, row, column, diagonal, etc). 
        /// </summary>
        public virtual int MaxFieldConstraints { get; protected set; }

        /// <summary>
        /// Returns how many constraints (limiting areas such as rows, columns, regions...) a given field coordinate has.
        /// </summary>
        /// <param name="x">The x component of the coordinate.</param>
        /// <param name="y">The Y component of the coordinate.</param>
        /// <returns>An <see cref="int"/> count of all the constraints.</returns>
        public abstract int GetConstraintCountForField(int x, int y);

        /// <summary>
        /// Determines whether the provided tile coordinate is a valid one for this Sudoku.
        /// </summary>
        /// <param name="x">The x component of the coordinate.</param>
        /// <param name="y">The Y component of the coordinate.</param>
        /// <returns>A <see cref="bool"/> that is true if the coordinate is valid.</returns>
        public virtual bool IsValidTile(int x, int y)
        {
            return x >= 0 && x < ColumnCount && y >= 0 && y < RowCount;
        }

        /// <summary>
        /// Returns the cached possible values for a given tile coord.
        /// </summary>
        protected abstract int GetTileCache(int x, int y);

        /// <summary>
        /// Adds the given value to the proper caches.
        /// </summary>
        protected abstract void AddTileCache(int x, int y, int value);

        /// <summary>
        /// Removes the value from the proper caches. May make a mess if the value isn't there.
        /// </summary>
        protected abstract void RemoveTileCache(int x, int y, int value);

        /// <summary>
        /// Creates the constraint caches that will be used for generation and reduction.
        /// </summary>
        protected abstract void CreateCaches();

        /// <summary>
        /// Returns a complete matrix filled with the current state of the Sudoku. 0 is empty field, -1 is invalid tile.
        /// Changes to the returned <see cref="Matrices.Matrix"/> should be avoided, since it may be the actual backing matrix.
        /// </summary>
        /// <returns>A standard <see cref="Matrices.Matrix"/>.</returns>
        public virtual Matrix GetMatrix()
        {
            return Matrix.ToMatrix();
        }

        /// <summary>
        /// Creates a visual component tree that is capable of displaying the complete Sudoku for interaction with the User.
        /// </summary>
        /// <returns>A <see cref="Grid"/> object that is the root of the visual component tree.</returns>
        public abstract Grid CreateVisual();

        /// <summary>
        /// Fills the entire matrix with values.
        /// </summary>
        /// <returns>Returns true if the algorithm succeeded, false if it failed.</returns>
        protected bool BruteFill(bool deterministic = true)
        {
            CreateCaches();
            List<SolverTile> emptyFields = new List<SolverTile>(Matrix.FieldCount);
            int pointer = 0;

            for (int y = 0; y < Matrix.Height; y++)
            {
                for (int x = 0; x < Matrix.Width; x++)
                {
                    if (Matrix[x, y] == 0)
                    {
                        emptyFields.Add(new SolverTile(x, y, 0));
                    }
                }
            }

            if (emptyFields.Count == 0) return true;

            if (!deterministic)
            {
                emptyFields.Shuffle(RNG);
            }

            int maxpointer = pointer;
            while (pointer > -1 && pointer < emptyFields.Count)
            {
                if (pointer > maxpointer)
                {
                    maxpointer = pointer;
                    Matrix.ToMatrix().Print();
                }
                SolverTile field = emptyFields[pointer];

                if (field.Value == 0)
                {
                    field.Value = GetTileCache(field.Location.X, field.Location.Y);
                    if (field.Value == 0) {
                        if (Matrix[field.Location] != 0)
                        {
                            RemoveTileCache(field.Location.X, field.Location.Y, Matrix[field.Location]);
                            Matrix[field.Location] = 0;
                        }
                        pointer--;
                        continue;
                    }
                }

                RemoveTileCache(field.Location.X, field.Location.Y, Matrix[field.Location]);

                int newVal = field.Value & (field.Value - 1);
                Matrix[field.Location] = field.Value ^ newVal;
                field.Value = newVal;
                AddTileCache(field.Location.X, field.Location.Y, Matrix[field.Location]);
                pointer++;
            }

            if (pointer < 0) return false;
            return true;
        }

        /// <summary>
        /// Used by the brutefill algorithm to keep track of field values.
        /// </summary>
        struct SolverTile
        {
            public Point16 Location { get; set; }
            public int Value { get; set; }

            public SolverTile(int x, int y, int value)
            {
                Location = new Point16(x, y);
                Value = value;
            }
        }
    }
}
