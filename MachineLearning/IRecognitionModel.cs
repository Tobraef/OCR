using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.ImagePrepare;

namespace LetterReader.MachineLearning
{
    public enum RecognitionModel
    {
        FixedSize,
        Resizing
    }

    interface IRecognitionModel
    {
        /// <summary>
        /// To be performed as first action in order to acknowledge font size and style. Performs these actions on the first line only.
        /// </summary>
        /// <param name="lines">Lines of text with their letters</param>
        /// <returns>True if a style was recognized. If not recognizer will compare with monospaced</returns>
        bool AcknowledgeStyle(List<List<SubMatrix>> lines);

        /// <summary>
        /// Processes letters and returns split to lines strings of letters.
        /// </summary>
        /// <param name="lines">Matrices to insert character into.</param>
        /// <returns>Lines translated to characters.</returns>
        List<char> ProcessPage(List<List<SubMatrix>> lines);
    }
}
