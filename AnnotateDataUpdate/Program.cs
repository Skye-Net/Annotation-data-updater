using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnnotateDataUpdate
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (DataUpdateFromArguments(args)) return;
            var path = GetPathAndAction(out var input);
            PerformDataUpdate(input, path);
        }

        #region User interaction
        /// <summary>
        /// Starts the action based on the user selection
        /// </summary>
        /// <param name="input">The consolekey</param>
        /// <param name="path">The path with data</param>
        private static void PerformDataUpdate(ConsoleKey input, string path)
        {
            while (input != ConsoleKey.D0 && input != ConsoleKey.NumPad0)
            {
                switch (input)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        RemoveInvalidData(path);
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        RemoveImagesWithNoTextFiles(path);
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        Console.WriteLine("Input class number to replace:\n");
                        var classNumber = Console.ReadLine();
                        RemoveAnnotationClass(path, classNumber);
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        Console.WriteLine("Input filename LIKE comparator that should be removed from dataset:\n");
                        var fileNameToRemove = Console.ReadLine();
                        RemoveFilesLike(path, fileNameToRemove);
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        RenameAnnotations(path);
                        break;
                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        RemoveTextFilesWithNoImages(path);
                        break;
                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                        Console.WriteLine("\nHow many percent of total data should be in the validation set? (Ex: 25)");
                        var percent = Console.ReadLine();
                        ExtractRandomValidationSet(path, percent);
                        break;
                    case ConsoleKey.D8:
                    case ConsoleKey.NumPad8:
                        ConvertAllImagesToPng(path);
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        Environment.Exit(0);
                        break;
                }

                Console.WriteLine();
                Console.WriteLine("Done!");
                Console.WriteLine("What do want to do?");
                input = Console.ReadKey().Key;
            }
        }

        /// <summary>
        /// Interacts with the user and gets user input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string GetPathAndAction(out ConsoleKey input)
        {
            Console.WriteLine("Input filepath");
            var path = Console.ReadLine();
            Console.WriteLine("What do want to do?");
            Console.WriteLine("1. Remove Invalid data from dataset");
            Console.WriteLine("2. Removes images with no corresponding text files");
            Console.WriteLine("3. Removes a particular annotation class from the testdata");
            Console.WriteLine("4. Removes datafiles that fulfills a LIKE comparator with another string");
            Console.WriteLine("5. Rename annotation");
            Console.WriteLine("6. Remove textfiles with no images");
            Console.WriteLine("7. Automatically extract a random validation set (25%)");
            Console.WriteLine("8. Convert all images to png");
            Console.WriteLine("0. Exit");
            input = Console.ReadKey().Key;
            return path;
        }
        
        /// <summary>
        /// Performs program actions based on program arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool DataUpdateFromArguments(string[] args)
        {
            if (args is not null && args.Length > 0)
            {
                var action = args[0];
                var directoryPath = args[1];
                var arg3 = args.Length > 2 ? args[2] : string.Empty;
                switch (action)
                {
                    case "1":
                        RemoveInvalidData(directoryPath);
                        break;
                    case "2":
                        RemoveImagesWithNoTextFiles(directoryPath);
                        break;
                    case "3":
                        RemoveAnnotationClass(directoryPath, arg3);
                        break;
                    case "4":
                        RemoveFilesLike(directoryPath, arg3);
                        break;
                    case "5":
                        RenameAnnotations(directoryPath);
                        break;
                    case "6":
                        RemoveTextFilesWithNoImages(directoryPath);
                        break;
                    case "7":
                        ExtractRandomValidationSet(directoryPath, arg3);
                        break;
                    case "8":
                        ConvertAllImagesToPng(directoryPath);
                        break;
                    default:
                        Console.WriteLine("Invalid input, please use 1-8 and a path");
                        break;
                }

                return true;
            }

            Console.WriteLine("Invalid arguments presented");
            return false;
        }
        
        #endregion

        #region Actions
        
        /// <summary>
        /// Action 1: Removes "invalid" data from a dataset
        /// Invalid data is a piece of annotated data where there is no object present in the image.
        /// In YOLO this is represented by an empty *.txt file. This removes the corresponding image and empty text file
        /// </summary>
        /// <param name="path">The path with data</param>
        private static void RemoveInvalidData(string path)
        {
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles("*.txt");
            Parallel.ForEach(files, file =>
            {
                var text = File.ReadAllText(file.FullName);
                if (string.IsNullOrWhiteSpace(text))
                {
                    File.Delete(file.FullName);
                    var filenameNoExtension = Path.GetFileNameWithoutExtension(file.FullName);
                    var dirName = Path.GetDirectoryName(file.FullName);
                    if (File.Exists(filenameNoExtension + ".png"))
                    {
                        File.Delete(Path.Combine(dirName, filenameNoExtension + ".png"));
                    }
                    if (File.Exists(filenameNoExtension + ".bmp"))
                    {
                        File.Delete(Path.Combine(dirName, filenameNoExtension + ".bmp"));
                    }
                    if (File.Exists(filenameNoExtension + ".jpg"))
                    {
                        File.Delete(Path.Combine(dirName, filenameNoExtension + ".jpg"));
                    }
                }
            });
        }

        /// <summary>
        /// Action 2: Removes images with no corresponding text files
        /// </summary>
        /// <param name="path">The path with data</param>
        private static void RemoveImagesWithNoTextFiles(string path)
        {
            var extensions = new[] {"jpg", "png", "bmp"};
            var files = Extensions.FilterFiles(path, extensions);
            Parallel.ForEach(files, file =>
            {
                var fileInfo = new FileInfo(file);
                if (!File.Exists(Path.ChangeExtension(fileInfo.FullName, "txt")))
                {
                    fileInfo.Delete();
                }
            });
        }
        

        /// <summary>
        /// Action 3: Removes a particular annotation class from the testdata
        /// </summary>
        /// <param name="path">The path with data</param>
        /// <param name="classNumberToRemove">The class to remove</param>
        private static void RemoveAnnotationClass(string path, string classNumberToRemove)
        {
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles("*.txt");

            Parallel.ForEach(files, file =>
            {
                var text = File.ReadAllLines(file.FullName);
                var oldText = string.Empty;
                foreach (var textLine in text)
                {
                    var splitLine = textLine.Split(" ");
                    var firstLineSplit = splitLine.FirstOrDefault();

                    if (firstLineSplit != classNumberToRemove)
                    {
                        oldText += string.IsNullOrWhiteSpace(oldText) ? textLine : Environment.NewLine + textLine;
                    }
                }
                File.WriteAllText(file.FullName, oldText);
            });
        }
        
        /// <summary>
        /// Action 4: Removes datafiles that fulfills a LIKE comparator with another string
        /// Useful when accidentally doubling all data from an accidental copy or something similar 
        /// </summary>
        /// <param name="path">The path with data</param>
        /// <param name="compareString">The string to compare against</param>
        private static void RemoveFilesLike(string path, string compareString)
        {
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles();
            Parallel.ForEach(files, file =>
            {
                if (Path.GetFileName(file.FullName).ToLowerInvariant().Contains(compareString))
                {
                    file.Delete();
                }
            });
            
        }
        
        /// <summary>
        /// Action 5: Renames an annotation
        /// This cannot be done from arguments without inputting information afterwards
        /// </summary>
        /// <param name="path"></param>
        private static void RenameAnnotations(string path)
        {
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles("*.txt");

            Console.WriteLine("\nInput old value to replace");
            var oldVal = Console.ReadKey().KeyChar;
            Console.WriteLine("\nInput new value");
            var newVal = Console.ReadKey().KeyChar;
            Parallel.ForEach(files, file =>
            {
                var text = File.ReadAllLines(file.FullName);
                var lineCount = 0;
                var oldText = String.Empty;
                foreach (var textLine in text)
                {
                    lineCount++;
                    var splitLine = textLine.Split(" ");
                    var firstLineSplit = splitLine.FirstOrDefault();

                    if (firstLineSplit == oldVal.ToString())
                    {
                        oldText += string.IsNullOrWhiteSpace(oldText) ? Extensions.ReplaceAtIndex(0, newVal, textLine) : Environment.NewLine + Extensions.ReplaceAtIndex(0, newVal, textLine); 
                    }
                    else
                    {
                        oldText += string.IsNullOrWhiteSpace(oldText) ? textLine : Environment.NewLine + textLine;
                    }
                    
                }
                File.WriteAllText(file.FullName, oldText);
            });
        }
        
        /// <summary>
        /// Action 6: Removes annotation text files where there is no corresponding image
        /// </summary>
        /// <param name="path">The path to remove annotation text files from</param>
        private static void RemoveTextFilesWithNoImages(string path)
        {
            var extensions = new[] {"txt"};
            var files = Extensions.FilterFiles(path, extensions);
            Parallel.ForEach(files, file =>
            {
                var fileInfo = new FileInfo(file);
                if (!File.Exists(Path.ChangeExtension(fileInfo.FullName, "jpg")) && !File.Exists(Path.ChangeExtension(fileInfo.FullName, "png")) && !File.Exists(Path.ChangeExtension(fileInfo.FullName, "bmp")))
                {
                    fileInfo.Delete();
                }
            });
        }
        
        /// <summary>
        /// Action 7: Extracts a random subset for validation from the full dataset
        /// By default this is 25%
        /// </summary>
        /// <param name="path">Path with the full dataset</param>
        /// <param name="percent">The percent to extract</param>
        private static void ExtractRandomValidationSet(string path, string percent = "25")
        {
            if (string.IsNullOrWhiteSpace(percent))
            {
                percent = "25";
            }
            var destDir = new DirectoryInfo(Path.Combine(path, "ValidationSet"));
            if (!destDir.Exists)
            {
                destDir.Create();
            }
            
            var extensions = new[] {"jpg", "png", "bmp"};
            var files = Extensions.FilterFiles(path, extensions).ToList();
            var totalImages = files.Count;
            var validationSetCount = (int) (totalImages * Convert.ToInt32(percent) / 100f);
            var random = new Random();
            var indexList = new List<int>();
            for (var i = 0; i < validationSetCount; i++)
            {
                var randomValue = random.Next(0, totalImages);
                while (indexList.Contains(randomValue))
                {
                    randomValue = random.Next(0, totalImages);
                }
                
                indexList.Add(randomValue);
            }

            Parallel.ForEach(indexList, index =>
            {
                var imageFile = new FileInfo(files[index]);
                var txtFile = new FileInfo(Path.ChangeExtension(files[index], "txt"));
                imageFile.MoveTo(Path.Combine(destDir.FullName, imageFile.Name));
                txtFile.MoveTo(Path.Combine(destDir.FullName, txtFile.Name));
            });

        }
        
        /// <summary>
        /// Action 8: Converts all images to PNG and deletes the old version
        /// </summary>
        /// <param name="path">The path to convert images in</param>
        private static void ConvertAllImagesToPng(string path)
        {
            var extensions = new[] {"jpg", "bmp"};
            var files = Extensions.FilterFiles(path, extensions).ToList();
            Parallel.ForEach(files, file =>
            {
                var fileInfo = new FileInfo(file);
                var img = new Bitmap(file);
                img.Save(Path.ChangeExtension(file, "png"), ImageFormat.Png);
                img.Dispose();
                fileInfo.Delete();
            });
        }
        #endregion
    }
}