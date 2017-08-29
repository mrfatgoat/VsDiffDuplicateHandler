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
                Console.WriteLine("Expected an XML file, \"good\" base path, and \"staged\" base path.");
                return;
            }

            string xmlPath = args[0];
            _goodBasePath = args[1];

            if (!File.Exists(xmlPath))
            {
                Console.WriteLine($@"file ""{xmlPath}"" does not exist.");
                return;
            }

            if (!Directory.Exists(_goodBasePath))
            {
                Console.WriteLine($"{nameof(_goodBasePath)} does not exist.");
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

            Console.ReadLine();
            return;
        }

        private static XDocument LoadXml(string xmlPath)
        {
            using (FileStream fs = File.OpenRead(xmlPath))
            {
                XDocument xdoc = XDocument.Load(fs);
                return xdoc;
            }
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
            IEnumerable<XElement> goodFiles = images.Where(i => IsGoodPath(i.Attribute("FileName").Value));
            IEnumerable<XElement> stagedFiles = images.Except(goodFiles);
            IEnumerable<XElement> checkedFiles = images.Where(i => i.Attribute("Checked").Value != "0");
            IEnumerable<XElement> keepFiles = images.Except(checkedFiles);

            // Delete the checked files
            foreach (XElement checkedFile in checkedFiles)
                FileSystem.DeleteFile(checkedFile.Attribute("FileName").Value, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

            XElement[] filesToMove = stagedFiles.Intersect(keepFiles).ToArray();

            if (filesToMove.Any())
            {
                string destinationDir = goodFiles
                    .Select(i => Path.GetDirectoryName(i.Attribute("FileName").Value))
                    .Distinct()
                    .SingleOrDefault();

                if (!String.IsNullOrWhiteSpace(destinationDir))
                {
                    foreach (XElement keeper in filesToMove)
                    {
                        // Create the destination directory
                        string currentPath = keeper.Attribute("FileName").Value;
                        string destPath = Path.Combine(destinationDir, Path.GetFileName(currentPath));
                        File.Move(currentPath, destPath);
                    }
                }
            }
        }

        private static bool IsGoodPath(string filePath)
        {
            bool isGood = Path.GetDirectoryName(filePath)
                .StartsWith(_goodBasePath, StringComparison.OrdinalIgnoreCase);

            return isGood;
        }
    }
}
