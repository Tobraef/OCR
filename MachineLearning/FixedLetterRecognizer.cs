using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.ImagePrepare;
using LetterReader.MachineLearning.DBHandle;

namespace LetterReader.MachineLearning
{
    public class FixedLetterRecognizer
    {
        private IEnumerable<ImageMatrix> capitals;
        private IEnumerable<ImageMatrix> normals;

        private char MatchLetter(ImageMatrix letter, IEnumerable<ImageMatrix> matchers)
        {
            char theOne = '_';
            float theRatio = 0;
            foreach (var m in matchers)
            {
                float ratio = MatrixTools.EqualPixelRatio(m, letter);
                if (ratio > theRatio)
                {
                    theRatio = ratio;
                    theOne = m.Character;
                }
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
                // dot etc. shouldnt be taken for matching in the first place
                else if (letter.Character == '\n' || letter.Character == ',' || letter.Character == '.')
                {
                    chars.Add(letter.Character);
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
        /// Will compare upcoming letters to those stored in given font folder
        /// </summary>
        /// <param name="font"></param>
        public FixedLetterRecognizer(string font)
        {
            capitals = DataRetriever.Size_Font_Type_All(DATA.fixedSizeFolderNode, font, DATA.capitalsFolderNode);
            normals = DataRetriever.Size_Font_Type_All(DATA.fixedSizeFolderNode, font, DATA.normalsFolderNode);
        }
    }
}
