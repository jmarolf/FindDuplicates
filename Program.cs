using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Locator;

using Newtonsoft.Json;

using static System.Console;
using static System.Math;

namespace FindDuplicates
{
    class Program
    {
        static void Main(string[] args)
        {
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToList();
            for (int i = 0; i < visualStudioInstances.Count; i++)
            {
                PrintDuplicateFiles(visualStudioInstances[i].VisualStudioRootPath);
            }
        }

        private static void PrintDuplicateFiles(string folderPath)
        {
            var dictionary = EnumerateFiles(folderPath);
            var totalFiles = dictionary.Values.SelectMany(x => x).Count();
            var largestWaste = 0L;
            var totalWastedBytes = 0L;
            FileInfo largestDuplicateFile = null;
            FileInfo mostDuplicateFile = null;
            int numberOfDuplicates = 0;
            int largestNumberOfDuplicates = 0;
            foreach (var key in dictionary.Keys)
            {
                var files = dictionary[key];
                var numberOfFiles = files.Count - 1;
                var fileInfo = files.First();

                largestNumberOfDuplicates = Max(numberOfFiles, largestNumberOfDuplicates);
                if (largestNumberOfDuplicates == numberOfFiles)
                {
                    mostDuplicateFile = fileInfo;
                }

                var wastedBytes = files.TotalWaste();
                largestWaste = Max(largestWaste, wastedBytes);
                if (largestWaste == wastedBytes)
                {
                    largestDuplicateFile = fileInfo;
                    numberOfDuplicates = numberOfFiles + 1;
                }
                totalWastedBytes += wastedBytes;
            }

            WriteLine($"Found {dictionary.Keys.Count} duplicate dlls in '{folderPath}'");
            WriteLine($"{totalWastedBytes.PrintSize()} of waste across {totalFiles} files");
            WriteLine();
            WriteLine($"Most impactful duplicate file is '{largestDuplicateFile.Name}' ({largestWaste.PrintSize()}) which is repeated {numberOfDuplicates} times");
            WriteLine($"with a total waste of {(largestWaste * (numberOfDuplicates - 1)).PrintSize()}");
            WriteLine("Duplicate paths are:");
            foreach (var file in dictionary[$"{largestDuplicateFile.Name}:{largestDuplicateFile.Length}"])
            {
                WriteLine($"    {file.FullName}");
            }
            WriteLine();
            WriteLine($"Most duplicated file is '{mostDuplicateFile.Name}' ({mostDuplicateFile.Length.PrintSize()}) which is duplicated {largestNumberOfDuplicates} times");
            WriteLine("Duplicate paths are:");
            foreach (var file in dictionary[$"{mostDuplicateFile.Name}:{mostDuplicateFile.Length}"])
            {
                WriteLine($"    {file.FullName}");
            }
            WriteLine();
            var output = dictionary.Select(kvp => new KeyValuePair<string, string[]>(kvp.Key, kvp.Value.Select(x => x.FullName).ToArray())).ToDictionary(x => x.Key, x => x.Value);
            var jsonstring = JsonConvert.SerializeObject(output, new JsonSerializerSettings { Formatting = Formatting.Indented});
            string fileNamePath = GetFileName();
            using var jsonFile = File.CreateText(fileNamePath);
            jsonFile.Write(jsonstring);
            WriteLine($"wrote out complete list of duplicate files to '{Path.GetFullPath(fileNamePath)}'");
            WriteLine();
        }

        private static string GetFileName()
        {
            var fileName = "duplicateFiles";
            var finalFileName = fileName;
            int appendNum = 1;
            while (File.Exists(finalFileName + ".json"))
            {
                finalFileName = fileName + appendNum;
                appendNum++;
            }

            return finalFileName + ".json";
        }

        private static Dictionary<string, List<FileInfo>> EnumerateFiles(string folderPath)
        {
            var dictionary = new Dictionary<string, List<FileInfo>>(StringComparer.OrdinalIgnoreCase);
            var files = Directory.EnumerateFiles(folderPath, "*.dll", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                var fileInfo = new FileInfo(fullPath);
                var size = fileInfo.Length;
                var key = $"{fileInfo.Name}:{size}";
                if (!dictionary.ContainsKey(key))
                {
                    dictionary[key] = new List<FileInfo>() { fileInfo };
                }
                else
                {
                    var list = dictionary[key];
                    list.Add(fileInfo);
                }
            }

            Cleanup(dictionary);

            return dictionary;
        }

        private static void Cleanup(Dictionary<string, List<FileInfo>> dictionary)
        {
            var removeList = new HashSet<string>();
            foreach (var key in dictionary.Keys)
            {
                var files = dictionary[key];
                if (files.Count == 1 || key.Split(':').First().EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase))
                {
                    removeList.Add(key);
                }
            }

            foreach (var key in removeList)
            {
                dictionary.Remove(key);
            }
        }
    }
}
