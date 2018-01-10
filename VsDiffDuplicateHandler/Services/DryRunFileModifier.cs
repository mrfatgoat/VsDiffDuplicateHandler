using Microsoft.Extensions.Logging;
using VsDiffDuplicateHandler.Models;
using VsDiffDuplicateHandler.Services.Interfaces;

namespace VsDiffDuplicateHandler.Services
{
    public class DryRunFileModifier : IFileModifier
    {
        private readonly ILogger<DryRunFileModifier> _logger;


        public DryRunFileModifier(ILogger<DryRunFileModifier> logger)
        {
            _logger = logger;
        }


        public void Delete(GroupFile groupFile)
        {
            _logger.LogInformation($"WOULD DELETE: {groupFile.FullName}");
        }


        public void Move(GroupFile groupFile, string dest)
        {
            _logger.LogInformation($"WOULD MOVE: {groupFile.FullName} to {dest}");
        }
    }
}
