using NSubstitute;
using System.Collections.Generic;
using System.IO.Abstractions;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Models;
using VsDiffDuplicateHandler.Services;
using VsDiffDuplicateHandler.Services.Interfaces;
using Xunit;

namespace VsDiffDuplicateHandlerTest.Services
{
    public class DuplicateProcessorTest
    {
        [Fact]
        public void NoModificationsIfNoCheckedFiles()
        {
            // Arrange
            IDuplicateReaderFactory readerFactory = this.ArrangeReaderFactoryForGroups(
                this.ArrangeGroup(
                    new GroupFile() { Checked = false },
                    new GroupFile() { Checked = false }));

            IFileModifier fileModifier = Substitute.For<IFileModifier>();

            DuplicateProcessor uut = this.DuplicateProcessorWithDefaultMocks(
                readerFactory: readerFactory,
                fileModifier: fileModifier);

            // Act
            uut.ProcessDuplicates();

            // Assert
            fileModifier.DidNotReceive().Delete(Arg.Any<GroupFile>());
            fileModifier.DidNotReceive().Move(Arg.Any<GroupFile>(), Arg.Any<string>());
        }


        [Fact]
        public void CheckedFilesAreDeleted()
        {
            // Arrange
            GroupFile deleteThis1 = new GroupFile() { FullName = @"c:\folder\delete1.png", Checked = true };
            GroupFile deleteThis2 = new GroupFile() { FullName = @"c:\somewhere\delete2.jpg", Checked = true };

            IDuplicateReaderFactory readerFactory = this.ArrangeReaderFactoryForGroups(
                this.ArrangeGroup(deleteThis1, deleteThis2));

            IFileModifier fileModifer = Substitute.For<IFileModifier>();

            DuplicateProcessor uut = this.DuplicateProcessorWithDefaultMocks(
                readerFactory: readerFactory,
                fileModifier: fileModifer);

            // Act
            uut.ProcessDuplicates();

            // Assert
            fileModifer.Received().Delete(deleteThis1);
            fileModifer.Received().Delete(deleteThis2);
        }


        [Fact]
        public void StagedFilesReplaceCheckedGoodFile()
        {
            // Arrange
            IDuplicateHandlerConfiguration config = Substitute.For<IDuplicateHandlerConfiguration>();
            config.GoodPath.Returns(@"c:\good");

            GroupFile goodFile = new GroupFile() { FullName = config.GoodPath + @"\decent.jpg", Checked = true };
            GroupFile stagedFile1 = new GroupFile() { FullName = @"c:\staged\better.png", Checked = false };
            GroupFile stagedFile2 = new GroupFile() { FullName = @"c:\staged\better_v2.png", Checked = false };

            IDuplicateReaderFactory readerFactory = this.ArrangeReaderFactoryForGroups(
                this.ArrangeGroup(goodFile, stagedFile1, stagedFile2));

            IFileModifier fileModifier = Substitute.For<IFileModifier>();

            DuplicateProcessor uut = this.DuplicateProcessorWithDefaultMocks(
                config: config,
                fileModifier: fileModifier,
                readerFactory: readerFactory);

            // Act
            uut.ProcessDuplicates();

            // Assert
            fileModifier.Received(1).Delete(goodFile);
            fileModifier.Received(1).Move(stagedFile1, config.GoodPath);
            fileModifier.Received(1).Move(stagedFile2, config.GoodPath);
        }


        [Fact]
        public void StagedFileIsNotMovedIfMultipleGoodPathsFound()
        {
            // Arrange
            IDuplicateHandlerConfiguration config = Substitute.For<IDuplicateHandlerConfiguration>();
            config.GoodPath.Returns(@"c:\good");

            GroupFile goodFile1 = new GroupFile() { FullName = config.GoodPath + @"\folder1\decent.jpg", Checked = true };
            GroupFile goodFile2 = new GroupFile() { FullName = config.GoodPath + @"\folder2\ok.jpg", Checked = true };
            GroupFile stagedFile = new GroupFile() { FullName = @"c:\staged\better.png", Checked = false };

            IDuplicateReaderFactory readerFactory = this.ArrangeReaderFactoryForGroups(
                this.ArrangeGroup(goodFile1, goodFile2, stagedFile));

            IFileModifier fileModifier = Substitute.For<IFileModifier>();

            DuplicateProcessor uut = this.DuplicateProcessorWithDefaultMocks(
                config: config,
                readerFactory: readerFactory,
                fileModifier: fileModifier);

            // Act
            uut.ProcessDuplicates();

            // Assert
            fileModifier.Received(1).Delete(goodFile1);
            fileModifier.Received(1).Delete(goodFile2);
            fileModifier.DidNotReceive().Move(Arg.Any<GroupFile>(), Arg.Any<string>());
        }


        private DuplicateProcessor DuplicateProcessorWithDefaultMocks(
            IDuplicateReaderFactory readerFactory = null,
            IFileModifier fileModifier = null,
            IDuplicateHandlerConfiguration config = null,
            IFileSystem fileSystem = null)
        {
            if (fileSystem == null)
            {
                FileSystem fs = new FileSystem();

                fileSystem = Substitute.For<IFileSystem>();
                fileSystem.Path.Returns(Substitute.For<PathBase>());

                fileSystem.Path.GetDirectoryName(Arg.Any<string>())
                    .Returns(callInfo => fs.Path.GetDirectoryName(callInfo.Arg<string>()));
                fileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
                    .Returns(callInfo => fs.Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));
            }

            return new DuplicateProcessor(
                readerFactory: readerFactory ?? Substitute.For<IDuplicateReaderFactory>(),
                config: config ?? Substitute.For<IDuplicateHandlerConfiguration>(),
                fileSystem: fileSystem,
                fileModifier: fileModifier ?? Substitute.For<IFileModifier>());
        }


        private IDuplicateReaderFactory ArrangeReaderFactoryForGroups(params DuplicateGroup[] groups)
        {
            IDuplicateReader reader = Substitute.For<IDuplicateReader>();
            reader.GetEnumerator().Returns(new List<DuplicateGroup>(groups).GetEnumerator());

            IDuplicateReaderFactory readerFactory = Substitute.For<IDuplicateReaderFactory>();
            readerFactory.CreateReader().Returns(reader);

            return readerFactory;
        }


        private DuplicateGroup ArrangeGroup(params GroupFile[] files)
        {
            return new DuplicateGroup()
            {
                Files = new List<GroupFile>(files)
            };
        }
    }
}
