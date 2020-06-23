using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.ImagePrepare;
using LetterReader.MachineLearning.DBHandle;

namespace LetterReader.MachineLearning
{
    class VaryingLetterRecognizer
    {
        private List<ImageMatrix> capitals;
        private List<ImageMatrix> normals;

        private bool EgligableForRecognition(ImageMatrix letter, ImageMatrix toCompare)
        {
            //Could make a decision based on the % compare, not scalar compare, but nah
            //Will cause problem on big letters, as none will meet the conditions or on too small letters, where all will meet the condition
            //But nah, a few PC ticks spared
            return Math.Abs(letter.HWRatio - toCompare.HWRatio) < 0.1;
        }

        private char CommaOrDot(ImageMatrix matrix)
        {
            return Math.Abs(matrix.BlacksInColumn(matrix.Width / 2) - matrix.BlacksInRow(matrix.Height / 2)) < 5 ? '.' : ',';
        }

        private char MatchLetter(ImageMatrix letter, IEnumerable<ImageMatrix> matchers)
        {
            char theOne = '_';
            float theRatio = 0;
            foreach (var m in matchers)
            {
                if (EgligableForRecognition(letter, m))
                {
                    float ratio = MatrixTools.EqualPixelRatioVaryingSize(m, letter);
                    if (ratio > theRatio)
                    {
                        theRatio = ratio;
                        theOne = m.Character;
                    }
                }
            }
            if (theRatio == 0)
            {
                return ',';//CommaOrDot(letter);
            }
            return theOne;
        }

        public List<char> MatchLetters(IEnumerable<ImageMatrix> letters)
        {
            char lastLetter = '_';
            List<char> chars = new List<char>();
            foreach (var letter in letters)
            {
                if (letter == null)
                {
                    chars.Add(' ');
                }
                else if (letter.Character == '\n')
                {
                    chars.Add('\n');
                }
                else if (lastLetter == '.' || chars.Count == 0)
                {
                    chars.Add(MatchLetter(letter, capitals));
                    lastLetter = '_';
                }
                else
                {
                    lastLetter = MatchLetter(letter, normals);
                    chars.Add(lastLetter);
                }
            }
            return chars;
        }

        /// <summary>
        /// Will compare upcoming letters to those stored in given font folder + will scale letters in those folder by given ratio
        /// </summary>
        /// <param name="font"></param>
        /// <param name="scalingRatio"></param>
        public VaryingLetterRecognizer(string font, float scalingRatio)
        {
            capitals = DataRetriever.Size_Font_Type_All(DATA.varyingSizeFolderNode, font, DATA.capitalsFolderNode)
                .Select(m => MatrixTools.Scale(m, scalingRatio))
                .ToList();
            normals = DataRetriever.Size_Font_Type_All(DATA.varyingSizeFolderNode, font, DATA.normalsFolderNode)
                .Select(m => MatrixTools.Scale(m, scalingRatio))
                .ToList();
        }
    }
}
