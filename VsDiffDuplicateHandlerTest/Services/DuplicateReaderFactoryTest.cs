using NSubstitute;
using System;
using System.IO.Abstractions;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Services;
using Xunit;

namespace VsDiffDuplicateHandlerTest.Services
{
    public class DuplicateReaderFactoryTest
    {
        [Fact]
        public void CreateReaderChoosesCorrectReader()
        {
            // Arrange
            IFileSystem fs = Substitute.For<IFileSystem>();
            IFileInfoFactory fileInfoFactory = Substitute.For<IFileInfoFactory>();
            fileInfoFactory.FromFileName(Arg.Any<string>())
                .ReturnsForAnyArgs(Substitute.For<FileInfoBase>());
            fs.FileInfo.ReturnsForAnyArgs(fileInfoFactory);

            IDuplicateReader correctReader = Substitute.For<IDuplicateReader>();
            correctReader.CanHandle(Arg.Any<FileInfoBase>()).Returns(true);
            IDuplicateReader incorrectReader1 = Substitute.For<IDuplicateReader>();
            incorrectReader1.CanHandle(Arg.Any<FileInfoBase>()).Returns(false);
            IDuplicateReader incorrectReader2 = Substitute.For<IDuplicateReader>();
            incorrectReader2.CanHandle(Arg.Any<FileInfoBase>()).Returns(false);

            DuplicateReaderFactory uut = new DuplicateReaderFactory(
                config: Substitute.For<IDuplicateHandlerConfiguration>(),
                fileSystem: fs,
                readers: new[] { incorrectReader1, correctReader, incorrectReader2 });

            // Act
            IDuplicateReader selectedReader = uut.CreateReader();

            // Assert
            Assert.Same(correctReader, selectedReader);
        }


        [Fact]
        public void CreateReaderThrowsIfNoReadersCanHandleFile()
        {
            // Arrange
            IFileSystem fs = Substitute.For<IFileSystem>();
            IFileInfoFactory fileInfoFactory = Substitute.For<IFileInfoFactory>();
            fileInfoFactory.FromFileName(Arg.Any<string>())
                .ReturnsForAnyArgs(Substitute.For<FileInfoBase>());
            fs.FileInfo.ReturnsForAnyArgs(fileInfoFactory);

            IDuplicateReader incorrectReader1 = Substitute.For<IDuplicateReader>();
            incorrectReader1.CanHandle(Arg.Any<FileInfoBase>()).Returns(false);
            IDuplicateReader incorrectReader2 = Substitute.For<IDuplicateReader>();
            incorrectReader2.CanHandle(Arg.Any<FileInfoBase>()).Returns(false);

            DuplicateReaderFactory uut = new DuplicateReaderFactory(
                config: Substitute.For<IDuplicateHandlerConfiguration>(),
                fileSystem: fs,
                readers: new[] { incorrectReader1, incorrectReader2 });

            // Act & Assert
            Assert.Throws<ArgumentException>(() => uut.CreateReader());
        }
    }
}
