using System.Collections.Generic;

namespace VsDiffDuplicateHandler.Models
{
    public class DuplicateGroup
    {
        public IEnumerable<GroupFile> Files { get; set; }
    }
}
