using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Models;

namespace VsDiffDuplicateHandler.Services
{
    public class DuplicateProcessor : IDuplicateProcessor
    {
        private readonly IDuplicateReaderFactory _readerFactory;
        private readonly IFileModifier _fileModifier;
        private readonly IDuplicateHandlerConfiguration _config;
        private readonly IFileSystem _fileSystem;

        public DuplicateProcessor(
            IDuplicateReaderFactory readerFactory, 
            IFileModifier fileModifier,
            IDuplicateHandlerConfiguration config,
            IFileSystem fileSystem)
        {
            _readerFactory = readerFactory;
            _fileModifier = fileModifier;
            _config = config;
            _fileSystem = fileSystem;
        }


        public void ProcessDuplicates()
        {
            IDuplicateReader dupReader = _readerFactory.CreateReader();
            
            foreach (DuplicateGroup group in dupReader)
            {
                GroupFile[] checkedFiles = group.Files.Where(f => f.Checked == true).ToArray();

                // If there are no checked files, we're done
                if (!checkedFiles.Any())
                    continue;

                GroupFile[] goodFiles = group.Files
                    .Where(f => IsGoodPath(f.FullName))
                    .ToArray();

                IEnumerable<GroupFile> stagedFiles = group.Files.Except(goodFiles);
                IEnumerable<GroupFile> keepFiles = group.Files.Except(checkedFiles);

                // Delete the checked files
                foreach (GroupFile checkedFile in checkedFiles)
                    _fileModifier.Delete(checkedFile);

                // Isolate the files we intend to move
                GroupFile[] filesToMove = stagedFiles.Intersect(keepFiles).ToArray();

                // If there's nothing to move, we're done
                if (!filesToMove.Any())
                    continue;

                // Figure out where the files are moving to
                IEnumerable<string> goodDirectories = goodFiles
                    .Select(f => _fileSystem.Path.GetDirectoryName(f.FullName))
                    .Distinct();

                // If there isn't exacty one destination, we don't know where the files go
                if (goodDirectories.Count() != 1)
                    continue;

                string destinationDir = goodDirectories.Single();

                // Move each file
                foreach (GroupFile keeper in filesToMove)
                {
                    // Build the destination directory
                    string destPath = _fileSystem.Path.Combine(destinationDir, _fileSystem.Path.GetFileName(keeper.FullName));
                    _fileModifier.Move(keeper, destPath);
                }
            }
        }

        private bool IsGoodPath(string filePath)
        {
            bool isGood = _fileSystem.Path.GetDirectoryName(filePath)
                .StartsWith(_config.GoodPath, StringComparison.OrdinalIgnoreCase);

            return isGood;
        }
    }
}
