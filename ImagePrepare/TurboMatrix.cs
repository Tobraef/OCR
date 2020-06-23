using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LetterReader.ImagePrepare
{
    /**<summary>Class created only for read/write/compare. The first column contains color information.</summary>
     * <remarks>Not tested properly yet.</remarks>
     */
    public class TurboMatrix
    {
        private List<List<int>> matrix;

        public List<List<int>> Matrix
        {
            get { return matrix; }
        }

        private bool WhiteApprover(bool i)
        {
            return i == false;
        }

        private bool BlackApprover(bool i)
        {
            return i == true;
        }

        private List<int> CountIntervalsinARow(ImageMatrix mat, int y)
        {
            List<int> counter = new List<int>();
            int currentCounter = 0;
            Func<bool, bool> choser;
            if (mat[0][y]) choser = BlackApprover; else choser = WhiteApprover;
            for (int x = 0; x < mat.Width; ++x)
            {
                if (choser(mat[x][y]))
                {
                    currentCounter++;
                }
                else
                {
                    if (choser == BlackApprover)
                        choser = WhiteApprover;
                    else
                        choser = BlackApprover;
                    counter.Add(currentCounter);
                    currentCounter = 1;
                }
            }
            if (currentCounter > 0)
            {
                counter.Add(currentCounter);
            }
            return counter;
        }

        private List<int> CountIntervalsinARow(SubMatrix mat, int y)
        {
            List<int> counter = new List<int>();
            int currentCounter = 0;
            Func<bool, bool> choser;
            if (mat.Get(0,y)) choser = BlackApprover; else choser = WhiteApprover;
            for (int x = 0; x < mat.Width; ++x)
            {
                if (choser(mat.Get(x,y)))
                {
                    currentCounter++;
                }
                else
                {
                    if (choser == BlackApprover)
                        choser = WhiteApprover;
                    else
                        choser = BlackApprover;
                    counter.Add(currentCounter);
                    currentCounter = 0;
                }
            }
            if (currentCounter > 0)
            {
                counter.Add(currentCounter);
            }
            return counter;
        }

        public TurboMatrix(ImageMatrix from)
        {
            if (from.Height != DATA.letterHeight || from.Width != DATA.letterWidth)
                Console.WriteLine("WARNING: create turbo matrix from non suitable ImageMatrix, not in standard size");
            matrix = new List<List<int>>();
            Task[] tasks = new Task[from.Height];
            for (int y = 0; y < from.Height; ++y)
            {
                matrix.Add(new List<int> { from[0][y] ? 1 : 0 });
                tasks[y] = Task.Factory.StartNew(
                    (cord) => matrix[(int)cord].AddRange(CountIntervalsinARow(from, (int)cord)), y);
            }
            Task.WaitAll(tasks);
        }

        public TurboMatrix(SubMatrix from)
        {
            if (from.Height != DATA.letterHeight || from.Width != DATA.letterWidth)
                Console.WriteLine("WARNING: create turbo matrix from non suitable SubMatrix, not in standard size");
            matrix = new List<List<int>>(from.Height);
            Task[] tasks = new Task[from.Height];
            for (int y = 0; y < from.Height; ++y)
            {
                matrix[y].Add(from.Get(0, y) ? 1 : 0);
                tasks[y] = Task.Factory.StartNew(
                    (cord) => matrix[(int)cord].AddRange(CountIntervalsinARow(from, (int)cord)), y);
            }
            Task.WaitAll(tasks);
        }

        public TurboMatrix(string fileName)
        {
            var lines = File.ReadLines(fileName);
            foreach (var line in lines)
            {
                matrix.Add(line.Split(' ').Select(i => int.Parse(i)).ToList());
            }
        }

        public void SaveToFile(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var row in matrix)
            {
                foreach (int val in row)
                {
                    sb.Append(val);
                    sb.Append(' ');
                }
                sb.AppendLine();
            }
            File.WriteAllText(fileName, sb.ToString());
        }

        /**<summary>Calculates difference of pixels in a row. First Pixel must be equal</summary>
         * <param name="first">Row that contains lesser number of elements</param>
         */
        private int CountDiffFirstLessStartEq(IEnumerable<int> first, IEnumerable<int> second)
        {
            first = first.Skip(1);
            second = second.Skip(1);
            int end = first.Count() - 1;
            int difference = 0;
            int partialDifS = 0;
            int partialDifF = 0;
            int i = 0;
            for (; i < end; ++i)
            {
                if (first.ElementAt(i) - partialDifF > second.ElementAt(i) - partialDifS)
                {
                    int dif = first.ElementAt(i) - second.ElementAt(i);
                    partialDifS = dif;
                    partialDifF = 0;
                    difference += dif;
                }
                else
                {
                    int dif = second.ElementAt(i) - first.ElementAt(i);
                    partialDifF = dif;
                    partialDifS = 0;
                    difference += dif;
                }
            }
            int leftOver = first.ElementAt(i);
            for (; i < second.Count(); i += 2)
            {
                leftOver -= second.ElementAt(i);
            }
            return difference + leftOver;
        }

        private int CountDiffFirstLessStartIneq(IEnumerable<int> first, IEnumerable<int> second)
        {
            //12233 _ 041122
            Console.WriteLine("RUN");
            int difference = first.ElementAt(1); // dif = 2 + 2 + 0 + 2
            first = first.Skip(2); // first = 233, 13, 3
            second = second.Skip(1); // second = 41122, 1122, 122
            int end = first.Count() - 1;
            int partialDifS = 0; // e = 2,
            int partialDifF = 0; // e = 0,
            int i = 0;
            for (; i < end; ++i)
            {
                Console.WriteLine("Difference{0}", difference);
                if (first.ElementAt(i) - partialDifF > second.ElementAt(i) - partialDifS)
                {
                    int dif = first.ElementAt(i) - partialDifF - second.ElementAt(i);
                    partialDifS = dif;
                    partialDifF = 0;
                    Console.WriteLine("First bigger, will add Dif{0}", dif);
                    difference += dif;
                }
                else
                {
                    int dif = second.ElementAt(i) - partialDifS - (first.ElementAt(i) - partialDifF);
                    partialDifF = dif;
                    partialDifS = 0;
                    Console.WriteLine("Second bigger, will add Dif{0}", dif);
                    difference += dif;
                }
            }
            int leftOver = first.ElementAt(i);
            Console.WriteLine("LeftOver {0}", leftOver);
            for (; i < second.Count(); i += 2)
            {
                Console.WriteLine("Reduction by {0}", second.ElementAt(i));
                leftOver -= second.ElementAt(i);
            }
            Console.WriteLine("Finally, different {0}", difference + leftOver);
            return difference + leftOver;
        }
        
        private int CountMissesInRow(List<int> first, List<int> second)
        {
            if (first.First() == second.First())
            {
                if (first.Count > second.Count)
                {
                    return CountDiffFirstLessStartEq(first, second);
                }
                else
                {
                    return CountDiffFirstLessStartEq(second, first);
                }
            }
            else
            {
                if (first.Count > second.Count)
                {
                    return CountDiffFirstLessStartIneq(second, first);
                }
                else
                {
                    return CountDiffFirstLessStartIneq(first, second);
                }
            }
        }

        public float Compare(TurboMatrix other)
        {
            int misses = 0;
            for (int x = 0; x < matrix.Count; ++x)
            {
                misses += CountMissesInRow(matrix[x], other.matrix[x]);
            }
            return 1 - (float)misses / DATA.letterHeight * DATA.letterWidth;
        }
    }
}
