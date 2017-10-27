using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace VsDiffDuplicateHandler
{
    class Program
    {
        private static string _goodBasePath;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Expected an XML file and \"good\" base path.");
                return;
            }

            string xmlPath = args[0];
            _goodBasePath = args[1];

            if (!File.Exists(xmlPath))
            {
                Console.WriteLine($@"File ""{xmlPath}"" does not exist.");
                return;
            }

            if (!Directory.Exists(_goodBasePath))
            {
                Console.WriteLine("\"Good\" base path does not exist.");
                return;
            }

            XDocument xdoc = null;

            try
            {
                xdoc = LoadXml(args[0]);
            }
            catch
            {
                Console.WriteLine($@"Could not read ""{xmlPath}"" as XML.");
                return;
            }

            ProcessDupeFile(xdoc);

            FileSystem.DeleteFile(xmlPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
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

        // TODO: Abstract this functionality
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
            bool isGood = Path.GetDirectoryName(filePath)
                .StartsWith(_goodBasePath, StringComparison.OrdinalIgnoreCase);

            return isGood;
        }
        
        private static void DeleteFile(string file)
        {
            try
            {
                FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception deleting {file}");
                Console.WriteLine(ex.Message);
            }
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
        }
        
        #endregion
    }
}
