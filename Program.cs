using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LetterReader.MachineLearning;

namespace LetterReader
{
    class Program
    {
        private static void QuickShot()
        {
            Machine machine = new Machine();
            var letters = machine.ProcessBook(new List<string> { DATA.standardTestFile }, RecognitionModel.Resizing);
            Console.Write(letters.Aggregate("", (t, c) => t + c));
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            Application.Run(new DisplayManager());
            //QuickShot();
        }
    }
}
