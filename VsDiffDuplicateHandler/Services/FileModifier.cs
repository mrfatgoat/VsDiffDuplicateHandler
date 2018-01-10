using Microsoft.Extensions.Logging;
using System;
using VsDiffDuplicateHandler.Models;
using VsDiffDuplicateHandler.Services.Interfaces;

namespace VsDiffDuplicateHandler.Services
{
    public class FileModifier : IFileModifier
    {
        private readonly ILogger<FileModifier> _logger;
        private readonly IFileOperationsAbstraction _fileOps;


        public FileModifier(ILogger<FileModifier> logger, IFileOperationsAbstraction fileOps)
        {
            _logger = logger;
            _fileOps = fileOps;
        }


        public void Delete(GroupFile groupFile)
        {
            try
            {
                _fileOps.Delete(groupFile.FullName);
                _logger.LogInformation($"DELETED {groupFile.FullName} to the recycle bin.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EXCEPTION DELETING {groupFile.FullName}.");
            }
        }


        public void Move(GroupFile groupFile, string dest)
        {
            try
            {
                _fileOps.Move(groupFile.FullName, dest);
                _logger.LogInformation($"MOVED {groupFile.FullName} to {dest}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception moving {groupFile.FullName}");
            }
        }
    }
}
