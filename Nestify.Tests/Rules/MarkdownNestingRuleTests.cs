using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Rules;
using System.Collections.Generic;

namespace Nestify.Tests.Rules
{
    [TestClass]
    public class MarkdownNestingRuleTests
    {
        private MarkdownNestingRule _rule;

        [TestInitialize]
        public void Setup()
        {
            _rule = new MarkdownNestingRule();
        }

        [TestMethod]
        [DataRow("README.md")]
        [DataRow("CHANGELOG.md")]
        [DataRow("Documentation.md")]
        public void CanHandle_MarkdownFile_ReturnsTrue(string fileName)
        {
            Assert.IsTrue(_rule.CanHandle(fileName));
        }

        [TestMethod]
        [DataRow("README.txt")]
        [DataRow("Document.cs")]
        [DataRow("Script.js")]
        public void CanHandle_NonMarkdownFile_ReturnsFalse(string fileName)
        {
            Assert.IsFalse(_rule.CanHandle(fileName));
        }

        [TestMethod]
        public void FindParent_MarkdownWithCSharpFile_ReturnsCSharpFile()
        {
            var availableFiles = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            {
                "UserService.cs",
                "UserService.md"
            };

            var parent = _rule.FindParent("UserService.md", availableFiles);

            Assert.AreEqual("UserService.cs", parent);
        }

        [TestMethod]
        public void FindParent_MarkdownWithJavaScriptFile_ReturnsJavaScriptFile()
        {
            var availableFiles = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            {
                "account.js",
                "account.md"
            };

            var parent = _rule.FindParent("account.md", availableFiles);

            Assert.AreEqual("account.js", parent);
        }

        [TestMethod]
        public void FindParent_MarkdownWithBothCSharpAndJavaScript_PrefersCSharp()
        {
            var availableFiles = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            {
                "Service.cs",
                "Service.js",
                "Service.md"
            };

            var parent = _rule.FindParent("Service.md", availableFiles);

            Assert.AreEqual("Service.cs", parent);
        }

        [TestMethod]
        public void FindParent_MarkdownWithoutParent_ReturnsNull()
        {
            var availableFiles = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            {
                "Other.cs",
                "Document.md"
            };

            var parent = _rule.FindParent("Document.md", availableFiles);

            Assert.IsNull(parent);
        }
    }
}
