using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Core;
using System;
using System.Linq;
using VsDiffDuplicateHandler.Models;
using VsDiffDuplicateHandler.Services;
using VsDiffDuplicateHandler.Services.Interfaces;
using Xunit;

namespace VsDiffDuplicateHandlerTest.Services
{
    public class FileModifierTest
    {
        [Fact]
        public void DeleteLogsSuccessfulDeletion()
        {
            // Arrange
            GroupFile file = new GroupFile();
            ILogger<FileModifier> logger = Substitute.For<ILogger<FileModifier>>();

            FileModifier uut = this.FileModifierWithDefaultMocks(
                logger: logger);

            // Act
            uut.Delete(file);

            // Assert
            ICall loggerCall = Assert.Single(logger.ReceivedCalls());
            Assert.Equal("Log", loggerCall.GetMethodInfo().Name);
            Assert.Equal(LogLevel.Information, loggerCall.GetOriginalArguments().First());
        }


        [Fact]
        public void DeleteLogsFailedDeletion()
        {
            // Arrange
            GroupFile file = new GroupFile();
            ILogger<FileModifier> logger = Substitute.For<ILogger<FileModifier>>();
            IFileOperationsAbstraction ops = Substitute.For<IFileOperationsAbstraction>();
            ops.When((x) => x.DeleteFile(Arg.Any<string>()))
                .Do((callInfo) => throw new Exception());

            FileModifier uut = this.FileModifierWithDefaultMocks(
                logger: logger,
                fileOps: ops);

            // Act
            uut.Delete(file);

            // Assert
            ICall loggerCall = Assert.Single(logger.ReceivedCalls());
            Assert.Equal("Log", loggerCall.GetMethodInfo().Name);
            Assert.Equal(LogLevel.Error, loggerCall.GetOriginalArguments().First());
        }


        [Fact]
        public void MoveLogsSuccessfulMove()
        {
            // Arrange
            GroupFile file = new GroupFile();
            ILogger<FileModifier> logger = Substitute.For<ILogger<FileModifier>>();

            FileModifier uut = this.FileModifierWithDefaultMocks(
                logger: logger);

            // Act
            uut.Move(file, "destination");

            // Assert
            ICall loggerCall = Assert.Single(logger.ReceivedCalls());
            Assert.Equal("Log", loggerCall.GetMethodInfo().Name);
            Assert.Equal(LogLevel.Information, loggerCall.GetOriginalArguments().First());

        }


        [Fact]
        public void MoveLogsFailedMove()
        {
            // Arrange
            GroupFile file = new GroupFile();
            ILogger<FileModifier> logger = Substitute.For<ILogger<FileModifier>>();
            IFileOperationsAbstraction ops = Substitute.For<IFileOperationsAbstraction>();
            ops.When((x) => x.MoveFile(Arg.Any<string>(), Arg.Any<string>()))
                .Do((callInfo) => throw new Exception());

            FileModifier uut = this.FileModifierWithDefaultMocks(
                logger: logger,
                fileOps: ops);

            // Act
            uut.Move(file, "destination");

            // Assert
            ICall loggerCall = Assert.Single(logger.ReceivedCalls());
            Assert.Equal("Log", loggerCall.GetMethodInfo().Name);
            Assert.Equal(LogLevel.Error, loggerCall.GetOriginalArguments().First());
        }


        private FileModifier FileModifierWithDefaultMocks(
            ILogger<FileModifier> logger = null,
            IFileOperationsAbstraction fileOps = null)
        {
            return new FileModifier(
                logger: logger ?? Substitute.For<ILogger<FileModifier>>(),
                fileOps: fileOps ?? Substitute.For<IFileOperationsAbstraction>());
        }
    }
}
