using System;
using VsDiffDuplicateHandler.Models;
using VsDiffDuplicateHandler.Services.Interfaces;

namespace VsDiffDuplicateHandler.Services
{
    public class DryRunFileModifier : IFileModifier
    {
        public void Delete(GroupFile groupFile)
        {
            // TODO: abstract logging
            Console.WriteLine($"WOULD DELETE: {groupFile.FullName}");
        }


        public void Move(GroupFile groupFile, string dest)
        {
            Console.WriteLine($"WOULD MOVE: {groupFile.FullName} to {dest}");
        }
    }
}
