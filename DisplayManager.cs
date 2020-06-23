using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LetterReader.ImagePrepare;
using System.Linq;
using System.IO;
using LetterReader.MachineLearning;

namespace LetterReader
{
    //CONSIDER
    // turbo matrix, hold begin and end, returns records directly

    public partial class DisplayManager : Form
    {
        private const string testFolder = DATA.testFolder;
        private List<Bitmap> pictures = new List<Bitmap>();
        private List<SubMatrix> letters = new List<SubMatrix>();
        private readonly List<string> files = new List<string>();
        private int index;

        private void FillTestPageContainer(string testFolder)
        {
            files.AddRange(Directory.EnumerateFiles(testFolder));
            textBoxImageFile.Text = files.First();
        }

        public DisplayManager()
        {
            InitializeComponent();
            FillTestPageContainer(testFolder);
        }

        private void FillFormWithPictures(SubMatrix matrix)
        {
            ImageCreator creator = new ImageCreator();
            pictures.Add(creator.CreateImageOutOfMatrix(matrix));
            var lines = new Splitters.LineSplitter().SplitToLines(matrix);
            pictures.AddRange(lines.Select(r => creator.CreateImageOutOfMatrix(r)));
            lines.RemoveAll(b => b.Empty);
            var lsp = new Splitters.LetterSplitter(lines.First());
            foreach (var line in lines)
            {
                lsp.SetNewMatrix(line);
                letters.AddRange(lsp.GetLetters());
            }
            pictures.AddRange(letters.Where(mat => !mat.Empty).Select(mat => creator.CreateImageOutOfMatrix(mat)));
            pictureBoxLine.Image = pictures.First();
            index = 0;
        }

        private void TestDisplay()
        {
            var folder = Path.Combine(DATA.testFolder, "Test Defense");
            var qwe = new ImageMatrix(new Bitmap(Image.FromFile(Path.Combine(folder, "qwe.png"))));
            var q = new ImageMatrix(new Bitmap(Image.FromFile(Path.Combine(folder, "q.png"))));
            var w = new ImageMatrix(new Bitmap(Image.FromFile(Path.Combine(folder, "w.png"))));
            var e = new ImageMatrix(new Bitmap(Image.FromFile(Path.Combine(folder, "e.png"))));
            qwe = qwe.CopyMatrix(new MatrixTools(qwe).TrimWhiteAreaAround());
            q = q.CopyMatrix(new MatrixTools(q).TrimWhiteAreaAround());
            w = w.CopyMatrix(new MatrixTools(w).TrimWhiteAreaAround());
            e = e.CopyMatrix(new MatrixTools(e).TrimWhiteAreaAround());
            qwe = MatrixTools.Scale(qwe, 150, 50);
            q = MatrixTools.Scale(q);
            w = MatrixTools.Scale(w);
            e = MatrixTools.Scale(e);
            var crt = new ImageCreator();
            var result = Splitters.DefenseMechanism.SplitToSingleLetters(qwe, new List<ImageMatrix> { q, w, e });
            pictures.AddRange(result.Select(a => crt.CreateImageOutOfMatrix(a)));
            pictureBoxLine.Image = pictures.First();
        }

        private void Collect()
        {
            var file = textBoxLetter.Text;
            Machine machine = new Machine();
            machine.AcknowledgeNewStyle(file, RecognitionModel.Resizing);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Collect();
            var crt = new ImageCreator();
            pictures = new Splitters.MatrixSplitter(textBoxLetter.Text).Letters.Select(l => crt.CreateImageOutOfMatrix(l)).ToList();
            pictureBoxLine.Image = pictures.First();
        }

        private void ClearPicture(ImageMatrix matrix)
        {
            var clearer = new BlurClearer(matrix);
            clearer.ClearShadows();
        }

        private SubMatrix GetOnlyTextArea(ImageMatrix matrix)
        {
            var tools = new MatrixTools(matrix);
            var textRectangle = tools.FindTextSubmatrix();
            return matrix.GetSubMatrix(textRectangle);
        }

        private void ButtonLoadImage_Click(object sender, EventArgs e)
        {
            Machine machine = new Machine();
            pictures = machine.ParseToLettersImages(textBoxImageFile.Text).ToList();
            pictureBoxLine.Image = pictures.First();
            index = 0;
        }

        private void ButtonNextImage_Click(object sender, EventArgs e)
        {
            index++;
            pictureBoxLine.Image = pictures[index];
        }

        private void ButtonNextFile_Click(object sender, EventArgs e)
        {
            int indexOfCurrent = files.IndexOf(textBoxImageFile.Text);
            if (indexOfCurrent == -1)
            {
                FillTestPageContainer(testFolder);
            }
            else
            {
                indexOfCurrent++;
                if (indexOfCurrent == files.Count)
                {
                    indexOfCurrent = 0;
                }
                textBoxImageFile.Text = files[indexOfCurrent];
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            TestSuite test = new TestSuite();
            test.TestSimplePageLineSplit();
            test.TestSimplePageLineAndLetterSplit();
            test.TestImageMatrixReadAndWrite();
            test.TestScaleDescaleComparison();
            //test.TestTurboMatrix();
            test.TestRecursiveCatch();
            test.TestSimpleLetterRecovery();
            test.TestConnectedLettersDistnignuish();
            test.TestScaling();
            test.TestSpaceRecognition();
            test.TestScaledComparison();
            test.TestVaryingSizesComparison();
            test.TestFixedSizeRecognition();
            test.TestResizingRecognition();
        }
    }
}
