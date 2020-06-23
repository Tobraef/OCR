using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LetterReader.ImagePrepare
{
    public class SubMatrix
    {
        private bool[][] matrix;
        private Rectangle rectangle;

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

        public bool Empty
        {
            get
            {
                return matrix == null;
            }
        }

        public bool Get(int x, int y)
        {
            return matrix[x + rectangle.X][y + rectangle.Y];
        }

        public bool[][] Matrix
        {
            get { return matrix; }
        }

        public int Width
        {
            get { return rectangle.Width; }
        }

        public int Height
        {
            get { return rectangle.Height; }
        }

        [Obsolete("Doesnt work as intended, errors")]
        public int xBegin
        {
            get { return rectangle.X; }
        }

        [Obsolete("Doesnt work as intended, errors")]
        public int yBegin
        {
            get { return rectangle.Y; }
        }

        public int xEnd
        {
            get { return rectangle.X + rectangle.Width; }
        }

        public int yEnd
        {
            get { return rectangle.Y + rectangle.Height; }
        }

        public SubMatrix GetSubMatrix(Rectangle rect)
        {
            return new SubMatrix(matrix, new Rectangle
            {
                X=rectangle.X + rect.X,
                Y=rectangle.Y + rect.Y,
                Height=rect.Height,
                Width=rect.Width
            });
        }

        public ImageMatrix ToFullMatrix()
        {
            bool[][] mat = new bool[Width][];
            for (int x = 0; x < Width; ++x)
            {
                mat[x] = new bool[Height];
                for (int y = 0; y < Height; ++y)
                {
                    mat[x][y] = Get(x, y);
                }
            }
            return new ImageMatrix(mat);
        }

        public SubMatrix(bool[][] matrix_, Rectangle rectangle_)
        {
            matrix = matrix_;
            rectangle = rectangle_;
            if(rectangle.X + rectangle.Width > matrix.Length)
            {
                rectangle.Width = matrix.Length - rectangle.X;
            }
            if (rectangle.Y + rectangle.Height > matrix[0].Length)
            {
                rectangle.Height = matrix[0].Length - rectangle.Y;
            }
        }
    }
}
