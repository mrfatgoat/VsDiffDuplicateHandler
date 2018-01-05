using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using VsDiffDuplicateHandler.Configuration;

namespace VsDiffDuplicateHandler.Services
{
    public class DuplicateReaderFactory : IDuplicateReaderFactory
    {
        private readonly IDuplicateHandlerConfiguration _config;
        private readonly IEnumerable<IDuplicateReader> _readers;
        private readonly IFileSystem _fileSystem;

        public DuplicateReaderFactory(
            IDuplicateHandlerConfiguration config, 
            IEnumerable<IDuplicateReader> readers,
            IFileSystem fileSystem)
        {
            _config = config;
            _readers = readers;
            _fileSystem = fileSystem;
        }

        public IDuplicateReader CreateReader()
        {
            FileInfoBase dupFileInfo = _fileSystem.FileInfo.FromFileName(_config.DuplicateFilePath);
            IDuplicateReader correctReader = _readers.SingleOrDefault(r => r.CanHandle(dupFileInfo));
            return correctReader ?? throw new ArgumentException($"Could not find a compatible reader for \"{dupFileInfo.FullName}\".");
        }
    }
}
