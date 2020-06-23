using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.ImagePrepare;
using LetterReader.Splitters;
using System.Drawing;
using System.IO;

namespace LetterReader
{
    public class TestSuite
    {
        private const string simplePageUrl = @"C:\Users\mprzemys\Desktop\Test pages\Bez nazwy.png";
        private string testName;
        private ImageMatrix workMatrix;
        private SubMatrix matrix;

        private List<List<SubMatrix>> GetLetters(List<SubMatrix> lines)
        {
            LetterSplitter splitter = new LetterSplitter(lines.First());
            var firstLine = splitter.GetLetters();
            lines.RemoveAt(0);
            var letters = lines.Select(l => { splitter.SetNewMatrix(l); return splitter.GetLetters(); }).ToList();
            letters.Insert(0, firstLine);
            letters.ForEach(l => l.RemoveAll(r => r == null));
            return letters;
        }

        private List<SubMatrix> GetLines()
        {
            LineSplitter lineSplitter = new LineSplitter();
            return lineSplitter.SplitToLines(matrix);
        }

        private void PrepareImage()
        {
            workMatrix = new ImageMatrix(new Bitmap(Image.FromFile(simplePageUrl)));
            BlurClearer clearer = new BlurClearer(workMatrix);
            clearer.ClearShadows();
            MatrixTools tools = new MatrixTools(workMatrix);
            matrix = workMatrix.GetSubMatrix(tools.FindTextSubmatrix());
        }

        public void TestSimplePageLineSplit()
        {
            try
            {
                PrepareImage();
                var lines = GetLines();
                if (lines.Count != 5)
                    Console.WriteLine("TEST_LineSplit:Lines count not equal 5, actual: " + lines.Count);
                else
                    Console.WriteLine("TEST_LineSplit: PASSED");
            } catch (Exception e) { Console.WriteLine("TEST_LineSplit: EXCEPTION " + e.Message); }
        }

        public void TestSimplePageLineAndLetterSplit()
        {
            PrepareImage();
            var lines = GetLines();
            var letters = GetLetters(lines);
            string fails = "Failed lines:";
            if (letters[0].Count != 17)
                fails += " line 0, letters found: " + letters[0].Count;
            if (letters[1].Count != 17)
                fails += " line 1, letters found: " + letters[1].Count;
            if (letters[2].Count != 19)
                fails += " line 2, letters found: " + letters[2].Count;
            if (letters[3].Count != 15)
                fails += " line 3, letters found: " + letters[3].Count;
            if (letters[4].Count != 6)
                fails += " line 4, letters found: " + letters[4].Count;
            if (fails.Length == 13)
                Console.WriteLine("TEST_LetterSplit: PASSED");
            else
                Console.WriteLine("TEST_LetterSplit: " + fails);
        }

        public void TestImageMatrixReadAndWrite()
        {
            PrepareImage();
            ImageCreator creator = new ImageCreator();
            string matrixFile = simplePageUrl.Replace(".png", "MAT.txt");
            matrix.ToFullMatrix().SaveMatrixToFile(matrixFile);
            var mat = new ImageMatrix(matrixFile);
            float ratio = MatrixTools.EqualPixelRatio(matrix, mat);
            if (ratio > 0.995)
                Console.WriteLine("TEST_MatrixR/W: PASSED, ratio: " + ratio.ToString());
            else
                Console.WriteLine("TEST_MatrixR/W: FAIL, compare ratio: " + ratio.ToString());
            matrixFile = matrixFile.Replace(".txt", ".png");
            creator.CreateImageOutOfMatrix(mat).Save(matrixFile);
        }

        public void CreateCutImageMatrix(ImageMatrix matrix)
        {
            List<ImageMatrix> matrixes = new List<ImageMatrix>();
            Rectangle rect = new Rectangle
            {
                X = 0,
                Y = 0,
                Width = matrix.Width - 1,
                Height = matrix.Height / 120
            };
            for (int i = 0; i < 100; ++i)
            {
                matrixes.Add(matrix.CopyMatrix(rect));
                rect.Y += matrix.Height / 100;
            }
            long count = 0;

            foreach (var mat in matrixes)
            {
                for (int x = 0; x < mat.Width; ++x)
                {
                    for (int y = 0; y < mat.Height; ++y)
                    {
                        if (mat[x][y])
                            count++;
                    }
                }
            }
        }

        public void CreateCopyImageMatrix(ImageMatrix matrix)
        {
            List<SubMatrix> matrixes = new List<SubMatrix>();
            Rectangle rect = new Rectangle
            {
                X = 0,
                Y = 0,
                Width = matrix.Width - 1,
                Height = matrix.Height / 120
            };
            for (int i = 0; i < 100; ++i)
            {
                matrixes.Add(matrix.GetSubMatrix(rect));
                rect.Y += matrix.Height / 100;
            }
            long count = 0;

            foreach (var mat in matrixes)
            {
                for (int x = 0; x < mat.Width; ++x)
                {
                    for (int y = 0; y < mat.Height; ++y)
                    {
                        if (mat.Get(x,y))
                            count++;
                    }
                }
            }
        }

        public void IterateOver(ImageMatrix matrix)
        {
            long count = 0;
            for (int x = 0; x < matrix.Width; ++x)
            {
                for (int y = 0; y < matrix.Height; ++y)
                {
                    if (matrix.Get(x,y))
                    {
                        count++;
                    }
                }
            }
        }

        public void IterateOver(SubMatrix matrix)
        {
            long count = 0;
            for (int x = 0; x < matrix.Width; ++x)
            {
                for (int y = 0; y < matrix.Height; ++y)
                {
                    if (matrix.Get(x, y))
                    {
                        count++;
                    }
                }
            }
        }

        public void Benchmark(Action function, string name, int times = 1)
        {
            GC.Collect();
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            for (int i = 0; i < times; ++i) function();
            watch.Stop();
            Console.WriteLine("TEST_" + name + ": took " + watch.ElapsedMilliseconds.ToString() + " to run it " + times.ToString() + " times");
        }

        public void TestScaleDescaleComparison()
        {
            int width = 100;
            int height = 100;
            bool[][] mat = new bool[width][];
            var rEngine = new Random();
            for (int x = 0; x < mat.Length; ++x)
            {
                mat[x] = new bool[height];
                for (int y = 0; y < mat[x].Length; ++y)
                {
                    mat[x][y] = rEngine.Next(2) == 1 ? true : false;
                }
            }
            ImageMatrix matrix = new ImageMatrix(mat);
            var creator = new ImageCreator();
            var scaled = MatrixTools.Scale(matrix, 50, 50);
            var descaled = MatrixTools.Scale(scaled, 100, 100);
            float s = MatrixTools.EqualPixelRatio(descaled, matrix);
            if (s > 0.5)
                Console.WriteLine("TEST_Scaling: Passed, scale equality: " + s.ToString());
            else
                Console.WriteLine("TEST_Scaling: fail, scale equality: " + s.ToString());
        }
         
        public void TestTurboMatrix()
        {
            ImageMatrix matrix = new ImageMatrix(new bool[10][]
            {
                new bool[3]{ false, false, false },
                new bool[3]{ false, false, false },
                new bool[3]{ true, true, true },
                new bool[3]{ true, true, true },
                new bool[3]{ false, false, false },
                new bool[3]{ false, false, false },
                new bool[3]{ false, false, false },
                new bool[3]{ true, true, true },
                new bool[3]{ true, true, true },
                new bool[3]{ true, true, true }
            });
            var turbo = new TurboMatrix(matrix);
            foreach (var row in turbo.Matrix)
            {
                if (row[0] != 0)
                    Console.WriteLine("TEST_TurboMatrix: First pixel match fail");
                if (row[1] != 2)
                    Console.WriteLine("TEST_TurboMatrix: Wrong number of correct pixels");
                if (row[2] != 2)
                    Console.WriteLine("TEST_TurboMatrix: Wrong number of correct pixels");
                if (row[3] != 3)
                    Console.WriteLine("TEST_TurboMatrix: Wrong number of correct pixels");
                if (row[4] != 3)
                    Console.WriteLine("TEST_TurboMatrix: Wrong number of correct pixels");
            }

            var sameTurbo = new TurboMatrix(matrix);
            float equality = turbo.Compare(sameTurbo) * DATA.letterHeight * DATA.letterWidth / 30;
            if (equality < 0.95)
                Console.WriteLine("TEST_TurboMatrix: Same turbo matrix comparison failed. Equality: " + equality.ToString());
            //12233 _ 041122
            var otherMatrix = new ImageMatrix(new bool[10][]
            {
                new bool[3]{ true, true, true },
                new bool[3]{ true, true, true },
                new bool[3]{ true, true, true },
                new bool[3]{ true, true, true },
                new bool[3]{ false, false, false }, 
                new bool[3]{ true, true, true },
                new bool[3]{ false, false, false },
                new bool[3]{ false, false, false }, 
                new bool[3]{ true, true, true }, 
                new bool[3]{ true, true, true } 
            });
            equality = turbo.Compare(new TurboMatrix(otherMatrix));
            Console.WriteLine("Equality: {0}", equality.ToString()); 
            equality /= (float)DATA.letterHeight * DATA.letterWidth / 30;
            if (equality > 0.6 || equality < 0.4)
            {
                //Console.WriteLine("TEST_TurboMatrix: Expected eq of different matrixes 0.5, received: {0}", equality.ToString());
            }
        }

        public void TestRecursiveCatch()
        {
            var testMatrix = new ImageMatrix(new Bitmap(Image.FromFile(Path.Combine(DATA.testFolder, "DefenseTest1png.png"))));
            var matrices = DefenseMechanism.RecursiveDistinguish(testMatrix);
            if (matrices.Count != 5)
            {
                Console.WriteLine("TEST_RecursiveCatch: Count of found matrices is incorrect, received count: " + matrices.Count.ToString());
                ImageCreator crt = new ImageCreator();
                crt.CreateImageOutOfMatrix(testMatrix).Save(Path.Combine(DATA.testFolder, "TestRecursive.png"));
                int i = 0;
                foreach (var m in matrices)
                {
                    crt.CreateImageOutOfMatrix(m).Save(Path.Combine(DATA.testFolder, "TestRecursive" + i++.ToString() + ".png"));
                }
            }
            else
                Console.WriteLine("TEST_RecursiveCatch: Passed");
        }

        public static void Log(string text)
        {
            Console.WriteLine(text);
        }

        public static void LogPass(string testName)
        {
            Console.WriteLine("TEST_" + testName + ": PASSED");
        }

        public static void LogFail(string testName, string reason)
        {
            Console.WriteLine("TEST_" + testName + ": FAIL " + reason);
        }

        private void ClassicalRoutine()
        {
            MachineLearning.Machine machine = new MachineLearning.Machine();
            var letters = machine.ParseToLettersImages(Path.Combine(DATA.testFolder, @"Bez nazwy.png"));
            // Last line still catches 2 spaces
            if (letters.Count() != 21 + 20 + 22 + 18 + 8)
            {
                Console.WriteLine("TEST_Routine: Failed, received images: {0}, expected: {1}", letters.Count(), (21 + 20 + 22 + 18 + 8));
            }
            else
            {
                Log("TEST_Routine: Passed");
            }
        }

        public void TestSimpleLetterRecovery()
        {
            Benchmark(ClassicalRoutine, "Routine", 1);
        }

        private void TEST(bool result, string failure = "")
        {
            if (result)
            {
                LogPass(testName);
            }
            else
            {
                LogFail(testName, failure);
            }
        }

        public void TestConnectedLettersDistnignuish()
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
            var result = DefenseMechanism.SplitToSingleLetters(qwe, new List<ImageMatrix> { q, w, e });
            if (result.Count != 3)
            {
                LogFail("Connected letters distinguish", "Received: " + result.Count.ToString() + " Expected: 3");
            }
            else
            {
                LogPass("Connected letters distinguish");
            }
        }

        public void TestScaling()
        {
            testName = "Scaling";
            var matrix = new ImageMatrix(new Bitmap(Image.FromFile(simplePageUrl)));
            {
                matrix = MatrixTools.Scale(matrix);
                TEST(matrix.Width == DATA.letterWidth, $"Actual {matrix.Width}");
                TEST(matrix.Height == DATA.letterHeight, $"Actual {matrix.Height}");
            }
            {
                matrix = MatrixTools.Scale(matrix, 1.5f);
                TEST(matrix.Width == 60, $"Actual {matrix.Width}");
                TEST(matrix.Height == 75, $"Actual {matrix.Height}");
            }
            {
                matrix = MatrixTools.Scale(matrix, 100, 100);
                TEST(matrix.Width == 100, $"Actual {matrix.Width}");
                TEST(matrix.Height == 100, $"Actual {matrix.Height}");
            }
            {
                matrix = MatrixTools.Scale(matrix, 60, 30);
                TEST(matrix.Width == 60, $"Actual {matrix.Width}");
                TEST(matrix.Height == 30, $"Actual {matrix.Height}");
            }
            matrix = new ImageMatrix(new bool[][] {
                new bool[]{ true, true, true, true },
                new bool[]{ true, true, true, true },
                new bool[]{ true, true, true, true },
                new bool[]{ true, true, true, true },
            });
            {
                matrix = MatrixTools.Scale(matrix);
                TEST(matrix.Matrix.SelectMany(a => a).All(b => b), "Not all pixels are black");
                matrix = MatrixTools.Scale(matrix, 200, 40);
                TEST(matrix.Matrix.SelectMany(a => a).All(b => b), "Not all pixels are black");
            }
        }

        public void TestSpaceRecognition()
        {
            PrepareImage();
            testName = "SpaceRecognition";
            var firstLine = GetLines().First();
            var letterSplitter = new LetterSplitter(firstLine);
            var letters = letterSplitter.GetLetters();
            TEST(letters.Where(m => m == null).Count() == 3, $"Didnt find 3 required spaces, found {letters.Where(m => m == null).Count()}");
        }

        public void TestScaledComparison()
        {
            testName = "ScaledComparison";
            Random r = new Random();
            int smallW = 5, smallH = 7;
            bool[][] smallMatrix = new bool[smallW][];
            for (int x = 0; x < smallW; ++x)
            {
                smallMatrix[x] = new bool[smallH];
                for (int y = 0; y < smallH; ++y)
                {
                    smallMatrix[x][y] = r.Next(2) == 1 ? true : false;
                }
            }
            ImageMatrix main = new ImageMatrix(smallMatrix);
            ImageMatrix second = MatrixTools.Scale(main, 8, 16);
            var r1 = MatrixTools.EqualPixelRatioScaleBased(main, second);

            TEST(r1 >= 0.99 && r1 <= 1.0, $"Comparison varying sizes is below 0.99, received ratio: {r1}");
        }

        public void TestVaryingSizesComparison()
        {
            testName = "VaryingSizeComparison";
            Random r = new Random();
            int smallW = 5, smallH = 7;
            bool[][] smallMatrix = new bool[smallW][];
            for (int x = 0; x < smallW; ++x)
            {
                smallMatrix[x] = new bool[smallH];
                for (int y = 0; y < smallH; ++y)
                {
                    smallMatrix[x][y] = r.Next(2) == 1 ? true : false;
                }
            }
            ImageMatrix main = new ImageMatrix(smallMatrix);
            ImageMatrix second = MatrixTools.Scale(main, 10, 6);
            var r2 = MatrixTools.EqualPixelRatioVaryingSize(main, second);
            TEST(r2 >= 0.99 && r2 <= 1.0, $"Comparison varying sizes is below 0.99, received ratio: {r2}");
        }

        public void TestFixedSizeRecognition()
        {
            testName = "FixedSizeRecognition";
            MachineLearning.Machine machine = new MachineLearning.Machine();
            var letters = machine.ProcessBook(new List<string> { DATA.standardTestFile }, MachineLearning.RecognitionModel.FixedSize);
            Console.Write(letters.Aggregate("", (t, c) => t + c));
            TEST(true, "");
        }

        public void TestResizingRecognition()
        {
            testName = "ResizingRecognition";
            MachineLearning.Machine machine = new MachineLearning.Machine();
            var letters = machine.ProcessBook(new List<string> { DATA.standardTestFile }, MachineLearning.RecognitionModel.Resizing);
            Console.Write(letters.Aggregate("", (t, c) => t + c));
            TEST(true, "");
        }
    }
}
