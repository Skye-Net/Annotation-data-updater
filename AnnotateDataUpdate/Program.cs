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
            if (args.Length > 0)
            {
                var action = args[0];
                var directoryPath = args[1];
                var percentage = args.Length > 2 ? args[2] : string.Empty;
                switch (action)
                {
                    case "1":
                        RemoveInvalidData(directoryPath);
                        break;
                    case "2":
                        RemoveImagesWithNoTextFiles(directoryPath);
                        break;
                    case "3":
                        RemoveHeadAnnotation(directoryPath);
                        break;
                    case "4":
                        RemoveCopyFiles(directoryPath);
                        break;
                    case "5":
                        RenameAnnotations(directoryPath);
                        break;
                    case "6":
                        RemoveTextFilesWithNoImages(directoryPath);
                        break;
                    case "7":
                        ExtractRandomValidationSet(directoryPath, percentage);
                        break;
                    case "8":
                        ConvertAllImagesToPng(directoryPath);
                        break;
                    default:
                        Console.WriteLine("Invalid input, please use 1-7 and a path");
                        break;
                }
                
                return;
            }
            
            Console.WriteLine("Input filepath");
            var path = Console.ReadLine();
            Console.WriteLine("What do want to do?");
            Console.WriteLine("1. Remove Invalid data from dataset");
            Console.WriteLine("2. Remove images with no annotations");
            Console.WriteLine("3. Remove head annotations from data");
            Console.WriteLine("4. Files with \"copy\" in the name");
            Console.WriteLine("5. Rename annotation");
            Console.WriteLine("6. Remove textfiles with no images");
            Console.WriteLine("7. Automatically extract a random validation set (25%)");
            Console.WriteLine("8. Convert all images to png");
            Console.WriteLine("0. Exit");
            ConsoleKey input = Console.ReadKey().Key;
            while (input != ConsoleKey.D0)
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
                        RemoveHeadAnnotation(path);
                        break;
                    case ConsoleKey.D4: 
                    case ConsoleKey.NumPad4:
                        RemoveCopyFiles(path);
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

        private static void ConvertAllImagesToPng(string? path)
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

        private static void ExtractRandomValidationSet(string path, string percent)
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

        private static void RemoveTextFilesWithNoImages(string? path)
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
       

        private static void RemoveCopyFiles(string path)
        {
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles();
            Parallel.ForEach(files, file =>
            {
                if (Path.GetFileName(file.FullName).ToLowerInvariant().Contains("copy"))
                {
                    file.Delete();
                }
            });
            
        }

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

        private static void RemoveHeadAnnotation(string path)
        {
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles("*.txt");

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

                    if (firstLineSplit is "0" or "2")
                    {
                        oldText += string.IsNullOrWhiteSpace(oldText) ? textLine : Environment.NewLine + textLine;
                    }
                }
                File.WriteAllText(file.FullName, oldText);
            });
        }
    }
}