using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Models;

namespace VsDiffDuplicateHandler.Services
{
    public class CsvDuplicateReader : IDuplicateReader
    {
        private readonly IDuplicateHandlerConfiguration _config;

        public CsvDuplicateReader(IDuplicateHandlerConfiguration config)
        {
            _config = config;
        }

        public bool CanHandle(FileInfoBase fileInfo) => fileInfo.Extension.Equals("csv", StringComparison.CurrentCultureIgnoreCase);

        public IEnumerator<DuplicateGroup> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
