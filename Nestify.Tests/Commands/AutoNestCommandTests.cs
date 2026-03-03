using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Commands;
using System;

namespace Nestify.Tests.Commands
{
    [TestClass]
    public class AutoNestCommandTests
    {
        [TestMethod]
        public void CommandSet_IsValidGuid()
        {
            Guid commandSet = AutoNestCommand.CommandSet;

            Assert.AreNotEqual(Guid.Empty, commandSet);
        }

        [TestMethod]
        public void CommandId_IsNonZero()
        {
            Assert.AreNotEqual(0, AutoNestCommand.CommandId);
        }

        [TestMethod]
        public void Instance_IsNullBeforeInitialization()
        {
            Assert.IsNull(AutoNestCommand.Instance);
        }

        [TestMethod]
        public void CommandId_IsDistinctFromNestFilesCommand()
        {
            Assert.AreNotEqual(NestFilesCommand.CommandId, AutoNestCommand.CommandId);
        }

        [TestMethod]
        public void CommandId_IsDistinctFromUnnestFilesCommand()
        {
            Assert.AreNotEqual(UnnestFilesCommand.CommandId, AutoNestCommand.CommandId);
        }

        [TestMethod]
        public void CommandSet_MatchesSharedCommandSet()
        {
            Assert.AreEqual(NestFilesCommand.CommandSet, AutoNestCommand.CommandSet);
            Assert.AreEqual(UnnestFilesCommand.CommandSet, AutoNestCommand.CommandSet);
        }
    }
}
