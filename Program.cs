using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Locator;

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
                var rootPath = visualStudioInstances[i].VisualStudioRootPath;
                PrintDuplicateFiles(rootPath);
            }
        }

        private static void PrintDuplicateFiles(string folderPath)
        {
            var dictionary = new Dictionary<string, List<FileInfo>>(StringComparer.OrdinalIgnoreCase);
            var files = Directory.EnumerateFiles(folderPath, "*.dll", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                var value = new FileInfo(fullPath);
                if (!dictionary.ContainsKey(value.Name))
                {
                    dictionary[value.Name] = new List<FileInfo>() { value };
                }
                else
                {
                    var list = dictionary[value.Name];
                    list.Add(value);
                }
            }

            var removeList = new List<string>();
            foreach (var key in dictionary.Keys)
            {
                if (dictionary[key].Count == 1 || key.EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase))
                {
                    removeList.Add(key);
                }
            }

            foreach (var key in removeList)
            {
                dictionary.Remove(key);
            }

            var totalFiles = dictionary.Values.SelectMany(x => x).Count();
            var totalWastedBytes = 0L;
            var largestWaste = 0L;
            FileInfo largestDuplicateFile = null;
            FileInfo mostDuplicateFile = null;
            int numberOfDuplicates = 0;
            int largestNumberOfDuplicates = 0;
            foreach (var key in dictionary.Keys)
            {
                var numberOfFiles = dictionary[key].Count - 1;
                var fileInfo = dictionary[key].First();

                largestNumberOfDuplicates = Max(numberOfFiles, largestNumberOfDuplicates);
                if (largestNumberOfDuplicates == numberOfFiles)
                {
                    mostDuplicateFile = fileInfo;
                }

                var bytes = fileInfo.Length;
                var wastedBytes = numberOfFiles * bytes;
                largestWaste = Max(largestWaste, wastedBytes);
                if (largestWaste == wastedBytes)
                {
                    largestDuplicateFile = fileInfo;
                    numberOfDuplicates = numberOfFiles + 1;
                }
                totalWastedBytes += wastedBytes;
            }

            WriteLine($"Found '{dictionary.Keys.Count}' duplicate dlls in '{folderPath}'");
            WriteLine($"{totalWastedBytes.PrintSize()} of waste across {totalFiles} files");
            WriteLine();
            WriteLine($"Most impactful duplicate file is '{largestDuplicateFile.Name}' ({largestWaste.PrintSize()}) which is repeated {numberOfDuplicates} times");
            WriteLine($"with a total waste of {(largestWaste * (numberOfDuplicates - 1)).PrintSize()}");
            WriteLine("Duplicate paths are:");
            foreach (var file in dictionary[largestDuplicateFile.Name])
            {
                WriteLine($"    {file.FullName}");
            }
            WriteLine();
            WriteLine($"Most duplicated file is '{mostDuplicateFile.Name}' ({mostDuplicateFile.Length.PrintSize()}) which is duplicated {largestNumberOfDuplicates} times");
            WriteLine("Duplicate paths are:");
            foreach (var file in dictionary[mostDuplicateFile.Name])
            {
                WriteLine($"    {file.FullName}");
            }
            WriteLine();
            _ = ReadLine();
        }
    }
}
