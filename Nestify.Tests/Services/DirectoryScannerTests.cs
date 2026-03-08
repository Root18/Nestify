using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Abstractions;
using Nestify.Services;
using System;

namespace Nestify.Tests.Services
{
    [TestClass]
    public class DirectoryScannerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullRuleEngine_ThrowsArgumentNullException()
        {
            var stubNestingService = new StubFileNestingService();
            new DirectoryScanner(null, stubNestingService);
        }

        [TestMethod]
        public void Constructor_NullRuleEngine_ExceptionContainsParamName()
        {
            var stubNestingService = new StubFileNestingService();

            try
            {
                new DirectoryScanner(null, stubNestingService);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("ruleEngine", ex.ParamName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullNestingService_ThrowsArgumentNullException()
        {
            var stubRuleEngine = new StubAutoNestRuleEngine();
            new DirectoryScanner(stubRuleEngine, null);
        }

        [TestMethod]
        public void Constructor_NullNestingService_ExceptionContainsParamName()
        {
            var stubRuleEngine = new StubAutoNestRuleEngine();

            try
            {
                new DirectoryScanner(stubRuleEngine, null);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("nestingService", ex.ParamName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_BothNull_ThrowsArgumentNullException()
        {
            new DirectoryScanner(null, null);
        }

        [TestMethod]
        public void Constructor_ValidArguments_CreatesInstance()
        {
            var stubRuleEngine = new StubAutoNestRuleEngine();
            var stubNestingService = new StubFileNestingService();

            var scanner = new DirectoryScanner(stubRuleEngine, stubNestingService);

            Assert.IsNotNull(scanner);
        }

        [TestMethod]
        public void Constructor_ValidArguments_ImplementsIDirectoryScanner()
        {
            var stubRuleEngine = new StubAutoNestRuleEngine();
            var stubNestingService = new StubFileNestingService();

            var scanner = new DirectoryScanner(stubRuleEngine, stubNestingService);

            Assert.IsInstanceOfType(scanner, typeof(IDirectoryScanner));
        }

        private class StubAutoNestRuleEngine : IAutoNestRuleEngine
        {
            public string FindParent(string fileName, System.Collections.Generic.HashSet<string> availableFiles) => null;
        }

        private class StubFileNestingService : IFileNestingService
        {
            public void NestFile(EnvDTE.ProjectItem childItem, EnvDTE.ProjectItem parentItem, Microsoft.VisualStudio.Shell.Interop.IVsHierarchy hierarchy, Microsoft.VisualStudio.Shell.Interop.IVsBuildPropertyStorage storage) { }
            public void UnnestFile(EnvDTE.ProjectItem childItem, Microsoft.VisualStudio.Shell.Interop.IVsHierarchy hierarchy, Microsoft.VisualStudio.Shell.Interop.IVsBuildPropertyStorage storage) { }
        }
    }
}
