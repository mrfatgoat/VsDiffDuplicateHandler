using LINQtoCSV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Models;
using VsDiffDuplicateHandler.Services.Interfaces;

namespace VsDiffDuplicateHandler.Services
{
    public class CsvDuplicateReader : IDuplicateReader
    {
        private readonly IDuplicateHandlerConfiguration _config;
        private readonly IFileSystem _fileSystem;


        public CsvDuplicateReader(IDuplicateHandlerConfiguration config, IFileSystem fileSystem)
        {
            _config = config;
            _fileSystem = fileSystem;
        }


        public bool CanHandle(IFileInfo fileInfo) => fileInfo.Extension.Equals(".csv", StringComparison.CurrentCultureIgnoreCase);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();


        public IEnumerator<DuplicateGroup> GetEnumerator()
        {
            CsvFileDescription csvDescription = new CsvFileDescription()
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                IgnoreUnknownColumns = true
            };

            CsvContext csvContext = new CsvContext();

            using (StreamReader csvStreamReader = _fileSystem.File.OpenText(_config.DuplicateFilePath))
            {
                IEnumerable<DuplicateGroup> dupGroups = csvContext.Read<CsvDuplicateRecord>(csvStreamReader, csvDescription)
                    .GroupBy(rec => rec.Group)
                    .Select(recGroup => new DuplicateGroup()
                    {
                        Files = recGroup.Select(rec => new GroupFile()
                        {
                            FullName = rec.FileName,
                            Checked = rec.Checked > 0
                        }).ToList()
                    });

                foreach (DuplicateGroup dupGroup in dupGroups)
                    yield return dupGroup;
            }
        }
    }
}
