﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Xml.Linq;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Models;
using VsDiffDuplicateHandler.Services.Interfaces;

namespace VsDiffDuplicateHandler.Services
{
    public class XmlDuplicateReader : IDuplicateReader
    {
        private readonly IFileOperationsAbstraction _fileOps;
        private readonly IDuplicateHandlerConfiguration _config;


        public XmlDuplicateReader(IDuplicateHandlerConfiguration config, IFileOperationsAbstraction fileOps)
        {
            _fileOps = fileOps;
            _config = config;
        }


        public bool CanHandle(IFileInfo fileInfo) => fileInfo.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase);


        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();


        public IEnumerator<DuplicateGroup> GetEnumerator()
        {
            XDocument xdoc = _fileOps.LoadXml(_config.DuplicateFilePath);

            // Get all the duplicate groups
            IEnumerable<XElement> xmlGroups = xdoc.Descendants("G");

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
    }
}
