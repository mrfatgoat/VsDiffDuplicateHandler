using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Xml.Linq;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Models;

namespace VsDiffDuplicateHandler.Services
{
    public class XmlDuplicateReader : IDuplicateReader
    {
        private readonly IXmlLoader _xmlLoader;
        private readonly IDuplicateHandlerConfiguration _config;

        public XmlDuplicateReader(IDuplicateHandlerConfiguration config, IXmlLoader xmlLoader)
        {
            _xmlLoader = xmlLoader;
            _config = config;
        }

        public bool CanHandle(FileInfoBase fileInfo) => fileInfo.Extension.Equals("xml", StringComparison.OrdinalIgnoreCase);

        public IEnumerator<DuplicateGroup> GetEnumerator()
        {
            XDocument xdoc = _xmlLoader.Load(_config.DuplicateFilePath);

            // Get all the duplicate groups
            IEnumerable<XElement> xmlGroups = xdoc.Descendants("Group");

            // Convert each group to a model
            foreach (XElement xmlGroup in xmlGroups)
            {
                DuplicateGroup duplicateGroup = new DuplicateGroup();
                List<GroupFile> groupFiles = new List<GroupFile>();

                IEnumerable<XElement> xmlImages = xmlGroup.Descendants("Image");
                foreach (XElement xmlImage in xmlImages)
                {
                    GroupFile groupFile = new GroupFile()
                    {
                        Checked = xmlImage.Attribute("Checked").Value != "0",
                        FullName = xmlImage.Attribute("FileName").Value
                    };

                    groupFiles.Add(groupFile);
                }

                duplicateGroup.Files = groupFiles;
                yield return duplicateGroup;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
