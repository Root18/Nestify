using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Abstractions;
using Nestify.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nestify.Tests.Services
{
    [TestClass]
    public class SiblingFileProviderTests
    {
        private string _testDir;
        private SiblingFileProvider _provider;

        [TestInitialize]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "NestifyTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
            _provider = new SiblingFileProvider(new FileValidator());
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, recursive: true);
        }

        [TestMethod]
        public void GetSiblingFiles_ReturnsSupportedFilesExcludingNames()
        {
            File.WriteAllText(Path.Combine(_testDir, "IService.cs"), "");
            File.WriteAllText(Path.Combine(_testDir, "Service.cs"), "");
            File.WriteAllText(Path.Combine(_testDir, "readme.txt"), "");

            var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Service.cs" };

            List<string> result = _provider.GetSiblingFiles(_testDir, exclude);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("IService.cs", result[0]);
        }

        [TestMethod]
        public void GetSiblingFiles_ExcludesConfigFiles()
        {
            File.WriteAllText(Path.Combine(_testDir, "app.component.ts"), "");
            File.WriteAllText(Path.Combine(_testDir, "package.json"), "");
            File.WriteAllText(Path.Combine(_testDir, "tsconfig.json"), "");

            var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            List<string> result = _provider.GetSiblingFiles(_testDir, exclude);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("app.component.ts", result[0]);
        }

        [TestMethod]
        public void GetSiblingFiles_ReturnsSortedResults()
        {
            File.WriteAllText(Path.Combine(_testDir, "Zebra.cs"), "");
            File.WriteAllText(Path.Combine(_testDir, "Alpha.cs"), "");
            File.WriteAllText(Path.Combine(_testDir, "Middle.cs"), "");

            var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            List<string> result = _provider.GetSiblingFiles(_testDir, exclude);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Alpha.cs", result[0]);
            Assert.AreEqual("Middle.cs", result[1]);
            Assert.AreEqual("Zebra.cs", result[2]);
        }

        [TestMethod]
        public void GetSiblingFiles_NonExistentDirectory_ReturnsEmptyList()
        {
            var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            List<string> result = _provider.GetSiblingFiles(@"C:\NonExistent_" + Guid.NewGuid(), exclude);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetSiblingFiles_OnlyUnsupportedFiles_ReturnsEmptyList()
        {
            File.WriteAllText(Path.Combine(_testDir, "readme.txt"), "");
            File.WriteAllText(Path.Combine(_testDir, "image.png"), "");

            var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            List<string> result = _provider.GetSiblingFiles(_testDir, exclude);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetSiblingFiles_EmptyDirectory_ReturnsEmptyList()
        {
            var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            List<string> result = _provider.GetSiblingFiles(_testDir, exclude);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullValidator_ThrowsArgumentNullException()
        {
            new SiblingFileProvider(null);
        }
    }
}
