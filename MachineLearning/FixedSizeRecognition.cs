using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.MachineLearning;
using LetterReader.MachineLearning.DBHandle;
using LetterReader.ImagePrepare;

namespace LetterReader.MachineLearning
{
    public class FixedSizeRecognition : IRecognitionModel
    {
        private FixedLetterRecognizer recognizer;

        private char CommaOrDot(SubMatrix matrix)
        {
            return Math.Abs(matrix.BlacksInColumn(matrix.Width / 2) - matrix.BlacksInRow(matrix.Height / 2)) < 5 ? '.' : ',';
        }

        private char PreRecognizeDotOrComma(SubMatrix matrix, int averageWidth, int averageHeight)
        {
            if (matrix.Width < averageWidth && matrix.Height < averageHeight / 2)
            {
                return CommaOrDot(matrix);
            }
            return '_';
        }

        private IEnumerable<ImageMatrix> PrepareForRecognition(IEnumerable<List<SubMatrix>> linedLetters)
        {
            var avgW = (int)linedLetters.First().Where(m => m != null).Average(m => m.Width);
            var avgH = (int)linedLetters.First().Where(m => m != null).Average(m => m.Height);
            return linedLetters.SelectMany(a =>
            {
                List<ImageMatrix> toRet = new List<ImageMatrix>();
                toRet.AddRange(a.Select(l =>
                {
                    if (l == null)
                    {
                        return null;
                    }
                    var preRecognition = PreRecognizeDotOrComma(l, avgW, avgH);
                    if (preRecognition != '_')
                    {
                        return new ImageMatrix(preRecognition);
                    }
                    return MatrixTools.Scale(l.ToFullMatrix());
                }));
                toRet.Add(new ImageMatrix('\n'));
                return toRet;
            });
        }

        public bool AcknowledgeStyle(List<List<SubMatrix>> lines)
        {
            recognizer = new FixedLetterRecognizer(DATA.monospacedFolderNode);
            return false;
        }

        public List<char> ProcessPage(List<List<SubMatrix>> lines)
        {
            var toRecognize = PrepareForRecognition(lines);
            return recognizer.MatchLetters(toRecognize);
        }
    }
}
