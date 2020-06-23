using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace LetterReader.MachineLearning.DBHandle
{
    /**<summary>Returns specified file names instead of its content. Just another layer of abstraction</summary>
     */
    public static class FileRetriever
    {
        private static string LastPartOf(string text)
        {
            return text.Substring(text.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }

        public static string Size_Font_Type_Letter(string size, string font, string type, char c)
        {
            return Path.Combine(DATA.mainFolder, size, font, type, c.ToString(), 0.ToString());
        }

        /**<summary>Could be reduced to just a list of files, as directory name is contained in the file name, but would be hard to extuinguish</summary>
         */
        public static Dictionary<string, string> Size_All_Type_Letter(string size, string type, char c)
        {
            return Directory.GetDirectories(Path.Combine(DATA.mainFolder, size))
                .Select(dir => Path.Combine(dir, type))
                .Select(dir => Directory.GetFiles(dir).FirstOrDefault(file => file.Contains(c)))
                .Where(str => !str.Equals(string.Empty))
                .ToDictionary(
                    str => LastPartOf(str),
                    str =>str);
        }

        public static List<string> Size_Font_All_All(string size, string font)
        {
            List<string> toRet = new List<string>();
            Directory.GetDirectories(Path.Combine(DATA.mainFolder, size, font))
                .Select(dir => Directory.GetFiles(dir))
                .ToList()
                .ForEach(arr => toRet.AddRange(arr));
            return toRet;
        }

        public static List<string> Size_Font_Type_All(string size, string font, string type)
        {
            return Directory.GetFiles(Path.Combine(DATA.mainFolder, size, font, type)).ToList();
        }

        public static Dictionary<string, List<string>> Size_All_Type_All(string size, string type)
        {
            return Directory.GetDirectories(Path.Combine(DATA.mainFolder, size)).ToDictionary(
               dir => LastPartOf(dir),
               dir => Directory.GetFiles(Path.Combine(dir, type)).ToList());
        }
    }
}
