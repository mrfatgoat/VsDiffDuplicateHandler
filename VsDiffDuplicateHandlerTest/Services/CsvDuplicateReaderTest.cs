using NSubstitute;
using System.IO.Abstractions;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Services;
using Xunit;

namespace VsDiffDuplicateHandlerTest.Services
{
    public class CsvDuplicateReaderTest
    {
        [Theory]
        [InlineData("csv", true)]
        [InlineData("CSV", true)]
        [InlineData("xyz", false)]
        public void CanHandleReturnsTrueForCorrectFileExtensionsOnly(string fileExtension, bool shouldHandle)
        {
            // Arrange
            FileInfoBase fileInfo = Substitute.For<FileInfoBase>();
            fileInfo.Extension.Returns(fileExtension);

            CsvDuplicateReader uut = new CsvDuplicateReader(
                config: Substitute.For<IDuplicateHandlerConfiguration>());

            // Act
            bool canHandle = uut.CanHandle(fileInfo);

            // Assert
            Assert.Equal(shouldHandle, canHandle);
        }
    }
}
