using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace LetterReader.ImagePrepare
{
    public class ImageCreator
    {
        public ImageCreator RevertMatrixColor(ImageMatrix matrix)
        {
            for (int x = 0; x < matrix.Width; ++x)
            {
                for (int y = 0; y < matrix.Height; ++y)
                {
                    matrix[x][y] = !matrix[x][y];
                }
            }
            return this;
        }

        private Bitmap CreateSpace()
        {
            Bitmap map = new Bitmap(40, 50);
            for (int x = 0; x < map.Width; ++x)
            {
                for (int y = 0; y < map.Height; ++y)
                {
                    map.SetPixel(x, y, Color.White);
                }
            }
            return map;
        }

        public Bitmap CreateImageOutOfMatrix(ImageMatrix matrix)
        {
            if (matrix == null)
            {
                return CreateSpace();
            }
            Bitmap map = new Bitmap(matrix.Width, matrix.Height);
            for (int x = 0; x < matrix.Width; ++x)
            {
                for (int y = 0; y < matrix.Height; ++y)
                {
                    map.SetPixel(x, y, matrix[x][y] ? Color.Black : Color.White);
                }
            }
            return map;
        }

        public Bitmap CreateImageOutOfMatrix(SubMatrix matrix)
        {
            if (matrix == null)
            {
                return CreateSpace();
            }
            Bitmap map = new Bitmap(matrix.Width, matrix.Height);
            for (int x = 0; x < matrix.Width; ++x)
            {
                for (int y = 0; y < matrix.Height; ++y)
                {
                    map.SetPixel(x, y, matrix.Get(x,y) ? Color.Black : Color.White);
                }
            }
            return map;
        }

        public ImageCreator()
        {
        }
    }
}
