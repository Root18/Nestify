using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Rules;
using System;
using System.Collections.Generic;

namespace Nestify.Tests.Rules
{
    [TestClass]
    public class JavaScriptMinNestingRuleTests
    {
        private JavaScriptMinNestingRule _rule;

        [TestInitialize]
        public void Setup()
        {
            _rule = new JavaScriptMinNestingRule();
        }

        [TestMethod]
        public void CanHandle_MinJs_ReturnsTrue()
        {
            Assert.IsTrue(_rule.CanHandle("app.min.js"));
        }

        [TestMethod]
        public void CanHandle_MinJsCaseInsensitive_ReturnsTrue()
        {
            Assert.IsTrue(_rule.CanHandle("app.Min.JS"));
        }

        [TestMethod]
        public void CanHandle_BundleMinJs_ReturnsFalse()
        {
            Assert.IsFalse(_rule.CanHandle("app.bundle.min.js"));
        }

        [TestMethod]
        [DataRow("app.bundle.js")]
        [DataRow("app.js")]
        [DataRow("app.css")]
        public void CanHandle_NonMinJs_ReturnsFalse(string fileName)
        {
            Assert.IsFalse(_rule.CanHandle(fileName));
        }

        [TestMethod]
        public void FindParent_JsExists_ReturnsJs()
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "app.min.js",
                "app.js"
            };

            string result = _rule.FindParent("app.min.js", files);

            Assert.AreEqual("app.js", result);
        }

        [TestMethod]
        public void FindParent_JsMissing_ReturnsNull()
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "app.min.js"
            };

            string result = _rule.FindParent("app.min.js", files);

            Assert.IsNull(result);
        }
    }
}
