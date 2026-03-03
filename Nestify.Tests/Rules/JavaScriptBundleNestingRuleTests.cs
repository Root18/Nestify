using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Rules;
using System;
using System.Collections.Generic;

namespace Nestify.Tests.Rules
{
    [TestClass]
    public class JavaScriptBundleNestingRuleTests
    {
        private JavaScriptBundleNestingRule _rule;

        [TestInitialize]
        public void Setup()
        {
            _rule = new JavaScriptBundleNestingRule();
        }

        [TestMethod]
        public void CanHandle_BundleJs_ReturnsTrue()
        {
            Assert.IsTrue(_rule.CanHandle("app.bundle.js"));
        }

        [TestMethod]
        public void CanHandle_BundleJsCaseInsensitive_ReturnsTrue()
        {
            Assert.IsTrue(_rule.CanHandle("app.Bundle.JS"));
        }

        [TestMethod]
        public void CanHandle_BundleMinJs_ReturnsFalse()
        {
            Assert.IsFalse(_rule.CanHandle("app.bundle.min.js"));
        }

        [TestMethod]
        [DataRow("app.min.js")]
        [DataRow("app.js")]
        [DataRow("app.ts")]
        public void CanHandle_NonBundleJs_ReturnsFalse(string fileName)
        {
            Assert.IsFalse(_rule.CanHandle(fileName));
        }

        [TestMethod]
        public void FindParent_JsExists_ReturnsJs()
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "app.bundle.js",
                "app.js"
            };

            string result = _rule.FindParent("app.bundle.js", files);

            Assert.AreEqual("app.js", result);
        }

        [TestMethod]
        public void FindParent_JsMissing_ReturnsNull()
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "app.bundle.js"
            };

            string result = _rule.FindParent("app.bundle.js", files);

            Assert.IsNull(result);
        }
    }
}
