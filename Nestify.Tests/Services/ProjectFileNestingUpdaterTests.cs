using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Services;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Nestify.Tests.Services
{
    [TestClass]
    public class ProjectFileNestingUpdaterTests
    {
        private const string MsBuildNs = "http://schemas.microsoft.com/developer/msbuild/2003";

        private string _tempDir;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "NestifyTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        private string WriteProject(string extension, string content)
        {
            var path = Path.Combine(_tempDir, "Test" + extension);
            File.WriteAllText(path, content);
            return path;
        }

        [TestMethod]
        [DataRow(".csproj", false)]
        [DataRow(".esproj", false)]
        [DataRow(".njsproj", false)]
        [DataRow(".vcxproj", false)]
        [DataRow(".shproj", false)]
        [DataRow(".sqlproj", false)]
        [DataRow(".vbproj", true)]
        [DataRow(".fsproj", true)]
        [DataRow(".pyproj", true)]
        public void ShouldUseDirectProjectFileFallback_ByProjectType(string extension, bool expected)
        {
            Assert.AreEqual(expected,
                ProjectFileNestingUpdater.ShouldUseDirectProjectFileFallback("Test" + extension));
        }

        [TestMethod]
        public void UpdateNesting_AddsDependentUpon_ToExistingIncludeItem()
        {
            var projectPath = WriteProject(".vbproj", $"""
                <Project xmlns="{MsBuildNs}">
                  <ItemGroup>
                    <None Include="Readme.md" />
                  </ItemGroup>
                </Project>
                """);

            ProjectFileNestingUpdater.UpdateNesting(
                projectPath, Path.Combine(_tempDir, "Readme.md"), "Program.vb", isNesting: true);

            var doc = XDocument.Load(projectPath);
            XNamespace ns = MsBuildNs;
            var item = doc.Root.Descendants(ns + "None").Single();
            Assert.AreEqual("Program.vb", item.Element(ns + "DependentUpon")?.Value);
        }

        [TestMethod]
        public void UpdateNesting_UsesExistingUpdateItem_InsteadOfCreatingDuplicate()
        {
            var projectPath = WriteProject(".vbproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <None Update="Readme.md" />
                  </ItemGroup>
                </Project>
                """);

            ProjectFileNestingUpdater.UpdateNesting(
                projectPath, Path.Combine(_tempDir, "Readme.md"), "Program.vb", isNesting: true);

            var doc = XDocument.Load(projectPath);
            var items = doc.Root.Descendants("None").ToList();
            Assert.AreEqual(1, items.Count, "an Update item must be reused, not duplicated");
            Assert.AreEqual("Readme.md", items[0].Attribute("Update")?.Value);
            Assert.AreEqual("Program.vb", items[0].Element("DependentUpon")?.Value);
        }

        [TestMethod]
        public void UpdateNesting_CreatesItem_WhenNoneExists()
        {
            var projectPath = WriteProject(".vbproj", $"""
                <Project xmlns="{MsBuildNs}">
                  <ItemGroup>
                  </ItemGroup>
                </Project>
                """);

            ProjectFileNestingUpdater.UpdateNesting(
                projectPath, Path.Combine(_tempDir, "Readme.md"), "Program.vb", isNesting: true);

            var doc = XDocument.Load(projectPath);
            XNamespace ns = MsBuildNs;
            var item = doc.Root.Descendants(ns + "None").Single();
            Assert.AreEqual("Readme.md", item.Attribute("Include")?.Value);
            Assert.AreEqual("Program.vb", item.Element(ns + "DependentUpon")?.Value);
        }

        [TestMethod]
        public void UpdateNesting_Unnest_RemovesDependentUpon()
        {
            var projectPath = WriteProject(".vbproj", $"""
                <Project xmlns="{MsBuildNs}">
                  <ItemGroup>
                    <None Include="Readme.md">
                      <DependentUpon>Program.vb</DependentUpon>
                    </None>
                  </ItemGroup>
                </Project>
                """);

            ProjectFileNestingUpdater.UpdateNesting(
                projectPath, Path.Combine(_tempDir, "Readme.md"), null, isNesting: false);

            var doc = XDocument.Load(projectPath);
            XNamespace ns = MsBuildNs;
            var item = doc.Root.Descendants(ns + "None").Single();
            Assert.IsNull(item.Element(ns + "DependentUpon"));
        }

        [TestMethod]
        public void UpdateNesting_Unnest_WithoutExistingItem_DoesNotCreateOne()
        {
            var projectPath = WriteProject(".vbproj", $"""
                <Project xmlns="{MsBuildNs}">
                  <ItemGroup>
                  </ItemGroup>
                </Project>
                """);

            ProjectFileNestingUpdater.UpdateNesting(
                projectPath, Path.Combine(_tempDir, "Readme.md"), null, isNesting: false);

            var doc = XDocument.Load(projectPath);
            XNamespace ns = MsBuildNs;
            Assert.IsFalse(doc.Root.Descendants(ns + "None").Any());
        }

        [TestMethod]
        public void UpdateNesting_NodeJsProject_ConvertsIncludeItemToContent()
        {
            var projectPath = WriteProject(".njsproj", $"""
                <Project xmlns="{MsBuildNs}">
                  <ItemGroup>
                    <None Include="app.min.js" />
                  </ItemGroup>
                </Project>
                """);

            ProjectFileNestingUpdater.UpdateNesting(
                projectPath, Path.Combine(_tempDir, "app.min.js"), "app.js", isNesting: true);

            var doc = XDocument.Load(projectPath);
            XNamespace ns = MsBuildNs;
            Assert.IsFalse(doc.Root.Descendants(ns + "None").Any());
            var item = doc.Root.Descendants(ns + "Content").Single();
            Assert.AreEqual("app.min.js", item.Attribute("Include")?.Value);
            Assert.AreEqual("app.js", item.Element(ns + "DependentUpon")?.Value);
        }

        [TestMethod]
        public void UpdateNesting_MatchesPaths_CaseInsensitively_AndAcrossSlashStyles()
        {
            var subDir = Path.Combine(_tempDir, "Docs");
            Directory.CreateDirectory(subDir);
            var projectPath = WriteProject(".vbproj", $"""
                <Project xmlns="{MsBuildNs}">
                  <ItemGroup>
                    <None Include="docs/README.MD" />
                  </ItemGroup>
                </Project>
                """);

            ProjectFileNestingUpdater.UpdateNesting(
                projectPath, Path.Combine(subDir, "Readme.md"), "Program.vb", isNesting: true);

            var doc = XDocument.Load(projectPath);
            XNamespace ns = MsBuildNs;
            var item = doc.Root.Descendants(ns + "None").Single();
            Assert.AreEqual("Program.vb", item.Element(ns + "DependentUpon")?.Value);
        }

        [TestMethod]
        public void UpdateNesting_ReusesItemOfAnyItemType_InsteadOfCreatingDuplicate()
        {
            var projectPath = WriteProject(".vbproj", $"""
                <Project xmlns="{MsBuildNs}">
                  <ItemGroup>
                    <EmbeddedResource Include="Strings.resx" />
                  </ItemGroup>
                </Project>
                """);

            ProjectFileNestingUpdater.UpdateNesting(
                projectPath, Path.Combine(_tempDir, "Strings.resx"), "Program.vb", isNesting: true);

            var doc = XDocument.Load(projectPath);
            XNamespace ns = MsBuildNs;
            Assert.IsFalse(doc.Root.Descendants(ns + "None").Any(),
                "an existing item of another item type must be reused, not duplicated as None");
            var item = doc.Root.Descendants(ns + "EmbeddedResource").Single();
            Assert.AreEqual("Program.vb", item.Element(ns + "DependentUpon")?.Value);
        }

        [TestMethod]
        public void UpdateNesting_NewItem_IsNotAppendedToUnrelatedItemGroup()
        {
            var projectPath = WriteProject(".vbproj", $"""
                <Project xmlns="{MsBuildNs}">
                  <ItemGroup>
                    <Reference Include="System.Xml" />
                  </ItemGroup>
                </Project>
                """);

            ProjectFileNestingUpdater.UpdateNesting(
                projectPath, Path.Combine(_tempDir, "Readme.md"), "Program.vb", isNesting: true);

            var doc = XDocument.Load(projectPath);
            XNamespace ns = MsBuildNs;
            var item = doc.Root.Descendants(ns + "None").Single();
            Assert.AreEqual("Readme.md", item.Attribute("Include")?.Value);
            Assert.IsFalse(item.Parent.Elements(ns + "Reference").Any(),
                "new items must not be mixed into unrelated item groups");
        }

        [TestMethod]
        public void UpdateNesting_RemovesDependentUpon_FromDuplicateItems()
        {
            var projectPath = WriteProject(".vbproj", $"""
                <Project xmlns="{MsBuildNs}">
                  <ItemGroup>
                    <Compile Include="Readme.md">
                      <DependentUpon>Old.vb</DependentUpon>
                    </Compile>
                    <None Include="Readme.md" />
                  </ItemGroup>
                </Project>
                """);

            ProjectFileNestingUpdater.UpdateNesting(
                projectPath, Path.Combine(_tempDir, "Readme.md"), "Program.vb", isNesting: true);

            var doc = XDocument.Load(projectPath);
            XNamespace ns = MsBuildNs;
            var noneItem = doc.Root.Descendants(ns + "None").Single();
            var compileItem = doc.Root.Descendants(ns + "Compile").Single();
            Assert.AreEqual("Program.vb", noneItem.Element(ns + "DependentUpon")?.Value);
            Assert.IsNull(compileItem.Element(ns + "DependentUpon"),
                "stale DependentUpon entries on duplicate items must be removed");
        }
    }
}
