using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.ImagePrepare;
using LetterReader.Splitters;
using LetterReader.MachineLearning.DBHandle;
using System.Drawing;
using System.IO;

namespace LetterReader.MachineLearning
{
    /// <summary>
    /// TODOS:
    /// Scaling recognition is not working good, matching is terrible
    /// New style recognizing based on monospaced - Not implemented.
    /// UltraMatrix with precalculated black counts in rows and in columns.
    /// NOTE:
    /// fixed size recognition is quite OK
    /// </summary>
    public class Machine
    {
        IRecognitionModel recognitionModel;

        #region
        private SubMatrix GetClearedTextMatrix(ImageMatrix matrix)
        {
            var clearer = new BlurClearer(matrix);
            clearer.ClearShadows();
            //overkill
            //clearer.ClearSingleLineBlurHorizontal();
            //clearer.ClearSingleLineBlurVertical();
            return matrix.GetSubMatrix(new MatrixTools(matrix).FindTextSubmatrix());
        }

        private List<SubMatrix> GetLines(SubMatrix textMatrix)
        {
            return new LineSplitter().SplitToLines(textMatrix);
        }

        private List<SubMatrix> GetLetters(SubMatrix line, LetterSplitter letterSplitter)
        {
            letterSplitter.SetNewMatrix(line);
            var letters = letterSplitter.GetLetters();
            //letters.Insert(0, line);
            return letters;
        }

        /// <summary>
        /// Clear, split to lines, split to letters.
        /// </summary>
        /// <param name="mainMatrix">Main image</param>
        /// <returns>Lines with letters, first letter is always the line</returns>
        private List<List<SubMatrix>> FindLettersToLines(ImageMatrix mainMatrix)
        {
            var matrix = GetClearedTextMatrix(mainMatrix);
            var lines = GetLines(matrix);
            var letterSplitter = new LetterSplitter(matrix);
            var resultLinesAndLetters = lines.Select(l => GetLetters(l, letterSplitter)).ToList();
            return resultLinesAndLetters;
        }
        private void SetRecognitionModel(RecognitionModel model)
        {
            switch (model)
            {
                case RecognitionModel.FixedSize: recognitionModel = new FixedSizeRecognition(); break;
                case RecognitionModel.Resizing: recognitionModel = new ScalingRecognition(); break;
            }
        }

        private List<char> ProcessPage(string file)
        {
            var mainMatrix = new ImageMatrix(new Bitmap(Image.FromFile(file)));
            var lines = FindLettersToLines(mainMatrix);
            return recognitionModel.ProcessPage(lines);
        }

        private void SaveMatricesToDirectory(string directory, IEnumerable<ImageMatrix> matrices)
        {
            var placeForNormals = Path.Combine(directory, DATA.normalsFolderNode);
            var placeForCapitals = Path.Combine(directory, DATA.capitalsFolderNode);
            foreach (var matrix in matrices)
            {
                if (char.IsUpper(matrix.Character))
                {
                    matrix.SaveMatrixToFile(Path.Combine(placeForCapitals, matrix.Character.ToString()));
                }
                else
                {
                    string append;
                    if (matrix.Character == '.')
                        append = "dot";
                    else if (matrix.Character == ',')
                        append = "coma";
                    else
                        append = matrix.Character.ToString();
                    matrix.SaveMatrixToFile(Path.Combine(placeForNormals, append));
                }
            }
        }

        private char NextChar(int i)
        {
            if (i >= 26)
            {
                i -= 26;
                return (char)('a' + i);
            }
            return (char)('A' + i);
        }

        #endregion private methods

        /// <summary>
        /// Retrieves letters ignoring spaces, so outcome is a line of text.
        /// </summary>
        /// <param name="pathToImage"></param>
        /// <returns></returns>
        public IEnumerable<Bitmap> ParseToLettersImages(string pathToImage)
        {
            var mainMatrix = new ImageMatrix(new Bitmap(Image.FromFile(pathToImage)));
            var clr = GetClearedTextMatrix(mainMatrix);
            var lines = GetLines(clr);
            LetterSplitter splitter = new LetterSplitter(clr);
            var letters = lines.SelectMany(l => {
                splitter.SetNewMatrix(l);
                return splitter.GetLetters();
            });
            List<SubMatrix> summarize = new List<SubMatrix> { clr };
            summarize.AddRange(lines);
            summarize.AddRange(letters);
            var crt = new ImageCreator();
            return summarize.Select(a => crt.CreateImageOutOfMatrix(a));
        }

        /// <summary>
        /// Main recognition tool. Uses first page as a sample for building recognition mechanism.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public List<char> ProcessBook(List<string> files, RecognitionModel model)
        {
            var sample = files.First();
            var mainMatrix = new ImageMatrix(new Bitmap(Image.FromFile(sample)));
            var sampleLetters = FindLettersToLines(mainMatrix);
            SetRecognitionModel(model);
            recognitionModel.AcknowledgeStyle(sampleLetters);

            List<char> toRet = new List<char>();
            toRet.AddRange(recognitionModel.ProcessPage(sampleLetters));
            foreach (var file in files.Skip(1))
            {
                toRet.AddRange(ProcessPage(file));
            }
            return toRet;
        }

        /// <summary>
        /// Take letters saved as grapic files and add them to the database. One image per letter.
        /// </summary>
        /// <param name="directory">Directory with images. Should be named as the font is meant to be.</param>
        public void ReadLettersFromDirectory(string directory, RecognitionModel model)
        {
            var files = Directory.GetFiles(directory);
            var matrices = files.Select(f =>
                {
                    var m = new ImageMatrix(new Bitmap(Image.FromFile(f)));
                    var fileName = f.Substring(f.LastIndexOf(Path.DirectorySeparatorChar));
                    if (fileName.Contains("dot"))
                    {
                        m.Character = '.';
                    }
                    else if (fileName.Contains("coma"))
                    {
                        m.Character = ',';
                    }
                    else
                    {
                        m.Character = fileName.First();
                    }
                    return MatrixTools.Scale(m.CopyMatrix(new MatrixTools(m).TrimWhiteAreaAround()));
                });
            var placeInDB = DataCollector.GenerateFontFolder(directory, model);
            SaveMatricesToDirectory(placeInDB, matrices);
        }

        /// <summary>
        /// Learn new style based on a clear page with separated letters from whole alphabet. Letters as on keyboard.
        /// Last 2 letters are dot and comma
        /// </summary>
        /// <param name="file">Path to file with letters.</param>
        public void AcknowledgeNewStyle(string file, RecognitionModel model)
        {
            List<ImageMatrix> letters = null;
            if (model == RecognitionModel.FixedSize)
            {
                letters = new MatrixSplitter(file).Letters;
            }
            else if (model == RecognitionModel.Resizing)
            {
                ImageMatrix main = new ImageMatrix(new Bitmap(Image.FromFile(file)));
                SubMatrix sub = new SubMatrix(main.Matrix, new Rectangle { X = 0, Y = 0, Height = main.Height, Width = main.Width });
                var letterSplitter = new LetterSplitter(sub);
                letters = GetLines(sub).SelectMany(line => GetLetters(line, letterSplitter)).Select(s => s.ToFullMatrix()).ToList();
            }
            var fileName = file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            var woExtension = fileName.Substring(0, fileName.IndexOf('.'));
            var placeInDB = DataCollector.GenerateFontFolder(woExtension, model);
            int i = 0;
            foreach (var l in letters.Take(letters.Count - 2))
            {
                l.Character = NextChar(i++);
            }
            SaveMatricesToDirectory(placeInDB, letters);
        }
    }
}
