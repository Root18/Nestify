using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Rules;
using System;
using System.Collections.Generic;

namespace Nestify.Tests.Rules
{
    [TestClass]
    public class CSharpInterfaceNestingRuleTests
    {
        private CSharpInterfaceNestingRule _rule;

        [TestInitialize]
        public void Setup()
        {
            _rule = new CSharpInterfaceNestingRule();
        }

        [TestMethod]
        public void CanHandle_CsFile_ReturnsTrue()
        {
            Assert.IsTrue(_rule.CanHandle("MyClass.cs"));
        }

        [TestMethod]
        public void CanHandle_CsFileCaseInsensitive_ReturnsTrue()
        {
            Assert.IsTrue(_rule.CanHandle("MyClass.CS"));
        }

        [TestMethod]
        [DataRow("script.js")]
        [DataRow("style.css")]
        [DataRow("page.html")]
        [DataRow("data.json")]
        public void CanHandle_NonCsFile_ReturnsFalse(string fileName)
        {
            Assert.IsFalse(_rule.CanHandle(fileName));
        }

        [TestMethod]
        public void FindParent_InterfaceExists_ReturnsInterfaceFileName()
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "IMyService.cs",
                "MyService.cs"
            };

            string result = _rule.FindParent("MyService.cs", files);

            Assert.AreEqual("IMyService.cs", result);
        }

        [TestMethod]
        public void FindParent_NoInterfaceExists_ReturnsNull()
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "MyService.cs",
                "OtherClass.cs"
            };

            string result = _rule.FindParent("MyService.cs", files);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindParent_InterfaceFileItself_ReturnsNull()
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "IMyService.cs",
                "MyService.cs"
            };

            string result = _rule.FindParent("IMyService.cs", files);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindParent_SingleCharacterName_NestsUnderPrefixedFile()
        {
            // "I.cs" has nameWithoutExt "I" — length 1, not treated as interface,
            // so it tries to find "II.cs" as parent
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "I.cs",
                "II.cs"
            };

            string result = _rule.FindParent("I.cs", files);

            Assert.AreEqual("II.cs", result);
        }

        [TestMethod]
        public void FindParent_StartsWithILowercase_IsNotTreatedAsInterface()
        {
            // "item.cs" starts with 'i' lowercase — not an interface pattern
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "item.cs",
                "Iitem.cs"
            };

            string result = _rule.FindParent("item.cs", files);

            Assert.AreEqual("Iitem.cs", result);
        }

        [TestMethod]
        public void FindParent_StartsWithIFollowedByLowercase_IsNotTreatedAsInterface()
        {
            // "Icon.cs" — 'I' followed by lowercase 'c', not considered an interface file
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Icon.cs",
                "IIcon.cs"
            };

            string result = _rule.FindParent("Icon.cs", files);

            Assert.AreEqual("IIcon.cs", result);
        }
    }
}
