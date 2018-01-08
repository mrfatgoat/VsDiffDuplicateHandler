using NSubstitute;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Models;
using VsDiffDuplicateHandler.Services;
using Xunit;

namespace VsDiffDuplicateHandlerTest.Services
{
    public class CsvDuplicateReaderTest
    {
        [Theory]
        [InlineData(".csv", true)]
        [InlineData(".CSV", true)]
        [InlineData(".xyz", false)]
        public void CanHandleReturnsTrueForCorrectFileExtensionsOnly(string fileExtension, bool shouldHandle)
        {
            // Arrange
            FileInfoBase fileInfo = Substitute.For<FileInfoBase>();
            fileInfo.Extension.Returns(fileExtension);

            CsvDuplicateReader uut = new CsvDuplicateReader(
                config: Substitute.For<IDuplicateHandlerConfiguration>(),
                fileSystem: Substitute.For<IFileSystem>());

            // Act
            bool canHandle = uut.CanHandle(fileInfo);

            // Assert
            Assert.Equal(shouldHandle, canHandle);
        }


        [Fact]
        public void GetEnumeratorGetsCorrectNumberOfGroups()
        {
            // Arrange
            IFileSystem fs = this.ArrangeFileSystemForCsvData(
                new CsvDuplicateRecord() { Group = 1, FileName = "", Checked = 0 },
                new CsvDuplicateRecord() { Group = 2, FileName = "", Checked = 0 },
                new CsvDuplicateRecord() { Group = 3, FileName = "", Checked = 0 });
            
            CsvDuplicateReader uut = new CsvDuplicateReader(
                config: Substitute.For<IDuplicateHandlerConfiguration>(),
                fileSystem: fs);

            // Act & Assert
            Assert.Equal(3, uut.Count());
        }


        [Fact]
        public void GetEnumeratorGetsCorrectFilesInGroup()
        {
            // Arrange
            IFileSystem fs = this.ArrangeFileSystemForCsvData(
                new CsvDuplicateRecord() { Group = 1, FileName = "file1", Checked = 0 },
                new CsvDuplicateRecord() { Group = 1, FileName = "file2", Checked = 0 });

            // Act
            CsvDuplicateReader uut = new CsvDuplicateReader(
                config: Substitute.For<IDuplicateHandlerConfiguration>(),
                fileSystem: fs);

            // Assert
            DuplicateGroup group = Assert.Single(uut);
            Assert.Equal(2, group.Files.Count());
            GroupFile file1 = Assert.Single(group.Files, f => f.FullName == "file1");
            GroupFile file2 = Assert.Single(group.Files, f => f.FullName == "file2");
        }


        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        public void CheckedValueResolvesToCorrectBoolean(int checkedValue, bool isChecked)
        {
            // Arrange
            IFileSystem fs = this.ArrangeFileSystemForCsvData(
                new CsvDuplicateRecord() { Group = 1, FileName = "file1", Checked = checkedValue });

            // Act
            CsvDuplicateReader uut = new CsvDuplicateReader(
                config: Substitute.For<IDuplicateHandlerConfiguration>(),
                fileSystem: fs);

            // Assert
            DuplicateGroup group = Assert.Single(uut);
            GroupFile file = Assert.Single(group.Files);
            Assert.Equal(isChecked, file.Checked);
        }


        private StreamReader ArrangeCsvStreamReader(params CsvDuplicateRecord[] csvRecords)
        {
            MemoryStream ms = new MemoryStream();

            StreamWriter sw = new StreamWriter(ms);
            sw.Write("Group,FileName,Checked\n");
            foreach (CsvDuplicateRecord r in csvRecords)
                sw.Write($"{r.Group},{r.FileName},{r.Checked}\n");

            sw.Flush();

            ms.Position = 0;
            return new StreamReader(ms);
        }

        private IFileSystem ArrangeFileSystemForCsvData(params CsvDuplicateRecord[] csvRecords)
        {
            StreamReader csvStreamReader = this.ArrangeCsvStreamReader(csvRecords);

            IFileSystem fs = Substitute.For<IFileSystem>();
            fs.File.Returns(Substitute.For<FileBase>());
            fs.File.OpenText(Arg.Any<string>()).ReturnsForAnyArgs(csvStreamReader);

            return fs;
        }
    }
}
