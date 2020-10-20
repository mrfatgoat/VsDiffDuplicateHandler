using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Services.Interfaces;

namespace VsDiffDuplicateHandler.Services
{
    public class DuplicateHandlerApp : IDuplicateHandlerApp
    {
        private readonly IDuplicateHandlerConfiguration _config;
        private readonly IDuplicateProcessor _duplicateProcessor;
        private readonly IFileOperationsAbstraction _fileOps;


        public DuplicateHandlerApp(
            IDuplicateHandlerConfiguration config,
            IDuplicateProcessor duplicateProcessor,
            IFileOperationsAbstraction fileOps)
        {
            _config = config;
            _duplicateProcessor = duplicateProcessor;
            _fileOps = fileOps;
        }


        public void Run()
        {
            // TODO: If anything fails, don't delete the dupe file.
            //       Provide a means of resuming???


            // Process duplicates
            _duplicateProcessor.ProcessDuplicates();

            if (!_config.DryRun)
            {
                // Delete the duplicates file
                _fileOps.DeleteFile(_config.DuplicateFilePath);
            }
        }
    }
}