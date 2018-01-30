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
            // Process duplicates
            _duplicateProcessor.ProcessDuplicates();

            // Delete the duplicates file
            _fileOps.DeleteFile(_config.DuplicateFilePath);
        }
    }
}
