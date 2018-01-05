﻿using NSubstitute;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Models;
using Xunit;

namespace VsDiffDuplicateHandler.Services
{
    public class XmlDuplicateReaderTest
    {
        [Theory]
        [InlineData("xml", true)]
        [InlineData("XML", true)]
        [InlineData("abc", false)]
        public void CanHandleReturnsTrueForCorrectFileExtensionsOnly(string fileExtension, bool shouldHandle)
        {
            // Arrange
            FileInfoBase fileInfo = Substitute.For<FileInfoBase>();
            fileInfo.Extension.Returns(fileExtension);

            XmlDuplicateReader uut = new XmlDuplicateReader(
                config: Substitute.For<IDuplicateHandlerConfiguration>(),
                xmlLoader: Substitute.For<IXmlLoader>());

            // Act
            bool canHandle = uut.CanHandle(fileInfo);

            // Assert
            Assert.Equal(shouldHandle, canHandle);
        }


        [Fact]
        public void GetEnumeratorGetsCorrectNumberOfGroups()
        {
            // Arrange
            XDocument xdoc = new XDocument(
                new XElement("root",
                    this.ArrangeGroup(),
                    this.ArrangeGroup(),
                    this.ArrangeGroup()));

            IXmlLoader xmlLoader = Substitute.For<IXmlLoader>();
            xmlLoader.Load(Arg.Any<string>()).Returns(xdoc);

            XmlDuplicateReader uut = new XmlDuplicateReader(
                config: Substitute.For<IDuplicateHandlerConfiguration>(),
                xmlLoader: xmlLoader);

            // Act & Assert
            Assert.Equal(3, uut.Count());
        }

        [Fact]
        public void GetEnumeratorGetsCorrectFilesInGroup()
        {
            // Arrange
            XDocument xdoc = new XDocument(
                new XElement("root",
                    this.ArrangeGroup(
                        this.ArrangeImage(true, "file1"),
                        this.ArrangeImage(false, "file2"))));

            IXmlLoader xmlLoader = Substitute.For<IXmlLoader>();
            xmlLoader.Load(Arg.Any<string>()).Returns(xdoc);

            XmlDuplicateReader uut = new XmlDuplicateReader(
                config: Substitute.For<IDuplicateHandlerConfiguration>(),
                xmlLoader: xmlLoader);

            // Act
            DuplicateGroup group = Assert.Single(uut);
            Assert.Equal(2, group.Files.Count());
            GroupFile file1 = Assert.Single(group.Files, f => f.FullName == "file1" && f.Checked == true);
            GroupFile file2 = Assert.Single(group.Files, f => f.FullName == "file2" && f.Checked == false);
        }

        private XElement ArrangeImage(bool isChecked, string fileName)
        {
            XElement imageElement = new XElement("Image");
            imageElement.SetAttributeValue("Checked", isChecked ? "1" : "0");
            imageElement.SetAttributeValue("FileName", fileName);
            return imageElement;
        }

        private XElement ArrangeGroup(params XElement[] images)
        {
            return new XElement("Group", images);
        }
    }
}