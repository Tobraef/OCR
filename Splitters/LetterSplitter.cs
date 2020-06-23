using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.ImagePrepare;
using System.Drawing;

namespace LetterReader.Splitters
{
    public class LetterSplitter
    {
        private SubMatrix matrix;

        public LetterSplitter SetNewMatrix(SubMatrix matrix)
        {
            this.matrix = matrix;
            return this;
        }

        private int GetCountOfBlackInRow(SubMatrix matrix, int y)
        {
            int count = 0;
            for (int x = 0; x < matrix.Width; ++x)
            {
                if (matrix.Get(x,y)) count++;
            }
            return count;
        }

        private SubMatrix TrimWhiteSpaceTopBot(SubMatrix matrix)
        {
            int top = 0, bot = 0;
            try
            {
                for (int y = 0; y < matrix.Height; ++y)
                {
                    if (GetCountOfBlackInRow(matrix, y) > 0)
                    {
                        top = y;
                        break;
                    }
                }
                for (int y = matrix.Height - 1; y > 0; --y)
                {
                    if (GetCountOfBlackInRow(matrix, y) > 0)
                    {
                        bot = y + 1;
                        break;
                    }
                }
                return matrix.GetSubMatrix(new Rectangle
                {
                    X = 0,
                    Y = top,
                    Width = matrix.Width,
                    Height = bot - top
                });
            } catch(Exception) { Console.WriteLine("Error for: {0} {1}", matrix.Height, matrix.Width); return matrix; }
        }

        private void TrimTopBotMatrixes(List<SubMatrix> mats)
        {
            var factory = new TaskFactory();
            var tasks = new List<Task>();
            for (int i = 0; i < mats.Count; ++i)
            {
                if (mats[i] != null)
                    tasks.Add(factory.StartNew((cord) => mats[(int)cord] = TrimWhiteSpaceTopBot(mats[(int)cord]), i));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private bool CheckIfColIsAllWhite(int x)
        {
            for (int y = 0; y < matrix.Height; ++y)
            {
                if (matrix.Get(x,y))
                {
                    return false;
                }
            }
            return true;
        }

        private List<int> GetWhiteCols()
        {
            List<int> whiteLinesIndexes = new List<int>();
            var factory = new TaskFactory<bool>();
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int x = 0; x < matrix.Width; ++x)
            {
                tasks.Add(factory.StartNew((coord) => CheckIfColIsAllWhite((int)coord), x));
            }
            for (int x = 0; x < matrix.Width; ++x)
            {
                if (tasks[x].Result)
                {
                    whiteLinesIndexes.Add(x);
                }
            }
            if (!CheckIfColIsAllWhite(matrix.Width - 1))
            {
                whiteLinesIndexes.Add(matrix.Width - 1);
            }
            return whiteLinesIndexes;
        }

        private List<SubMatrix> GetLettersUntrimmed(List<int> whiteColumns, Queue<int> spaceIndexes)
        {
            var factory = new TaskFactory<SubMatrix>();
            var tasks = new List<Task<SubMatrix>>();
            for(int i = 0; i < whiteColumns.Count - 1; ++i)
            {
                int space = whiteColumns[i + 1] - whiteColumns[i];
                if (space > 1)
                {
                    tasks.Add(factory.StartNew((Index) =>
                    {
                        int index = (int)Index;
                        return matrix.GetSubMatrix(new Rectangle
                        {
                            X = whiteColumns[index],
                            Y = 0,
                            Width = whiteColumns[index + 1] - whiteColumns[index],
                            Height = matrix.Height
                        });
                    }, i));
                }
                else if (spaceIndexes.Count > 0 && whiteColumns[i + 1] > spaceIndexes.Peek())
                {
                    tasks.Add(factory.StartNew(() => null));
                    spaceIndexes.Dequeue();
                }
            }
            return tasks.Select(t => t.Result).ToList();
        }

        /*<summary>
         * null ImageMatrix to be treated as ' ' aka space
         *</summary>
         */
        public List<SubMatrix> GetLetters()
        {
            var whiteCols = GetWhiteCols();
            var spaces = FindSpaces(whiteCols);
            var mats = GetLettersUntrimmed(whiteCols, new Queue<int>(spaces));
            TrimTopBotMatrixes(mats);
            return mats;
        }
        public List<SubMatrix> GetLettersNoSpaces()
        {
            var whiteCols = GetWhiteCols();
            var mats = GetLettersUntrimmed(whiteCols, new Queue<int>());
            TrimTopBotMatrixes(mats);
            return mats;
        }

        /// <summary>
        /// FIXME
        /// </summary>
        /// <param name="whiteCols"></param>
        /// <returns></returns>
        private List<KeyValuePair<int, int>> CalculateIntervals(IEnumerable<int> whiteCols)
        {
            List<KeyValuePair<int, int>> intervals = new List<KeyValuePair<int, int>>();
            int start = whiteCols.First();
            int count = 0;
            var iter = whiteCols.GetEnumerator();
            iter.MoveNext();
            int previous = iter.Current;
            while (iter.MoveNext())
            {
                if (iter.Current - previous == 1)
                {
                    count++;
                }
                else
                {
                    intervals.Add(new KeyValuePair<int, int>(start, count));
                    start = iter.Current;
                    count = 0;
                }
                previous = iter.Current;
            }
            return intervals;
        }

        private static IEnumerable<int> TrimColumns(List<int> whiteColumns)
        {
            int start = 0;
            for (int i = 0; i < whiteColumns.Count; ++i)
            {
                if (whiteColumns[i + 1] - whiteColumns[i] != 1)
                {
                    start = i + 1;
                    break;
                }
            }
            int end = 0;
            for (int i = whiteColumns.Count - 1; i > -1; --i)
            {
                if (whiteColumns[i] - whiteColumns[i - 1] != 1)
                {
                    end = i;
                    break;
                }
            }
            return whiteColumns.Skip(start).Take(end - start);
        }

        /// <summary>
        /// Based on average interval between letters, marks indexes, where spaces begin.
        /// </summary>
        /// <returns>Indexes where spaces begin.</returns>
        private IEnumerable<int> FindSpaces(List<int> whiteCols)
        {
            var trimmed = TrimColumns(whiteCols);
            if (!trimmed.Any())
            {
                return new List<int>();
            }
            var intervals = CalculateIntervals(trimmed);
            var avg = intervals.Average(a => a.Value) * 1.3;
            intervals.RemoveAll(kvp => kvp.Value < avg);
            return intervals.Select(kvp => kvp.Key);
        }

        public LetterSplitter(SubMatrix matrix)
        {
            this.matrix = matrix;
        }
    }
}
