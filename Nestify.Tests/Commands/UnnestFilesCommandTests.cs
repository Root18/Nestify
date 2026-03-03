using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Commands;
using System;

namespace Nestify.Tests.Commands
{
    [TestClass]
    public class UnnestFilesCommandTests
    {
        [TestMethod]
        public void CommandSet_IsValidGuid()
        {
            Guid commandSet = UnnestFilesCommand.CommandSet;

            Assert.AreNotEqual(Guid.Empty, commandSet);
        }

        [TestMethod]
        public void CommandId_IsNonZero()
        {
            Assert.AreNotEqual(0, UnnestFilesCommand.CommandId);
        }

        [TestMethod]
        public void Instance_IsNullBeforeInitialization()
        {
            Assert.IsNull(UnnestFilesCommand.Instance);
        }

        [TestMethod]
        public void CommandId_IsDistinctFromNestFilesCommand()
        {
            Assert.AreNotEqual(NestFilesCommand.CommandId, UnnestFilesCommand.CommandId);
        }

        [TestMethod]
        public void CommandId_IsDistinctFromAutoNestCommand()
        {
            Assert.AreNotEqual(AutoNestCommand.CommandId, UnnestFilesCommand.CommandId);
        }
    }
}
