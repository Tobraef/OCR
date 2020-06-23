using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.ImagePrepare;
using LetterReader.MachineLearning;
using System.IO;

namespace LetterReader.MachineLearning
{
    public class ScalingRecognition : IRecognitionModel
    {
        private VaryingLetterRecognizer recognizer_;
        private string font_ = "monospaced"; // please fix me

        /// <summary>
        /// Ratio for height to width + height
        /// </summary>
        /// <param name="file">Matrix file</param>
        /// <returns>HeightWidth ratio + height</returns>
        private Tuple<float, float> GetMatrixInfo(string file)
        {
            var lines = File.ReadAllLines(file);
            return new Tuple<float, float>((float)lines.Length / lines.First().Length, lines.Length);
        }

        private static bool InRangeFrom(float x, float y, float range)
        {
            if (x > y)
                return x - y < range;
            return y - x < range;
        }

        /// <summary>
        /// Checks required resize scaling for upcoming letters basing on the capital letter.
        /// </summary>
        /// <param name="capital">Capital letter to be checked.</param>
        /// <returns>Scaling ratio for all upcoming letters</returns>
        private float ResizeScale(ImageMatrix capital)
        {
            var files = DBHandle.FileRetriever.Size_All_Type_All(DATA.varyingSizeFolderNode, DATA.capitalsFolderNode);
            float heightWidthRatio = (float)capital.Height / capital.Width;
            float toRet = 0;
            float bestAlikeRatio = 0;
            foreach (var fileD in files)
            {
                foreach (var file in fileD.Value)
                {
                    var matrixInfo = GetMatrixInfo(file);
                    var matrixHWRatio = matrixInfo.Item1;
                    if (InRangeFrom(matrixHWRatio, heightWidthRatio, 0.1f))
                    {
                        var alikeRatio = MatrixTools.EqualPixelRatioScaleBased(new ImageMatrix(file), capital);
                        if (alikeRatio > bestAlikeRatio)
                        {
                            bestAlikeRatio = alikeRatio;
                            toRet = matrixInfo.Item2 / capital.Height;
                        }
                    }
                }
            }
            return toRet;
        }

        public bool AcknowledgeStyle(List<List<SubMatrix>> lines)
        {
            var shouldBeCapital = lines.First().First().ToFullMatrix();
            var scale = ResizeScale(shouldBeCapital);
            Console.WriteLine($"Resize scale received: {scale}");
            recognizer_ = new VaryingLetterRecognizer(font_, 1/scale);
            return true;
        }

        public List<char> ProcessPage(List<List<SubMatrix>> lines)
        {
            if (recognizer_ == null)
            {
                recognizer_ = new VaryingLetterRecognizer(font_, ResizeScale(lines.First().First().ToFullMatrix()));
            }
            List<char> toRet = new List<char>();
            foreach (var line in lines)
            {
                toRet.AddRange(recognizer_.MatchLetters(line.Select(m => m == null ? null : m.ToFullMatrix())));
                toRet.Add('\n');
            }
            return toRet;
        }
    }
}
