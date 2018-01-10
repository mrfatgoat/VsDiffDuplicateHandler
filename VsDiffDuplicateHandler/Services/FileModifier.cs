using Microsoft.Extensions.Logging;
using VBIO = Microsoft.VisualBasic.FileIO;
using System;
using VsDiffDuplicateHandler.Models;
using VsDiffDuplicateHandler.Services.Interfaces;
using System.IO.Abstractions;

namespace VsDiffDuplicateHandler.Services
{
    public class FileModifier : IFileModifier
    {
        private readonly ILogger<FileModifier> _logger;
        private readonly IFileSystem _fileSystem;


        public FileModifier(ILogger<FileModifier> logger, IFileSystem fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }


        public void Delete(GroupFile groupFile)
        {
            try
            {
                VBIO.FileSystem.DeleteFile(groupFile.FullName, VBIO.UIOption.OnlyErrorDialogs, VBIO.RecycleOption.SendToRecycleBin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EXCEPTION DELETING {groupFile.FullName}.");
            }

            _logger.LogInformation($"DELETED {groupFile.FullName} to the recycle bin.");
        }


        public void Move(GroupFile groupFile, string dest)
        {
            try
            {
                _fileSystem.File.Move(groupFile.FullName, dest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception moving {groupFile.FullName}");
            }

            _logger.LogInformation($"MOVED {groupFile.FullName} to {dest}.");
        }
    }
}
