using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sudoku.Util
{

    /// <summary>
    /// Static methods that can't be put elsewhere. This is a nice way of saying a trash heap.
    /// </summary>
    static class StaticHelpers
    {

        /// <summary>
        /// DP to use for coordinate binding.
        /// </summary>
        public static DependencyProperty CoordProperty = DependencyProperty.RegisterAttached("Coord", typeof(Point32), typeof(UIElement));

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
        /// Returns the width and height of a Sudoku, calculated from the maximum field values.
        /// </summary>
        /// <param name="MaxValue">The highest value a field can have.</param>
        /// <returns>A tuple containing the width and height of a single region.</returns>
        /// <remarks>
        /// Side effect of making the generator self-contained. Note that this returns the value that
        /// "makes the most sense". The method asks for a Maxval, but it could also ask for width and height, or
        /// a single dimension if we always assume a perfect square, but with more flexibility comes
        /// a few logic issues, and some decisions to make.
        /// 
        /// If we asked for dimensions instead, we could make the argument that an 8x8 Sudoku with 4 4x4 regions is perfectly
        /// legal, since of course we could choose from 16 values to fit into that 8 for rows and columns, and there would be no
        /// need to use the same value twice. There are a few configurations where this issue could arise and not just with squares,
        /// so better to lay down the rule that we *need* to use all numbers.
        /// 
        /// So MaxVal is better in that regard. But is it better to make a say 12x12 grid into 12 3x4, or 12 2x6.
        /// The former of course makes more sense, but the better question is, what if we *want* a 2x6 RegionSize?
        /// It could be perfect for a multi-Sudoku later, or some other interesting game variant.
        /// 
        /// When we make things more abstract, easier to use, and above all flexible, we may run into similar issues.
        /// Multiple solutions that are better for some cases than others. The idea of making GeneratorArgs a thing was born
        /// while writing this method.
        /// 
        /// So yes, there is sometimes no cover-all answer, or we just don't want it, while a default behaviour makes sense.
        /// This method should return the answer that is the most logical, in our case the two least further apart numbers that when
        /// multiplied together give MaxVal. I might never use it, but it helped think to write it, so here it is.
        /// 
        /// For the love of all that is holy, prime check the input.
        /// </remarks>
        public static (int width, int height) GetRegionSize(int MaxValue)
        {
            int sqrt = (int)Math.Floor(Math.Sqrt(MaxValue));
            //a whole sqrt means we can abide traditional rules, best case
            if (sqrt * sqrt == MaxValue) return (sqrt, sqrt);

            //the least deviation from the sqrt will return the least further apart values.
            int width = sqrt + 1;
            while (width * sqrt != MaxValue)
            {
                if (width * sqrt > MaxValue)
                    sqrt--;
                else
                    width++;
            }
            return (width, sqrt);
        }

        /// <summary>
        /// Creates a standard grid that should be used for games that need a classic style one.
        /// </summary>
        /// <returns>A <see cref="Grid"/> object that is the root of the visual tree</returns>
        public static Grid CreateGrid(int regionWidth, int regionHeight)
        {
            Grid retGrid = new Grid();

            for (int i = 0; i < regionWidth; i++)
            {
                retGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < regionHeight; i++)
            {
                retGrid.RowDefinitions.Add(new RowDefinition() { });
            }


            for (int x = 0; x < regionWidth; x++)
            {
                for (int y = 0; y < regionHeight; y++)
                {
                    Border b = new Border()
                    {
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.Black
                    };
                    retGrid.Children.Add(b);
                    Grid.SetRow(b, y);
                    Grid.SetColumn(b, x);

                    Grid innerGrid = new Grid();

                    for (int i = 0; i < regionWidth; i++)
                    {
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    for (int i = 0; i < regionHeight; i++)
                    {
                        innerGrid.RowDefinitions.Add(new RowDefinition() { });
                    }

                    b.Child = innerGrid;
                    for (int x2 = 0; x2 < regionWidth; x2++)
                    {
                        for (int y2 = 0; y2 < regionHeight; y2++)
                        {
                            Border b2 = new Border()
                            {
                                BorderThickness = new Thickness(2),
                                BorderBrush = Brushes.Black
                            };

                            b2.SetValue(CoordProperty, new Point32(x2 + regionWidth * x, y2 + regionHeight * y));
                            innerGrid.Children.Add(b2);
                            Grid.SetRow(b2, y2);
                            Grid.SetColumn(b2, x2);
                            Label ContentLbl = new Label() { Width = double.NaN, Height = double.NaN, Padding = new Thickness(0) };
                            b2.Child = new Viewbox() { Child = ContentLbl, Stretch = Stretch.Uniform };
                        }
                    }
                }
            }
            
            return retGrid;
        }

    }
}
