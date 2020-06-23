using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.ImagePrepare;
using System.Drawing;

namespace LetterReader.Splitters
{
    internal class Corners
    {
        int[] sides = new int[4];

        internal int Left { get { return sides[0]; } set { sides[0] = value; } }
        internal int Right { get { return sides[1]; } set { sides[1] = value; } }
        internal int Top { get { return sides[2]; } set { sides[2] = value; } }
        internal int Bot { get { return sides[3]; } set { sides[3] = value; } }

        public static implicit operator Rectangle(Corners c)
        {
            return new Rectangle { X = c.Left, Y = c.Top, Width = c.Right - c.Left, Height = c.Bot - c.Top };
        }
    }

    public static class DefenseMechanism
    {
        private static void ResizeIfPossible(Corners rect, in Point pt)
        {
            if (rect.Left > pt.X)
            {
                rect.Left = pt.X;
            }
            else if (rect.Right < pt.X)
            {
                rect.Right = pt.X;
            }
            else if (rect.Top > pt.Y)
            {
                rect.Top = pt.Y;
            }
            else if (rect.Bot < pt.Y)
            {
                rect.Bot = pt.Y;
            }
        }

        private static ImageMatrix CutBlackRegion(int x, int y, ImageMatrix matrix)
        {
            Queue<Point> points = new Queue<Point>();
            List<Point> blacks = new List<Point>();
            points.Enqueue(new Point(x, y));
            Corners bounds = new Corners { Left = x, Top = y, Right = x + 1, Bot = y + 1};
            while (points.Count > 0)
            {
                var pt = points.Dequeue();
                blacks.Add(pt);
                if (matrix[pt.X + 1][pt.Y])
                {
                    var toPush = new Point { X = pt.X + 1, Y = pt.Y };
                    ResizeIfPossible(bounds, in toPush);
                    matrix[pt.X + 1][pt.Y] = false;
                    points.Enqueue(toPush);
                }
                if (matrix[pt.X][pt.Y + 1])
                {
                    var toPush = new Point { X = pt.X, Y = pt.Y + 1 };
                    ResizeIfPossible(bounds, in toPush);
                    matrix[pt.X][pt.Y + 1] = false;
                    points.Enqueue(toPush);
                }
                if (matrix[pt.X - 1][pt.Y])
                {
                    var toPush = new Point { X = pt.X - 1, Y = pt.Y };
                    ResizeIfPossible(bounds, in toPush);
                    matrix[pt.X - 1][pt.Y] = false;
                    points.Enqueue(toPush);
                }
                if (matrix[pt.X][pt.Y - 1])
                {
                    var toPush = new Point { X = pt.X, Y = pt.Y - 1 };
                    ResizeIfPossible(bounds, in toPush);
                    matrix[pt.X][pt.Y - 1] = false;
                    points.Enqueue(toPush);
                }

                if (matrix[pt.X + 1][pt.Y + 1])
                {
                    var toPush = new Point { X = pt.X + 1, Y = pt.Y + 1 };
                    ResizeIfPossible(bounds, in toPush);
                    matrix[pt.X + 1][pt.Y + 1] = false;
                    points.Enqueue(toPush);
                }
                if (matrix[pt.X - 1][pt.Y + 1])
                {
                    var toPush = new Point { X = pt.X - 1, Y = pt.Y + 1 };
                    ResizeIfPossible(bounds, in toPush);
                    matrix[pt.X - 1][pt.Y + 1] = false;
                    points.Enqueue(toPush);
                }
                if (matrix[pt.X - 1][pt.Y - 1])
                {
                    var toPush = new Point { X = pt.X - 1, Y = pt.Y - 1 };
                    ResizeIfPossible(bounds, in toPush);
                    matrix[pt.X - 1][pt.Y - 1] = false;
                    points.Enqueue(toPush);
                }
                if (matrix[pt.X + 1][pt.Y - 1])
                {
                    var toPush = new Point { X = pt.X + 1, Y = pt.Y - 1 };
                    ResizeIfPossible(bounds, in toPush);
                    matrix[pt.X + 1][pt.Y - 1] = false;
                    points.Enqueue(toPush);
                }
            }
            return new ImageMatrix(blacks, bounds);
        }

        /**<summary>Recursively distinguish black regions from given matrix</summary>
         * <returns>Black, contigous regions</returns>
         */
        public static List<ImageMatrix> RecursiveDistinguish(ImageMatrix toProcess)
        {
            var processed = toProcess.CopyToSafeMatrix();
            List<ImageMatrix> matrixes = new List<ImageMatrix>();
            for (int x = 0; x < processed.Width; ++x)
            {
                for (int y = 0; y < processed.Height; ++y)
                {
                    if (processed[x][y])
                    {
                        matrixes.Add(CutBlackRegion(x, y, processed));
                    }
                }
            }
            return matrixes;
        }
        /// <summary>
        /// Get X coordinates that are believed to connect two separate letters, by counting
        /// medium black pixel density.
        /// </summary>
        /// <param name="matrix">Matrix to find cut indexes.</param>
        /// <returns>List of X coords that are believed to separate letters.</returns>
        private static List<int> GetPossibleCutIndexes(ImageMatrix matrix)
        {
            List<KeyValuePair<int, int>> blacksToIndex = new List<KeyValuePair<int, int>>();
            int currentMinimum = 1000;
            for (int x = 0; x < matrix.Width; ++x)
            {
                var blacks = matrix[x].Count(s => s);
                if (blacks < currentMinimum)
                {
                    currentMinimum = blacks;
                    blacksToIndex.RemoveAll(kvp => kvp.Key >= currentMinimum + 3);
                    blacksToIndex.Add(new KeyValuePair<int, int>(blacks, x));
                }
                else if (blacks <= currentMinimum + 3)
                {
                    blacksToIndex.Add(new KeyValuePair<int, int>(blacks, x));
                }
            }
            return blacksToIndex.Select(kvp => kvp.Value).ToList();
        }

        private static ImageMatrix MatchWithCut(int begin, int cutIndex, ImageMatrix toProcess, List<ImageMatrix> matchers)
        {
            var partial = toProcess.CopyMatrix(new Rectangle { X = begin, Y = 0, Width = cutIndex - begin, Height = toProcess.Height });
            partial = MatrixTools.Scale(partial);
            return matchers.FirstOrDefault(m => MatrixTools.EqualPixelRatio(partial, m) > DATA.CompareRatio_Decent);
        }

        /**<summary>Splits a bunch of connected letters into single ones. Tries to cut them on a vertical line, 
         * where density of blacks is the lowest and compares them to matchers</summary>
         */
        public static List<ImageMatrix> SplitToSingleLetters(ImageMatrix toProcess, List<ImageMatrix> matchers)
        {
            List<ImageMatrix> toRet = new List<ImageMatrix>();
            var cutIndexes = GetPossibleCutIndexes(toProcess);
            int begin = 0;
            foreach (int cutIndex in cutIndexes)
            {
                var match = MatchWithCut(begin, cutIndex, toProcess, matchers);
                if (match != null)
                {
                    toRet.Add(match);
                    begin = cutIndex;
                }
            }
            if (toRet.Count == 1)
            {
                toRet.Add(toProcess.CopyMatrix(new Rectangle { X = begin, Y = 0, Height = toProcess.Height, Width = toProcess.Width - begin }));
                Console.WriteLine("DefenseMechanism-SplitToSingleLetters: WARNING, recognized first part, but not second part");
            }
            if (toRet.Count == 0)
            {
                Console.WriteLine("DefenseMechanism-SplitToSingleLetters: WARNING, couldnt recognze any letters");
            }
            return toRet;
        }
    }
}
