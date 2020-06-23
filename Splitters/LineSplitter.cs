using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LetterReader.ImagePrepare;

namespace LetterReader.Splitters
{
    public class LineSplitter
    {
        private SubMatrix image;

        private bool CheckIfRowIsAllWhite(int y)
        {
            for (int x = 0; x < image.Width; ++x)
            {
                if (image.Get(x, y))
                {
                    return false;
                }
            }
            return true;
        }

        private List<int> GetWhiteRows()
        {
            List<int> whiteLinesIndexes = new List<int>();
            var factory = new TaskFactory<bool>();
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int y = 0; y < image.Height; ++y)
            {
                tasks.Add(factory.StartNew((coord) => CheckIfRowIsAllWhite((int)coord), y));
            }
            for (int y = 0; y < image.Height; ++y)
            {
                if (tasks[y].Result)
                {
                    whiteLinesIndexes.Add(y);
                }
            }
            return whiteLinesIndexes;
        }

        /*<algo>
         * based on rows that contain only white, check intervals between them:
         * interval begin - index, interval length - height
         * if begins with non 0, first white is already after one line, nice fitting must have happened
         * </algo>
         */
        private Task<List<KeyValuePair<int, int>>> GetIndexesToHeight(List<int> whiteLinesIndexes)
        {
            return Task.Run(() =>
            {
                List<KeyValuePair<int, int>> indexToHeightSections = new List<KeyValuePair<int, int>>();
                if (whiteLinesIndexes[0] > 1)
                {
                    Console.WriteLine("WARNING: top section of text immediately begins with black, too good fitting, bottom can be lost: LineSplitter");
                    indexToHeightSections.Add(new KeyValuePair<int, int>(0, whiteLinesIndexes[0]));
                }
                for (int i = 0; i < whiteLinesIndexes.Count - 1; ++i)
                {
                    if (whiteLinesIndexes[i] - whiteLinesIndexes[i + 1] < -5)
                    {
                        indexToHeightSections.Add(new KeyValuePair<int, int>(whiteLinesIndexes[i], whiteLinesIndexes[i + 1] - whiteLinesIndexes[i]));
                    }
                }
                return indexToHeightSections;
            });
        }

        /*<summary>
         * traverser from left to right looking for a line containing a black pixel
         * </summary>
         */
        private int ScanForFirstNonWhiteColumn(int yTop, int yBottom)
        {
            for (int x = 0; x < image.Width; ++x)
            {
                for (int y = yTop; y < yBottom; ++y)
                {
                    if (image.Get(x,y))
                    {
                        return x;
                    }
                }
            }
            Console.WriteLine("WARNING: couldnt find black in section marked as black for top={0} bot={1}", yTop, yBottom);
            return 0;
        }

        /*<summary>
         * Scans for first column containing black pixel from right to begin
         * </summary>
         */
        private int ScanForFirstNonWhiteColumnReverse(int yTop, int yBottom)
        {
            for (int x = image.Width - 1; x > 0; --x)
            {
                for (int y = yTop; y < yBottom; ++y)
                {
                    if (image.Get(x, y))
                    {
                        return x;
                    }
                }
            }
            Console.WriteLine("WARNING: couldnt find black in section marked as black for top={0} bot={1}", yTop, yBottom);
            return 0;
        }

        /*<algo>
         * given line height and begin, trim left and right to fit the rectangle
         * </algo>
         * <rethink>
         *   these left bound and right bound setting, is partially done by MatrixTools, where the frame
         *   with text only is substracted.The thing is, it's not that efficiect, because it takes the upper bound of a letter.
         *   So when sentence is: Tooo the upper bound of 'T' wont have enough pixels to be taken as a sentence.
         *   This piece of code neglects the left and right white space.Its a short adjustment so i guess it will stay,
         *   because the line detection for some reason fails without it, and the reason described above is probably the one
         *   <sol> improve text matrix finding </sol>
         * </rethink>
         */
        private Rectangle GetLetterLine(int lineIndex, int height)
        {
            var factory = new TaskFactory<int>();
            var leftBoundTask = factory.StartNew(() => ScanForFirstNonWhiteColumn(lineIndex, lineIndex + height));
            var rightBoundTask = factory.StartNew(() => ScanForFirstNonWhiteColumnReverse(lineIndex, lineIndex + height));
            if (leftBoundTask.Result == 0 || rightBoundTask.Result == 0)
            {
                Console.WriteLine("WARNING: EMPTY RECT");
                return Rectangle.Empty;
            }
            var toRet = new Rectangle
            {
                X = leftBoundTask.Result - 1, // provide first white space, helps letter splitter
                Y = lineIndex,
                Width = rightBoundTask.Result - leftBoundTask.Result + 2,
                Height = height
            };
            return toRet;
        }

        public List<SubMatrix> SplitToLines(SubMatrix matrix)
        {
            image = matrix;
            List<int> whiteLinesIndexes = GetWhiteRows();
            List<KeyValuePair<int, int>> indexToHeights = GetIndexesToHeight(whiteLinesIndexes).Result;
            whiteLinesIndexes.Clear();
            var factory = new TaskFactory<Rectangle>();
            var tasks = indexToHeights.Select(pair => factory.StartNew(
                () => GetLetterLine(pair.Key, pair.Value))).ToArray();
            // met it like this?????
            //List<Rectangle> rectangles = tasks.Select(task => task.Result).ToList();
            //rectangles.RemoveAll(rect => rect.IsEmpty);
            //rectangles.Clear();
            //return indexToHeights.Select(kvp => new SubMatrix(matrix, new Rectangle { X = 0, Y = kvp.Key, Height = kvp.Value, Width = matrix.Width })
            //    ).ToList();
            //
            return tasks.Select(t => matrix.GetSubMatrix(t.Result)).ToList();// new SubMatrix(matrix.Matrix, t.Result)).ToList();
        }
    }
}
