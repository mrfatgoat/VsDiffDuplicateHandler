using VsDiffDuplicateHandler.Models;

namespace VsDiffDuplicateHandler.Services
{
    public interface IFileModifier
    {
        void Delete(GroupFile groupFile);
        void Move(GroupFile groupFile, string dest);
    }
}
