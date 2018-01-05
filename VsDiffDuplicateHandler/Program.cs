using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Services;

namespace VsDiffDuplicateHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            // [X] Validate arguments
            // [X] Create configuration object
            // [ ] Populate DI container
            // [ ] Retrieve the duplicate processor
            // [ ] Process the duplicates

            IDuplicateHandlerConfiguration config = AssertValidArguments(args);

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IDuplicateHandlerConfiguration>(config);
            services.AddSingleton<IDuplicateReaderFactory, DuplicateReaderFactory>();
            services.AddSingleton<IDuplicateReader, XmlDuplicateReader>();
            services.AddSingleton<System.IO.Abstractions.IFileSystem, System.IO.Abstractions.FileSystem>();




            return;

            string dupFilePath;

            XDocument xdoc = null;

            try
            {
                xdoc = LoadXml(args[0]);
            }
            catch
            {
                Console.WriteLine($@"Could not read ""{dupFilePath}"" as XML.");
                return;
            }

            ProcessDupeFile(xdoc);

            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(dupFilePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }

        private static void ProcessDupeFile(XDocument xdoc)
        {
            // Get all the duplicate groups
            IEnumerable<XElement> groups = xdoc.Descendants("Group");

            // Process each group
            foreach (XElement group in groups)
                ProcessGroup(group);
        }

        private static void ProcessGroup(XElement group)
        {
            XElement[] images = group.Descendants("Image").ToArray();

            XElement[] checkedFiles = images
                .Where(i => i.Attribute("Checked").Value != "0")
                .ToArray();

            // If there are no checked files, leave everything alone
            if (!checkedFiles.Any())
                return;

            XElement[] goodFiles = images
                .Where(i => IsGoodPath(i.Attribute("FileName").Value))
                .ToArray();
            IEnumerable<XElement> stagedFiles = images.Except(goodFiles);            
            IEnumerable<XElement> keepFiles = images.Except(checkedFiles);

            // Delete the checked files
            foreach (XElement checkedFile in checkedFiles)
                DeleteFile(checkedFile.Attribute("FileName").Value);

            // Isolate the files we intend to move
            XElement[] filesToMove = stagedFiles.Intersect(keepFiles).ToArray();

            // If there's nothing to move, we're done
            if (!filesToMove.Any())
                return;

            // Figure out where the files are moving to
            IEnumerable<string> goodDirectories = goodFiles
                .Select(i => Path.GetDirectoryName(i.Attribute("FileName").Value))
                .Distinct();

            // If there isn't exacty one destination, we don't know where the files go
            if (goodDirectories.Count() != 1)
                return;

            string destinationDir = goodDirectories.Single();

            // Move each file
            foreach (XElement keeper in filesToMove)
            {
                // Build the destination directory
                string currentPath = keeper.Attribute("FileName").Value;
                string destPath = Path.Combine(destinationDir, Path.GetFileName(currentPath));
                MoveFile(currentPath, destPath);
            }
        }

        #region File Operations

        // TODO: Abstract file ops
        private static XDocument LoadXml(string xmlPath)
        {
            using (FileStream fs = File.OpenRead(xmlPath))
            {
                XDocument xdoc = XDocument.Load(fs);
                return xdoc;
            }
        }
        
        private static bool IsGoodPath(string filePath)
        {
            // TEMP
            string _goodBasePath = String.Empty;

            bool isGood = Path.GetDirectoryName(filePath)
                .StartsWith(_goodBasePath, StringComparison.OrdinalIgnoreCase);

            return isGood;
        }
        
        private static void DeleteFile(string file)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception deleting {file}");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine($"DELETED {file}");
        }

        private static void MoveFile(string file, string dest)
        {
            try
            {
                File.Move(file, dest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception moving {file}");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine($"MOVED {file} to {Path.GetDirectoryName(dest)}");
        }
        
        #endregion

        private static IDuplicateHandlerConfiguration AssertValidArguments(string[] args)
        {
            if (args.Length < 2)
            {
                // TODO: Abstract logging
                Console.WriteLine("Expected a \"duplicates\" file and \"good\" base path.");
                throw new ArgumentException();
            }

            bool dryrun = 
                args.Length == 3 && 
                args[2].Equals("--dryrun", StringComparison.OrdinalIgnoreCase);

            return new DuplicateHandlerConfiguration()
            {
                DuplicateFilePath = args[0],
                GoodPath = args[1],
                DryRun = dryrun
            };

            // TODO: Abstract file operations
            //if (!File.Exists(dupFilePath))
            //{
            //    Console.WriteLine($@"File ""{dupFilePath}"" does not exist.");
            //}

            //if (!Directory.Exists(_goodBasePath))
            //{
            //    Console.WriteLine("\"Good\" base path does not exist.");
            //}
        }
    }
}
