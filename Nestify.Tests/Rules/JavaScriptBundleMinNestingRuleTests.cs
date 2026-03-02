using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Rules;
using System;
using System.Collections.Generic;

namespace Nestify.Tests.Rules
{
    [TestClass]
    public class JavaScriptBundleMinNestingRuleTests
    {
        private JavaScriptBundleMinNestingRule _rule;

        [TestInitialize]
        public void Setup()
        {
            _rule = new JavaScriptBundleMinNestingRule();
        }

        [TestMethod]
        public void CanHandle_BundleMinJs_ReturnsTrue()
        {
            Assert.IsTrue(_rule.CanHandle("app.bundle.min.js"));
        }

        [TestMethod]
        public void CanHandle_BundleMinJsCaseInsensitive_ReturnsTrue()
        {
            Assert.IsTrue(_rule.CanHandle("app.Bundle.Min.JS"));
        }

        [TestMethod]
        [DataRow("app.bundle.js")]
        [DataRow("app.min.js")]
        [DataRow("app.js")]
        [DataRow("app.cs")]
        public void CanHandle_NonBundleMinJs_ReturnsFalse(string fileName)
        {
            Assert.IsFalse(_rule.CanHandle(fileName));
        }

        [TestMethod]
        public void FindParent_BundleJsExists_ReturnsBundleJs()
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "app.bundle.min.js",
                "app.bundle.js"
            };

            string result = _rule.FindParent("app.bundle.min.js", files);

            Assert.AreEqual("app.bundle.js", result);
        }

        [TestMethod]
        public void FindParent_BundleJsMissing_ReturnsNull()
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "app.bundle.min.js",
                "app.js"
            };

            string result = _rule.FindParent("app.bundle.min.js", files);

            Assert.IsNull(result);
        }
    }
}
