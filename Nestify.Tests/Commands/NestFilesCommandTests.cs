using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Commands;
using System;

namespace Nestify.Tests.Commands
{
    [TestClass]
    public class NestFilesCommandTests
    {
        [TestMethod]
        public void CommandSet_IsValidGuid()
        {
            Guid commandSet = NestFilesCommand.CommandSet;

            Assert.AreNotEqual(Guid.Empty, commandSet);
        }

        [TestMethod]
        public void CommandId_IsNonZero()
        {
            Assert.AreNotEqual(0, NestFilesCommand.CommandId);
        }

        [TestMethod]
        public void Instance_IsNullBeforeInitialization()
        {
            Assert.IsNull(NestFilesCommand.Instance);
        }

        [TestMethod]
        public void CommandId_IsDistinctFromUnnestFilesCommand()
        {
            Assert.AreNotEqual(UnnestFilesCommand.CommandId, NestFilesCommand.CommandId);
        }

        [TestMethod]
        public void CommandId_IsDistinctFromAutoNestCommand()
        {
            Assert.AreNotEqual(AutoNestCommand.CommandId, NestFilesCommand.CommandId);
        }
    }
}
