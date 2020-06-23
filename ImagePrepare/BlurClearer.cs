using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace LetterReader.ImagePrepare
{
    public class BlurClearer
    {
        private readonly ImageMatrix matrix;

        private bool IsBlack(int x, int y)
        {
            return matrix[x][y];
        }

        /// <summary>
        /// Clears vertical black lines that are 1px thick.
        /// </summary>
        public void ClearSingleLineBlurVertical()
        {
            List<Task> tasks = new List<Task>(matrix.Width / 2);
            Action<int> act = (begin) => {
            for (int x = begin; x < matrix.Width - 1; x += 2)
            {
                tasks.Add(Task.Factory.StartNew((xc) =>
                {
                    int xC = (int)xc;
                    for (int y = 0; y < matrix.Height; ++y)
                    {
                        if (matrix[xC - 1][y] == false && matrix[xC + 1][y] == false)
                        {
                            matrix[xC][y] = false;
                        }
                    }
                }, x));
            }};
            act(1); Task.WaitAll(tasks.ToArray()); tasks.Clear(); 
            act(2); Task.WaitAll(tasks.ToArray()); tasks.Clear();
        }

        [Obsolete]
        private void RecursiveShadowClear(int x, int y)
        {
            matrix[x][y] = false;
            if (x < matrix.Width - 1 && IsBlack(x + 1, y)) RecursiveShadowClear(x + 1, y);
            if (x > 0 && IsBlack(x - 1, y)) RecursiveShadowClear(x - 1, y);
            if (y < matrix.Height - 1 && IsBlack(x, y + 1)) RecursiveShadowClear(x, y + 1);
            if (y > 0 && IsBlack(x, y - 1)) RecursiveShadowClear(x, y - 1);
        }

        private void QueShadowClear(int xb, int yb)
        {
            Queue<Point> toClear = new Queue<Point>();
            toClear.Enqueue(new Point { X = xb, Y = yb });
            while (toClear.Count > 0)
            {
                var point = toClear.Dequeue();
                int x = point.X; int y = point.Y;
                if (matrix[x][y] == false)
                    continue;
                matrix[x][y] = false;
                if (x < matrix.Width - 1 && IsBlack(x + 1, y)) toClear.Enqueue(new Point { X = x + 1, Y = y });
                if (x > 0 && IsBlack(x - 1, y)) toClear.Enqueue(new Point { X = x - 1, Y = y });
                if (y < matrix.Height - 1 && IsBlack(x, y + 1)) toClear.Enqueue(new Point { X = x, Y = y + 1 });
                if (y > 0 && IsBlack(x, y - 1)) toClear.Enqueue(new Point { X = x, Y = y - 1});
            }
        }

        /**<summary>
         * Recursively clears black pixels from borders. If clear borders was called before, this function is useless.
         * </summary>
         */
        public void ClearShadows()
        {
            for (int x = 0; x < matrix.Width; ++x)
            {
                if (IsBlack(x, 0))
                {
                    QueShadowClear(x, 0);
                }
                if (IsBlack(x, matrix.Height - 1))
                {
                    QueShadowClear(x, matrix.Height - 1);
                }
            }
            for (int y = 0; y < matrix.Height; ++y)
            {
                if (IsBlack(0, y))
                {
                    QueShadowClear(0, y);
                }
                if (IsBlack(matrix.Width - 1, y))
                {
                    QueShadowClear(matrix.Width - 1, y);
                }
            }

        }

        /// <summary>
        /// Turn all border pixels white. Calling clear shadows after will not have any effect. 
        /// </summary>
        public void ClearBorders()
        {
            for (int x = 0; x < matrix.Width; ++x)
            {
                matrix[x][0] = matrix[x][matrix.Height - 1] = false;
            }
            for (int y = 0; y < matrix.Height; ++y)
            {
                matrix[0][y] = matrix[matrix.Width - 1][y] = false;
            }
        }

        /// <summary>
        /// Clears horizontal black lines that are 1px thick.
        /// </summary>
        public void ClearSingleLineBlurHorizontal()
        {
            List<Task> tasks = new List<Task>(matrix.Width / 2);
            Action<int> act = (begin) => {
                for (int y = begin; y < matrix.Height - 1; y += 2)
                {
                    tasks.Add(Task.Factory.StartNew((yc) =>
                    {
                        int yC = (int)yc;
                        for (int x = 0; x < matrix.Width; ++x)
                        {
                            if (matrix[x][yC + 1] == false && matrix[x][yC - 1] == false)
                            {
                                matrix[x][yC] = false;
                            }
                        }
                    }, y));
                }
            };
            act(1); Task.WaitAll(tasks.ToArray()); tasks.Clear();
            act(2); Task.WaitAll(tasks.ToArray()); tasks.Clear();
        }

        private bool IsSurroundedByBlack(int x, int y)
        {
            try
            {
                int surrounded = 0;
                if (IsBlack(x + 1, y)) surrounded++;
                if (IsBlack(x - 1, y)) surrounded++;
                if (IsBlack(x, y + 1)) surrounded++;
                if (IsBlack(x, y - 1)) surrounded++;
                return surrounded < 2;
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("IsSurroundedByBlack: Out of range: x = {0}, y = {1}", x, y);
                return true;
            }
        }

        [Obsolete]
        public void ClearStandAlonePixels()
        {

        }

        public BlurClearer(ImageMatrix matrix_)
        {
            matrix = matrix_;
        }
    }
}
