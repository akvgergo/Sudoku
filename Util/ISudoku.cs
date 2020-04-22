using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sudoku
{
    /// <summary>
    /// Represents a game of Sudoku.
    /// </summary>
    interface ISudoku
    {
        /// <summary>
        /// The total number of Fields or "tiles" this Sudoku contains. The size of the puzzle.
        /// </summary>
        int FieldCount { get; }

        /// <summary>
        /// The total number of Rows this Sudoku contains. The height of the puzzle.
        /// </summary>
        int RowCount { get; }
        
        /// <summary>
        /// The total number of Columns this Sudoku contains. The width of the the puzzle.
        /// </summary>
        int ColumnCount { get; }
        
        /// <summary>
        /// The highest value a single field may contain. For a classic Sudoku, this would be 9.
        /// </summary>
        int MaxValue { get; }

        /// <summary>
        /// Whether this Sudoku has been properly filled to completion by an algorithm
        /// </summary>
        bool IsGenerated { get; }

        /// <summary>
        /// Whether this Sudoku has been reduced, so that it is ready to be solved by a user
        /// </summary>
        bool IsReduced { get; }

        /// <summary>
        /// Whether this Sudoku is guaranteed to be unique, meaning it has only one solution after reduction.
        /// Ideally, this is always true.
        /// </summary>
        bool IsUnique { get; }

        /// <summary>
        /// The seed this Sudoku was created with.
        /// </summary>
        string Seed { get; }

        /// <summary>
        /// Returns the minimum count of field constraints a given tile has (Region, row, column, diagonal, etc). 
        /// </summary>
        int MinFieldConstraints { get; }

        /// <summary>
        /// Returns the maximum count of field constraints a given tile can have (Region, row, column, diagonal, etc). 
        /// </summary>
        int MaxFieldConstraints { get; }

        /// <summary>
        /// Returns how many constraints (limiting areas such as rows, columns, regions...) a given field coordinate has.
        /// </summary>
        /// <param name="x">The x component of the coordinate.</param>
        /// <param name="y">The Y component of the coordinate.</param>
        /// <returns>An <see cref="int"/> count of all the constraints.</returns>
        int GetConstraintCountForField(int x, int y);

        /// <summary>
        /// Determines whether the provided tile coordinate is a valid one for this Sudoku.
        /// </summary>
        /// <param name="x">The x component of the coordinate.</param>
        /// <param name="y">The Y component of the coordinate.</param>
        /// <returns>A <see cref="bool"/> that is true if the coordinate is valid.</returns>
        bool IsValidTile(int x, int y);

        /// <summary>
        /// Returns a complete matrix filled with the current state of the Sudoku. 0 is empty field, -1 is invalid tile.
        /// Changes to the returned <see cref="Matrices.Matrix"/> should be avoided, since it may be the actual backing matrix.
        /// </summary>
        /// <returns>A standard <see cref="Matrices.Matrix"/>.</returns>
        Matrices.Matrix GetMatrix();

        /// <summary>
        /// Creates a visual component tree that is capable of displaying the complete Sudoku for interaction with the User.
        /// </summary>
        /// <returns>A <see cref="Grid"/> object that is the root of the visual component tree.</returns>
        Grid CreateVisual();
    }
}
