using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace LetterReader.ImagePrepare
{
    public class ImageMatrix
    {
        private readonly bool[][] matrix;

        public bool Empty
        {
            get
            {
                return matrix == null;
            }
        }

        public char Character
        {
            get;
            set;
        }

        /// <summary>
        /// You would think, 256 * 3 / 2 equals 384 and you would be right, but nah, we need to hack, 
        /// set to 450 because MS paint paints grayish stuff instead of black
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsMediumBlack(Color c)
        {
            return c.R + c.G + c.B < 400;
        }

        public ImageMatrix(Bitmap map)
        {
            Character = '_';
            matrix = new bool[map.Width][];
            for (int x = 0; x < matrix.Length; ++x)
            {
                matrix[x] = new bool[map.Height];
            }

            for (int x = 0; x < map.Width; ++x)
            {
                for (int y = 0; y < map.Height; ++y)
                {
                    if (IsMediumBlack(map.GetPixel(x, y)))
                        matrix[x][y] = true;
                    else
                        matrix[x][y] = false;
                }
            }
        }

        public int BlacksInRow(int y)
        {
            int count = 0;
            for (int x = 0; x < Height; ++x)
            {
                if (Get(x, y))
                    count++;
            }
            return count;
        }

        public int BlacksInColumn(int x)
        {
            return matrix[x].Count(a => a);
        }

        public bool[][] Matrix
        {
            get { return matrix; }
        }

        public bool Get(int x, int y)
        {
            return matrix[x][y];
        }

        public bool[] this[int x]
        {
            get { return matrix[x]; }
        }

        public int Width
        {
            get { return matrix.Length; }
        }

        public int Height
        {
            get { return matrix[0].Length; }
        }

        public ImageMatrix CopyMatrix(Rectangle rect)
        {
            bool[][] matrix = new bool[rect.Width][];
            for (int x = 0; x < rect.Width; ++x)
            {
                matrix[x] = new bool[rect.Height];
                for (int y = 0; y < rect.Height; ++y)
                {
                    matrix[x][y] = this.matrix[x + rect.X][y + rect.Y];
                }
            }
            return new ImageMatrix(matrix);
        }

        public ImageMatrix CopyMatrix()
        {
            bool[][] matrix = new bool[Width][];
            for (int x = 0; x < Width; ++x)
            {
                matrix[x] = new bool[Height];
                for (int y = 0; y < Height; ++y)
                {
                    matrix[x][y] = this.matrix[x][y];
                }
            }
            return new ImageMatrix(matrix);
        }

        /**<summary>Safe matrix contains a white border around</summary>
         */
        public ImageMatrix CopyToSafeMatrix()
        {
            bool[][] matrix = new bool[Width + 2][];
            matrix[0] = new bool[Height + 2];
            matrix[matrix.Length - 1] = new bool[Height + 2];
            for (int x = 0, X = 1; x < Width; ++x, ++X)
            {
                matrix[X] = new bool[Height + 2];
                for (int y = 0, Y = 1; y < Height; ++y, ++Y)
                {
                    matrix[X][Y] = this.matrix[x][y];
                }
            }
            return new ImageMatrix(matrix);
        }

        public ImageMatrix(List<Point> blacks, in Rectangle rect)
        {
            Character = '_';
            bool[][] matrix = new bool[rect.Width + 1][];
            for(int x = 0; x < matrix.Length; ++x)
            {
                matrix[x] = new bool[rect.Height + 1];
            }
            for (int i = 0; i < blacks.Count; ++i)
            {
                try
                {
                    matrix[blacks[i].X - rect.X][blacks[i].Y - rect.Y] = true;
                } catch (Exception) { /* This is bullshit regarding defensive mechanism, should be handled above */ }
            }
            this.matrix = matrix;
        }

        public SubMatrix GetSubMatrix(Rectangle rect)
        {
            return new SubMatrix(matrix, rect);
        }

        public void SaveMatrixToFile(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    if (matrix[x][y])
                        sb.Append('1');
                    else
                        sb.Append('0');
                }
                sb.AppendLine();
            }
            File.WriteAllText(fileName, sb.ToString());
        }

        public ImageMatrix(string fromMatrixFile)
        {
            var text = File.ReadAllLines(fromMatrixFile);
            bool[][] image = new bool[text.First().Length][];
            for (int i = 0; i < text.First().Length; ++i)
            {
                image[i] = new bool[text.Length];
                for (int y = 0; y < text.Length; ++y)
                {
                    image[i][y] = text[y][i] == '1';
                }
            }
            matrix = image;

            Character = fromMatrixFile[fromMatrixFile.LastIndexOf(Path.DirectorySeparatorChar) + 1];
        }

        public float HWRatio
        {
            get
            {
                return (float)Height / Width;
            }
        }

        public ImageMatrix(bool[][] matrix)
        {
            Character = '_';
            this.matrix = matrix;
        }

        public ImageMatrix(char c)
        {
            Character = c;
        }
    }
}
