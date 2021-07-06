using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ImageQuery
{
    class Program
    {
        static void Main(string[] args)
        {
            run();
        }

        static void exit()
        {
            Console.WriteLine("Press ENTER to quit...");
            Console.ReadLine();
            Environment.Exit(0);
        }

        static void run()
        {
            Console.WriteLine($"OFFLINE IMAGE QUERY AND EXTRACTION TOOL v{Assembly.GetExecutingAssembly().GetName().Version}");
            Console.WriteLine("https://github.com/jason-chao/offline-image-query");
            Console.Write(Environment.NewLine);

            Console.WriteLine("This tool is designed to facilitate the query and extraction of image files in social and media research.");
            Console.Write(Environment.NewLine);

            Console.WriteLine("It locates image files scattered in nested and sparse directories by filename, copies them to a new location and then inserts labels as prefixes to the image filenames.  You will be asked to provide paths of folders / files as inputs.");
            Console.Write(Environment.NewLine);

            Console.WriteLine("Hint: You may drag and drop a folder / file to the terminal window to paste the path.");
            Console.Write(Environment.NewLine);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("Note for MacOS and Linux users: Please remove any backslash (\\), quotation mark (' or \") and trailing whitespace from the name of folder or file to be dropped to the terminal window.");
                Console.Write(Environment.NewLine);
            }

            Console.WriteLine("1. CSV file: the CSV file of image filenames and labels");
            Console.WriteLine("Note: The filename must be in the first column and the label must be in the second column.");
            Console.WriteLine("The headers in the first row are irrelevant.");
            Console.Write(Environment.NewLine);

            string csvPath = string.Empty;

            do
            {
                Console.Write("Path (press ENTER when done): ");
                csvPath = cleanPath(Console.ReadLine());

                if (!File.Exists(csvPath))
                    Console.WriteLine("Error: The file does not exist");
                else break;

            } while (true);


            Console.WriteLine("Reading the CSV file ...");

            ICollection<ImageLabel> pairs = null;

            try
            {
                pairs = LocalFileQuery.LoadLabelData(csvPath);
            }
            catch { }
            finally
            {
                if (pairs == null)
                {
                    Console.WriteLine("Error: The CSV file is invalid");
                    exit();
                }
                else
                {
                    if (!pairs.Any())
                    {
                        Console.WriteLine("Error: No valid record in the CSV file");
                        exit();
                    }
                }
            }

            Console.Write(Environment.NewLine);

            Console.WriteLine("2. Source folder: The folder in which all images are located.  The images may be placed in sub-folders.");
            Console.Write(Environment.NewLine);

            string datasetDirPath = string.Empty;

            do
            {
                Console.Write("Path (press ENTER when done): ");
                datasetDirPath = cleanPath(Console.ReadLine());

                if (!Directory.Exists(datasetDirPath))
                    Console.WriteLine("Error: The folder does not exist");
                else break;

            } while (true);

            Console.WriteLine("Looking for the images ...");
            var pairsWithFullPaths = LocalFileQuery.FindFullFilenames(datasetDirPath, pairs);

            var pairsNotFound = pairsWithFullPaths.Where(p => !p.FileInfo.Exists).ToList();
            var pairsFound = pairsWithFullPaths.Where(p => p.FileInfo.Exists).ToList();

            Console.Write(Environment.NewLine);

            Console.WriteLine($"Total entires: {pairs.Count};  Found: {pairsFound.Count};  Not found: {pairsNotFound.Count}");
            Console.Write(Environment.NewLine);

            Console.WriteLine("3. Destinatoin folder: The folder to which the images will be copied.  An empty folder is highly recommended.");
            Console.Write(Environment.NewLine);

            string targetDirPath = string.Empty;

            do
            {

                Console.Write("Path (press ENTER when done): ");
                targetDirPath = cleanPath(Console.ReadLine());

                if (!Directory.Exists(targetDirPath))
                {
                    try
                    {
                        var dirInfo = Directory.CreateDirectory(targetDirPath);
                        targetDirPath = dirInfo.FullName;
                        break;
                    }
                    catch { }
                    finally
                    {
                        Console.WriteLine("Error: The input is invalid");
                    }
                }
                else break;

            } while (true);

            Console.Write(Environment.NewLine);

            int fileCount = 0;

            foreach (var pair in pairsFound)
            {
                string destination = Path.Join(targetDirPath, $"{pair.Label}_{pair.FileInfo.Name}");
                Console.WriteLine($"Copying {pair.Label} : {pair.FileInfo.Name} ...");
                if (!File.Exists(destination))
                    pair.FileInfo.CopyTo(destination);
                fileCount++;
            }

            string notFoundFilesCsvFilename = null;

            if (pairsNotFound.Count > 0)
            {
                notFoundFilesCsvFilename = Path.Join(targetDirPath, string.Format("NotFound_{0}_{1:yyyyMMdd_HHmmss}.csv", (new FileInfo(csvPath)).Name, DateTime.Now));
                File.WriteAllLines(notFoundFilesCsvFilename, pairsNotFound.Select(p => $"\"{p.Filename}\",\"{p.Label}\""));
            }

            Console.Write(Environment.NewLine);

            Console.WriteLine($"Files copied: {fileCount}");

            if (!string.IsNullOrEmpty(notFoundFilesCsvFilename))
            {
                Console.WriteLine($"List of files not found: {notFoundFilesCsvFilename}");
            }

            Console.Write(Environment.NewLine);

            Console.WriteLine("Thank you for using OFFLINE IMAGE QUERY AND EXTRACTION TOOL");
            Console.WriteLine("Concept by Janna Joceli Omena");
            Console.WriteLine("Development by Jason Chao");
            Console.WriteLine("https://github.com/jason-chao/offline-image-query");

            Console.Write(Environment.NewLine);

            exit();
        }

        static string trimQuotationMarks(string path, string quotationMark = "\"")
        {
            if (path.StartsWith(quotationMark))
                path = path.Substring(1);

            if (path.EndsWith(quotationMark))
                path = path.Remove(path.Length - 1);

            return path;
        }

        static string cleanPath(string path)
        {
            path = path.Trim();

            List<string> invalidPathCharacters = Path.GetInvalidPathChars().Select(c => c.ToString()).ToList();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                invalidPathCharacters.Add("\\");

            foreach (var ch in invalidPathCharacters)
            {
                if (path.Contains(ch))
                    path = path.Replace(ch, string.Empty);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                path = trimQuotationMarks(path, "\"");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                path = trimQuotationMarks(path, "'");

            return path;

        }
    }
}
