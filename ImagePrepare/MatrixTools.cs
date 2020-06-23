using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LetterReader.ImagePrepare
{
    public class MatrixTools
    {
        private ImageMatrix matrix;

        private int GetCountOfBlackInRow(int y)
        {
            int count = 0;
            for (int x = 0; x < matrix.Width; ++x)
            {
                if (matrix[x][y]) count++;
            }
            return count;
        }

        private int GetCountOfBlackInCol(int x)
        {
            int count = 0;
            for (int y = 0; y < matrix.Height; ++y)
            {
                if (matrix[x][y]) count++;
            }
            return count;
        }

        private int FindTextBegin(int avg, int[] array)
        {
            for(int i = 0; i < array.Length - 1; ++i)
            {
                if (array[i] > avg && array[i + 1] > avg)
                    return i - 15 >= 0 ? i - 15 : 0; // leave some white space so far
            }
            Console.WriteLine("WARNING: Didnt find suitable begin horizont from left with avg={0}", avg);
            return 0;
        }

        private int FindTextEnd(int avg, int[] array)
        {
            for (int i = array.Length - 1; i > 0; --i)
            {
                if (array[i] > avg && array[i - 1] > avg)
                    return i + 15 <= array.Length - 1 ? i + 15 : array.Length - 1; // leave some white space so far
            }
            Console.WriteLine("WARNING: Didnt find suitable begin horizont from right with avg={0}", avg);
            return 0;
        }

        /**<summary>
         * Calculate average of black pixels in rows and cols
         * if section contains more than the average - its text
         * if a few sections in a row contains and few before dont - its text begin
         * </summary>
         * <rethink>
         * How to trim white spaces around perfectly
         * <sol>having the plausible text begin corner, iterate up and left, until completely white col and row is found</sol>
         * </rethink>
         */
        public Rectangle FindTextSubmatrix()
        {
            int[] horizontalFill = new int[matrix.Width];
            int[] verticalFill = new int[matrix.Height];
            var factory = new TaskFactory();
            var tasks = new List<Task>();
            int avgH = 0;
            int avgV = 0;

            for (int x = 0; x < horizontalFill.Length; ++x)
                tasks.Add(factory.StartNew((cord) => {
                    int count = GetCountOfBlackInCol((int)cord);
                    if (count > 5)
                        avgH += horizontalFill[(int)cord] = count;
                    }, x));
            for (int y = 0; y < verticalFill.Length; ++y)
                tasks.Add(factory.StartNew((cord) => {
                    int count = GetCountOfBlackInRow((int)cord);
                    if (count > 5)
                        avgV += verticalFill[(int)cord] = count;
                    }, y));

            Task.WaitAll(tasks.ToArray());
            avgH /= horizontalFill.Length; avgV /= verticalFill.Length;
            int xb = FindTextBegin(avgH / 5, horizontalFill), xe = FindTextEnd(avgH / 5, horizontalFill),
                yb = FindTextBegin(avgV / 5, verticalFill), ye = FindTextEnd(avgV / 10, verticalFill);
            return new Rectangle
            {
                X = xb,
                Y = yb,
                Width = xe - xb,
                Height = ye - yb
            };
        }

        private static int Less(int a, int b)
        {
            return a > b ? b : a;
        }

        private static int Greater(int a, int b)
        {
            return a > b ? a : b;
        }

        /// <summary>
        /// Clear white lines from all sides as long as they are all white
        /// </summary>
        /// <returns></returns>
        public Rectangle TrimWhiteAreaAround()
        {
            Task<Point>[] tasks = new Task<Point>[4];
            tasks[0] = Task<Point>.Factory.StartNew(() =>
            {
                for (int x = 0; x < matrix.Width; ++x)
                {
                    for (int y = 0; y < matrix.Height; ++y)
                    {
                        if (matrix[x][y])
                            return new Point(x,y);
                    }
                }
                return Point.Empty;
            });
            tasks[1] = Task<Point>.Factory.StartNew(() =>
            {
                for (int y = 0; y < matrix.Height; ++y)
                {
                    for (int x = 0; x < matrix.Width; ++x)
                    {
                        if (matrix[x][y])
                            return new Point(x,y);
                    }
                }
                return Point.Empty;
            });
            tasks[2] = Task<Point>.Factory.StartNew(() =>
            {
                for (int x = matrix.Width - 1; x > -1; --x)
                {
                    for (int y = matrix.Height - 1; y > -1; --y)
                    {
                        if (matrix[x][y])
                            return new Point(x,y);
                    }
                }
                return Point.Empty;
            });
            tasks[3] = Task<Point>.Factory.StartNew(() =>
            {
                for (int y = matrix.Height - 1; y > -1; --y)
                {
                    for (int x = matrix.Width - 1; x > -1; --x)
                    {
                        if (matrix[x][y])
                            return new Point(x,y);
                    }
                }
                return Point.Empty;
            });
            int lBoundX = tasks.Min(xy => xy.Result.X);
            int lBoundY = tasks.Min(xy => xy.Result.Y);
            int rBoundX = tasks.Max(xy => xy.Result.X);
            int rBoundY = tasks.Max(xy => xy.Result.Y);
            return new Rectangle
            {
                X = lBoundX,
                Y = lBoundY,
                Width = rBoundX - lBoundX,
                Height = rBoundY - lBoundY
            };
        }

        public static float EqualPixelRatio(SubMatrix lhs, SubMatrix rhs)
        {
            int eq = 0;
            for (int x = 0; x < lhs.Width; ++x)
            {
                for (int y = 0; y < lhs.Height; ++y)
                {
                    if (!(rhs.Get(x, y) ^ lhs.Get(x, y)))
                    {
                        eq++;
                    }
                }
            }
            return (float)eq / (lhs.Width * lhs.Height);
        }

        public static float EqualPixelRatio(ImageMatrix lhs, SubMatrix rhs)
        {
            int eq = 0;
            for (int x = 0; x < lhs.Width; ++x)
            {
                for (int y = 0; y < lhs.Height; ++y)
                {
                    if (!(rhs.Get(x, y) ^ lhs.Get(x, y)))
                    {
                        eq++;
                    }
                }
            }
            return (float)eq / (lhs.Width * lhs.Height);
        }

        public static float EqualPixelRatio(SubMatrix lhs, ImageMatrix rhs)
        {
            int eq = 0;
            for (int x = 0; x < lhs.Width; ++x)
            {
                for (int y = 0; y < lhs.Height; ++y)
                {
                    if (!(rhs.Get(x, y) ^ lhs.Get(x, y)))
                    {
                        eq++;
                    }
                }
            }
            return (float)eq / (lhs.Width * lhs.Height);
        }

        public static float EqualPixelRatio(ImageMatrix lhs, ImageMatrix rhs)
        {
            int eq = 0;
            for (int x = 0; x < lhs.Width; ++x)
            {
                for (int y = 0; y < lhs.Height; ++y)
                {
                    if (!(rhs.Get(x, y) ^ lhs.Get(x, y)))
                    {
                        eq++;
                    }
                }
            }
            return (float)eq / (lhs.Width * lhs.Height);
        }

        private static void Swap<T1>(ref T1 lhs, ref T1 rhs) 
        {
            var tmp = lhs;
            lhs = rhs;
            rhs = tmp;
        }

        private static bool PrecheckIfOneBigger(ImageMatrix lhs, ImageMatrix rhs)
        {
            return
                (lhs.Width >= rhs.Width && lhs.Height >= rhs.Height) ||
                (rhs.Width >= lhs.Width && rhs.Height >= lhs.Height);
        }

        /// <summary>
        /// Compares matrices by iterating first scaledX, then scaledY
        /// </summary>
        /// <param name="lhs">Lefthand side to compare</param>
        /// <param name="rhs">Righthand side to compare</param>
        /// <returns>Equal pixel ratio.</returns>
        public static float EqualPixelRatioVaryingSize(ImageMatrix lhs, ImageMatrix rhs)
        {
            //Keep lhs smaller
            if (PrecheckIfOneBigger(lhs, rhs))
            {
                return EqualPixelRatioScaleBased(lhs, rhs);
            }
            if (lhs.Width > rhs.Width)
            {
                Swap(ref lhs, ref rhs);
            }
            float xScale = (float)lhs.Width / rhs.Width;
            int eq = 0;
            int count = rhs.Width * rhs.Height;
            for (int x = 0; x < rhs.Width; ++x)
            {
                for (int y = 0; y < rhs.Height; ++y)
                {
                    if (rhs[x][y] == lhs[(int)(x * xScale)][y])
                    {
                        eq++;
                    }
                }
            }
            if (lhs.Height > rhs.Height)
                Swap(ref lhs, ref rhs);
            count += rhs.Width * rhs.Height;
            float yScale = (float)lhs.Height / rhs.Height;
            for (int x = 0; x < rhs.Width; ++x)
            {
                for (int y = 0; y < rhs.Height; ++y)
                {
                    if (rhs[x][y] == lhs[x][(int)(y * yScale)])
                    {
                        eq++;
                    }
                }
            }
            return (float)eq / count;
        }

        public static ImageMatrix Scale(ImageMatrix matrix, float scale)
        {
            int width = (int)(matrix.Width * scale);
            int height = (int)(matrix.Height * scale);
            bool[][] mat = new bool[width][];
            for (int x = 0; x < width; ++x)
            {
                mat[x] = new bool[height];
                for (int y = 0; y < height; ++y)
                {
                    try
                    {
                        mat[x][y] = matrix[(int)(x / scale)][(int)(y / scale)];
                    }
                    catch (IndexOutOfRangeException) {
                        Console.WriteLine($"Float scaling failed: {(int)(x * scale)} {(int)(y * scale)}"); }
                }
            }
            var toRet = new ImageMatrix(mat);
            toRet.Character = matrix.Character;
            return toRet;
        }

        public static ImageMatrix Scale(ImageMatrix matrix, int desiredWidth, int desiredHeight)
        {
            float xScale = (float)matrix.Width / desiredWidth;
            float yScale = (float)matrix.Height / desiredHeight;
            bool[][] mat = new bool[desiredWidth][];
            for (int x = 0; x < desiredWidth; ++x)
            {
                mat[x] = new bool[desiredHeight];
                for (int y = 0; y < desiredHeight; ++y)
                {
                    try
                    {
                        mat[x][y] = matrix[(int)(x * xScale)][(int)(y * yScale)];
                    }
                    catch (IndexOutOfRangeException) {
                        Console.WriteLine($"Float scaling failed: {(int)(x * xScale)} {(int)(y * yScale)}"); }
                }
            }
            return new ImageMatrix(mat);
        }

        public static ImageMatrix Scale(ImageMatrix matrix)
        {
            float xScale = (float)matrix.Width / DATA.letterWidth;
            float yScale = (float)matrix.Height / DATA.letterHeight;
            bool[][] mat = new bool[DATA.letterWidth][];
            for (int x = 0; x < DATA.letterWidth; ++x)
            {
                mat[x] = new bool[DATA.letterHeight];
                for (int y = 0; y < DATA.letterHeight; ++y)
                {
                    mat[x][y] = matrix[(int)(x * xScale)][(int)(y * yScale)];
                }
            }
            return new ImageMatrix(mat);
        }

        private static void InvalidMatrix(ImageMatrix first, ImageMatrix second) =>
            throw new InvalidOperationException(string.Format("Matrix passed is bigger in only one dimension: W{0} H{1} , second W{2} H{3}",
                    first.Width, first.Height, second.Width, second.Height));

        private static bool SecondBigger(ImageMatrix first, ImageMatrix second)
        {
            if(first.Height > second.Height)
            {
                if (first.Width < second.Width)
                {
                    InvalidMatrix(first, second);
                }
            }
            else if (second.Height > first.Height)
            {
                if (second.Width < first.Width)
                {
                    InvalidMatrix(first, second);
                }
            }
            return second.Height > first.Height || second.Width > first.Width;
        }

        /// <summary>
        /// Performs matrix compare on different sizes matrices. One matrix must be both in height and width bigger. Throws if one dimension is bigger and the other smaller.
        /// PROBLEMO: if one HW ratio is 1.01 and other is 0.99 segfault. Please fix.
        /// </summary>
        /// <param name="first">First to compare</param>
        /// <param name="second">Second to compare</param>
        /// <returns>Equal pixes compare ratio</returns>
        public static float EqualPixelRatioScaleBased(ImageMatrix first, ImageMatrix second)
        {
            if (SecondBigger(first, second))
            {
                Swap(ref first, ref second);
            }
            float xScale = (float)first.Width / second.Width;
            float yScale = (float)first.Height / second.Height;
            int count = 0;
            var matrixF = first.Matrix;
            var matrixS = second.Matrix;
            int rangeX = Math.Min(first.Width, second.Width * (int)xScale);
            for (int x = 0; x < first.Width; ++x)
            {
                for (int y = 0; y < first.Height; ++y)
                {
                    if (matrixS[(int)(x / xScale)][(int)(y / yScale)] == matrixF[x][y]) count++;
                }
            }
            return (float)count / (first.Width * first.Height);
        }

        public MatrixTools(ImageMatrix mat)
        {
            matrix = mat;
        }
    }

    public class SubMatrixTools
    {
        private SubMatrix matrix;

        private int GetCountOfBlackInRow(int y)
        {
            int count = 0;
            for (int x = 0; x < matrix.Width; ++x)
            {
                if (matrix.Get(x,y)) count++;
            }
            return count;
        }

        private int GetCountOfBlackInCol(int x)
        {
            int count = 0;
            for (int y = 0; y < matrix.Height; ++y)
            {
                if (matrix.Get(x,y)) count++;
            }
            return count;
        }

        private int FindTextBegin(int avg, int[] array)
        {
            for (int i = 0; i < array.Length - 1; ++i)
            {
                if (array[i] > avg && array[i + 1] > avg)
                    return i - 15; // leave some white space so far
            }
            Console.WriteLine("WARNING: Didnt find suitable begin horizont from left with avg={0}", avg);
            return 0;
        }

        private int FindTextEnd(int avg, int[] array)
        {
            for (int i = array.Length - 1; i > 0; --i)
            {
                if (array[i] > avg && array[i - 1] > avg)
                    return i + 15; // leave some white space so far
            }
            Console.WriteLine("WARNING: Didnt find suitable begin horizont from right with avg={0}", avg);
            return 0;
        }

        /**<summary>
         * Calculate average of black pixels in rows and cols
         * if section contains more than the average - its text
         * if a few sections in a row contains and few before dont - its text begin
         * </summary>
         * <rethink>
         * How to trim white spaces around perfectly
         * <sol>having the plausible text begin corner, iterate up and left, until completely white col and row is found</sol>
         * </rethink>
         */
        public Rectangle FindTextSubmatrix()
        {
            int[] horizontalFill = new int[matrix.Width];
            int[] verticalFill = new int[matrix.Height];
            var factory = new TaskFactory();
            var tasks = new List<Task>();
            int avgH = 0;
            int avgV = 0;

            for (int x = 0; x < horizontalFill.Length; ++x)
                tasks.Add(factory.StartNew((cord) => {
                    int count = GetCountOfBlackInCol((int)cord);
                    if (count > 5)
                        avgH += horizontalFill[(int)cord] = count;
                }, x));
            for (int y = 0; y < verticalFill.Length; ++y)
                tasks.Add(factory.StartNew((cord) => {
                    int count = GetCountOfBlackInRow((int)cord);
                    if (count > 5)
                        avgV += verticalFill[(int)cord] = count;
                }, y));

            Task.WaitAll(tasks.ToArray());
            avgH /= horizontalFill.Length; avgV /= verticalFill.Length;
            avgH -= avgH / 10; avgV -= avgV / 10;
            int xb = FindTextBegin(avgH, horizontalFill), xe = FindTextEnd(avgH, horizontalFill),
                yb = FindTextBegin(avgV, verticalFill), ye = FindTextEnd(avgV, verticalFill);
            return new Rectangle
            {
                X = xb,
                Y = yb,
                Width = xe - xb,
                Height = ye - yb
            };
        }

        /// <summary>
        /// Cut white lines from all sides.
        /// </summary>
        /// <returns></returns>
        public Rectangle TrimWhiteAreaAround()
        {
            Task<int>[] tasks = new Task<int>[4];
            tasks[0] = Task<int>.Factory.StartNew(() =>
            {
                for (int x = 0; x < matrix.Width; ++x)
                {
                    for (int y = 0; y < matrix.Height; ++y)
                    {
                        if (matrix.Get(x,y))
                            return x;
                    }
                }
                return 0;
            });
            tasks[1] = Task<int>.Factory.StartNew(() =>
            {
                for (int y = 0; y < matrix.Height; ++y)
                {
                    for (int x = 0; x < matrix.Width; ++x)
                    {
                        if (matrix.Get(x, y))
                            return y;
                    }
                }
                return 0;
            });
            tasks[2] = Task<int>.Factory.StartNew(() =>
            {
                for (int x = matrix.Width - 1; x > -1; --x)
                {
                    for (int y = matrix.Height - 1; y > -1; --y)
                    {
                        if (matrix.Get(x, y))
                            return y;
                    }
                }
                return matrix.Height - 1;
            });
            tasks[3] = Task<int>.Factory.StartNew(() =>
            {
                for (int y = matrix.Height - 1; y > -1; --y)
                {
                    for (int x = matrix.Width - 1; x > -1; --x)
                    {
                        if (matrix.Get(x, y))
                            return x;
                    }
                }
                return matrix.Width - 1;
            });
            return new Rectangle
            {
                X = tasks[0].Result,
                Y = tasks[1].Result,
                Width = tasks[2].Result - tasks[1].Result,
                Height = tasks[3].Result - tasks[0].Result
            };
        }

        public SubMatrixTools(SubMatrix mat)
        {
            matrix = mat;
        }
    }
}
