using System.Xml.Linq;

namespace VsDiffDuplicateHandler.Services.Interfaces
{
    public interface IFileOperationsAbstraction
    {
        void DeleteFile(string filePath);
        void MoveFile(string filePath, string destFolder);
        XDocument LoadXml(string filePath);
    }
}
