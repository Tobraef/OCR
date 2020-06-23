using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.ImagePrepare;
using System.Drawing;
using System.IO;

namespace LetterReader.Splitters
{
    /// <summary>
    /// Combines clearing and splitting.
    /// </summary>
    public class MatrixSplitter
    {
        private SubMatrix textMatrix;
        private List<SubMatrix> lines;

        public List<ImageMatrix> Lines
        {
            get
            {
                if (lines != null)
                {
                    return lines.Select(l => l.ToFullMatrix()).ToList();
                }
                lines = new LineSplitter().SplitToLines(textMatrix);
                return lines.Select(l => l.ToFullMatrix()).ToList();
            }
        }

        /// <summary>
        /// Fixed size letters
        /// </summary>
        public List<ImageMatrix> Letters
        {
            get
            {
                if (lines == null)
                {
                    var ls = Lines;
                }
                var letterSplitter = new LetterSplitter(textMatrix);
                return lines.SelectMany(l =>
                {
                    letterSplitter.SetNewMatrix(l);
                    return letterSplitter.GetLetters().Select(a => a == null? null : MatrixTools.Scale(a.ToFullMatrix()));
                }).ToList();
            }
        }

        /// <summary>
        /// Reads and clears the matrix.
        /// </summary>
        /// <param name="file"></param>
        public MatrixSplitter(string file)
        {
            var matrix = new ImageMatrix(new Bitmap(Image.FromFile(file)));
            var clearer = new BlurClearer(matrix);
            clearer.ClearShadows();
            textMatrix = matrix.GetSubMatrix(new MatrixTools(matrix).FindTextSubmatrix());
        }

        public MatrixSplitter(FileStream stream)
        {
            var matrix = new ImageMatrix(new Bitmap(Image.FromStream(stream)));
            var clearer = new BlurClearer(matrix);
            clearer.ClearShadows();
            textMatrix = matrix.GetSubMatrix(new MatrixTools(matrix).FindTextSubmatrix());
        }
    }
}
