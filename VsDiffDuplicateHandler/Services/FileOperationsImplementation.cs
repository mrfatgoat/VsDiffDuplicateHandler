using VsDiffDuplicateHandler.Services.Interfaces;
using IOAbstractions = System.IO.Abstractions;
using VBIO = Microsoft.VisualBasic.FileIO;

namespace VsDiffDuplicateHandler.Services
{
    public class FileOperationsImplementation : IFileOperationsAbstraction
    {
        private IOAbstractions.IFileSystem _fileSystem;


        public FileOperationsImplementation(IOAbstractions.IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }


        public void Delete(string filePath)
        {
            VBIO.FileSystem.DeleteFile(filePath, VBIO.UIOption.OnlyErrorDialogs, VBIO.RecycleOption.SendToRecycleBin);
        }


        public void Move(string filePath, string destFolder)
        {
            _fileSystem.File.Move(filePath, destFolder);
        }
    }
}
