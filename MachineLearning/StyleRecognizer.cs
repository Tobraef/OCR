using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetterReader.ImagePrepare;
using System.IO;
using LetterReader.MachineLearning.DBHandle;

namespace LetterReader.MachineLearning
{
    /// CURRENTLY NOT USED, WILL PROBABLY MOVE TO RECOGNITION MODELS

    //public class StyleRecognizer
    //{ 
    //    private Task<ImageMatrix> FindMatch(ImageMatrix letter, List<ImageMatrix> letters)
    //    {
    //        return Task<ImageMatrix>.Factory.StartNew(
    //            () => letters.FirstOrDefault(mat => MatrixTools.EqualPixelRatio(letter, mat) > DATA.CompareRatio_Decent));
    //    } 

    //    private Dictionary<char, ImageMatrix> TryMatchWithTimesNewRoman(string fromFolder, List<ImageMatrix> letters)
    //    {
    //        return Directory.GetFiles(fromFolder).ToDictionary(
    //            file => file.First(), 
    //            file => FindMatch(new ImageMatrix(file), letters))
    //            .Where(kvp => !kvp.Value.Result.Empty)
    //            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Result);
    //    }

    //    public void RecordNewStyle(List<ImageMatrix> capitals, List<ImageMatrix> normals, string fontFolder)
    //    {
    //        var smalls = TryMatchWithTimesNewRoman(Path.Combine(DATA.mainFolder, "TimesNewRoman", DATA.capitalsFolder),
    //            capitals);
    //        var caps = TryMatchWithTimesNewRoman(Path.Combine(DATA.mainFolder, "TimesNewRoman", DATA.normalsFolder),
    //            normals);
    //        var collector = new DataCollector { FontFolder = fontFolder };
    //        collector.RecordNormals(smalls);
    //        collector.RecordCapitals(caps);
    //    }

    //    public string MatchWithExistingStyle(ImageMatrix letter, char c)
    //    {
    //        string type;
    //        if (char.IsDigit(c))
    //            type = DATA.numbersFolder;
    //        else if (char.IsUpper(c))
    //            type = DATA.capitalsFolder;
    //        else
    //            type = DATA.normalsFolder;
    //        var pairs = FileRetriever.All_Type_Letter(type, c);
    //        return pairs.FirstOrDefault(kvp => MatrixTools.EqualPixelRatio(
    //            new ImageMatrix(kvp.Value), letter) > DATA.CompareRatio_Decent).Key;
    //    }

    //    public string MatchWithExistingStyle(ImageMatrix capital)
    //    {
    //        var toTestFolders = FileRetriever.All_Type_All(DATA.capitalsFolder);
    //        ImageMatrix tested;
    //        KeyValuePair<float, string> highestThusFar = new KeyValuePair<float, string>();
    //        foreach (var folder in toTestFolders)
    //        {
    //            foreach (var file in folder.Value)
    //            {
    //                tested = new ImageMatrix(file);
    //                var compareRatio = MatrixTools.EqualPixelRatio(capital, tested);
    //                if (compareRatio >= DATA.CompareRatio_Solid)
    //                {
    //                    return folder.Key;
    //                }
    //                else if (compareRatio >= DATA.CompareRatio_Valid)
    //                {
    //                    if (highestThusFar.Key < compareRatio)
    //                    {
    //                        highestThusFar = new KeyValuePair<float, string>(compareRatio, folder.Key);
    //                    }
    //                }
    //            }
    //        }
    //        Console.WriteLine("StyleRecognizer-MatchWithExistingStyleCapital: Style matched: " + highestThusFar.Value);
    //        return highestThusFar.Value;
    //    }

    //    public string MatchWithExistingStyle(IEnumerable<ImageMatrix> normals)
    //    {
    //        float Score_Solid = 1.2f;
    //        float Score_Decent = 1.0f;
    //        float Score_Valid = 0.7f;

    //        var toTestLetters = DataRetriever.All_Type_All(DATA.normalsFolder);
    //        string toRet = string.Empty;
    //        float maxScore = 0;
    //        foreach (var testLetters in toTestLetters)
    //        {
    //            float score = 0;
    //            foreach (var normal in normals)
    //            {
    //                int rIndex = -1;
    //                for (int i = 0; i < testLetters.Value.Count; ++i)
    //                {
    //                    var compareRatio = MatrixTools.EqualPixelRatio(normal, testLetters.Value[i]);
    //                    if (compareRatio >= DATA.CompareRatio_Solid)
    //                    {
    //                        rIndex = i;
    //                        score += Score_Solid;
    //                    }
    //                    else if (compareRatio >= DATA.CompareRatio_Decent)
    //                    {
    //                        score += Score_Decent;
    //                    }
    //                    else if (compareRatio >= DATA.CompareRatio_Valid)
    //                    {
    //                        score += Score_Valid;
    //                    }
    //                }
    //                if (rIndex != -1)
    //                {
    //                    testLetters.Value.RemoveAt(rIndex);
    //                }
    //            }
    //            if (score > maxScore)
    //            {
    //                maxScore = score;
    //                toRet = testLetters.Key;
    //            }
    //        }
    //        Console.WriteLine("StyleRecognizer-MatchWithExistingStyleNormals: Found style: " + toRet);
    //        return toRet;
    //    }
    //}
}
