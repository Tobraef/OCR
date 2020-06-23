using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LetterReader.ImagePrepare;
using System.IO;

namespace LetterReader.MachineLearning.DBHandle
{
    public class DataCollector
    {
        private ImageCreator creator = new ImageCreator();

        public string FontFolder
        {
            private get;
            set;
        }

        private void WriteToFolder(ImageMatrix matrix, char c, string appendFolder)
        {
            int number = 0;
            string file;
            if (c == '.')
                file = "dot";
            else if (c == ',')
                file = "coma";
            else
                file = c.ToString();
            file = Path.Combine(appendFolder, file);
            while (File.Exists(file + number.ToString()))
            {
                number++;
            }
            matrix.SaveMatrixToFile(file + number.ToString());
        }

        public void RecordCapitals(Dictionary<char, ImageMatrix> toSave)
        {
            var folder = FontFolder + DATA.capitalsFolderNode;
            foreach (var kvp in toSave)
            {
                WriteToFolder(kvp.Value, kvp.Key, folder);
            }
        }

        public void RecordNormals(Dictionary<char, ImageMatrix> toSave)
        {
            var folder = FontFolder + DATA.normalsFolderNode;
            foreach (var kvp in toSave)
            {
                WriteToFolder(kvp.Value, kvp.Key, folder);
            }
        }

        public void RecordNumbers(Dictionary<char, ImageMatrix> toSave)
        {
            var folder = FontFolder + DATA.numbersFolderNode;
            foreach (var kvp in toSave)
            {
                WriteToFolder(kvp.Value, kvp.Key, folder);
            }
        }

        public void RecordLetter(ImageMatrix matrix)
        {
            string targetFolder;
            if (char.IsLetter(matrix.Character))
            {
                if (char.IsUpper(matrix.Character))
                {
                    targetFolder = Path.Combine(FontFolder, DATA.capitalsFolderNode);
                }
                else
                {
                    targetFolder = Path.Combine(FontFolder, DATA.normalsFolderNode);
                }
            }
            else
            {
                targetFolder = Path.Combine(FontFolder, DATA.numbersFolderNode);
            }
            WriteToFolder(matrix, matrix.Character, targetFolder);
        }

        /**<returns>
         * Name of the new created folder with subfolders in it
         * </returns>
         */
        public static string GenerateFontFolder()
        {
            string fontFolderName = "";
            var dirs = Directory.EnumerateDirectories(DATA.mainFolder)
                .Where(dir => dir.Contains("generated"));
            if (dirs.Count() == 0)
            {
                fontFolderName = "generated0";
            }
            else
            {
                fontFolderName += dirs.Max(dir => dir.Last());
            }
            fontFolderName = Path.Combine(DATA.mainFolder, fontFolderName);
            Directory.CreateDirectory(fontFolderName);
            string normalLettersFolder = Path.Combine(fontFolderName, "Normal");
            string capitalLettersFolder = Path.Combine(fontFolderName, "Capital");
            string numberLettersFolder = Path.Combine(fontFolderName, "Number");
            Directory.CreateDirectory(normalLettersFolder);
            Directory.CreateDirectory(capitalLettersFolder);
            Directory.CreateDirectory(numberLettersFolder);
            return fontFolderName;
        }

        public static string GenerateFontFolder(string name, RecognitionModel model)
        {
            string modelFolder = string.Empty;
            switch (model)
            {
                case RecognitionModel.FixedSize: modelFolder = DATA.fixedSizeFolderNode; break;
                case RecognitionModel.Resizing: modelFolder = DATA.varyingSizeFolderNode; break;
            }
            var fontFolder = Path.Combine(DATA.mainFolder, modelFolder, name);
            Directory.CreateDirectory(fontFolder);
            Directory.CreateDirectory(Path.Combine(fontFolder, DATA.capitalsFolderNode));
            Directory.CreateDirectory(Path.Combine(fontFolder, DATA.normalsFolderNode));
            return fontFolder;
        }
    }
}
