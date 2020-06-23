using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.MachineLearning;
using LetterReader.ImagePrepare;
using System.IO;

namespace LetterReader.MachineLearning.DBHandle
{
    public static class DataRetriever
    {
        private static string LastPartOf(string text)
        {
            return text.Substring(text.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }

        public static ImageMatrix Size_Font_Type_Letter(string size, string font, string type, char c)
        {
            return new ImageMatrix(Path.Combine(DATA.mainFolder, size, font, type, c.ToString(), 0.ToString()));
        }

        public static Dictionary<string, ImageMatrix> Size_All_Type_Letter(string size, string type, char c)
        {
            return Directory.GetDirectories(Path.Combine(DATA.mainFolder, size))
                .Select(dir => Path.Combine(dir, type))
                .Select(dir => Directory.GetFiles(dir).FirstOrDefault(file => file.Contains(c)))
                .Where(str => !str.Equals(string.Empty))
                .ToDictionary(
                    str => LastPartOf(str),
                    str => new ImageMatrix(str));
        }

        public static List<ImageMatrix> Size_Font_All_All(string size, string font)
        {
            List<ImageMatrix> toRet = new List<ImageMatrix>();
            Directory.GetDirectories(Path.Combine(DATA.mainFolder, size, font))
                .Select(dir => Directory.GetFiles(dir)).ToList()
                .ForEach(files => toRet.AddRange(files
                    .Select(file => new ImageMatrix(file))));
            return toRet;
        }

        public static List<ImageMatrix> Size_Font_Type_All(string size, string font, string type)
        {
            return Directory.GetFiles(Path.Combine(DATA.mainFolder, size, font, type))
                .Select(file => new ImageMatrix(file)).ToList();
        }

        public static Dictionary<string, List<ImageMatrix>> Size_All_Type_All(string size, string type)
        {
            return Directory.GetDirectories(Path.Combine(DATA.mainFolder, size)).ToDictionary(
               dir => LastPartOf(dir),
               dir => Directory.GetFiles(Path.Combine(size, dir, type)).Select(file => new ImageMatrix(file)).ToList());
        }
    }
}
