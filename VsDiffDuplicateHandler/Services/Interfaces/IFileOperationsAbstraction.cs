namespace VsDiffDuplicateHandler.Services.Interfaces
{
    public interface IFileOperationsAbstraction
    {
        void Delete(string filePath);
        void Move(string filePath, string destFolder);
    }
}
