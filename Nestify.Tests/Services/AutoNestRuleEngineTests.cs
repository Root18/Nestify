using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Abstractions;
using Nestify.Services;
using System;
using System.Collections.Generic;

namespace Nestify.Tests.Services
{
    [TestClass]
    public class AutoNestRuleEngineTests
    {
        [TestMethod]
        public void FindParent_MatchingRule_ReturnsParent()
        {
            var stubRule = new StubNestingRule(canHandle: true, parent: "IService.cs");
            var engine = new AutoNestRuleEngine(new INestingRule[] { stubRule });
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "IService.cs", "Service.cs" };

            string result = engine.FindParent("Service.cs", files);

            Assert.AreEqual("IService.cs", result);
        }

        [TestMethod]
        public void FindParent_NoMatchingRule_ReturnsNull()
        {
            var stubRule = new StubNestingRule(canHandle: false, parent: null);
            var engine = new AutoNestRuleEngine(new INestingRule[] { stubRule });
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "readme.txt" };

            string result = engine.FindParent("readme.txt", files);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindParent_NoRules_ReturnsNull()
        {
            var engine = new AutoNestRuleEngine(new INestingRule[0]);
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "file.cs" };

            string result = engine.FindParent("file.cs", files);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindParent_FirstRuleHandlesButReturnsNull_TriesSecondRule()
        {
            var rule1 = new StubNestingRule(canHandle: true, parent: null);
            var rule2 = new StubNestingRule(canHandle: true, parent: "parent.js");
            var engine = new AutoNestRuleEngine(new INestingRule[] { rule1, rule2 });
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "file.js", "parent.js" };

            string result = engine.FindParent("file.js", files);

            Assert.AreEqual("parent.js", result);
        }

        [TestMethod]
        public void FindParent_FirstRuleMatches_DoesNotTrySecondRule()
        {
            var rule1 = new StubNestingRule(canHandle: true, parent: "first.cs");
            var rule2 = new StubNestingRule(canHandle: true, parent: "second.cs");
            var engine = new AutoNestRuleEngine(new INestingRule[] { rule1, rule2 });
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "file.cs" };

            string result = engine.FindParent("file.cs", files);

            Assert.AreEqual("first.cs", result);
        }

        [TestMethod]
        public void FindParent_RuleCannotHandle_SkipsRule()
        {
            var rule1 = new StubNestingRule(canHandle: false, parent: "wrong.cs");
            var rule2 = new StubNestingRule(canHandle: true, parent: "correct.cs");
            var engine = new AutoNestRuleEngine(new INestingRule[] { rule1, rule2 });
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "file.cs" };

            string result = engine.FindParent("file.cs", files);

            Assert.AreEqual("correct.cs", result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullRules_ThrowsArgumentNullException()
        {
            new AutoNestRuleEngine(null);
        }

        private class StubNestingRule : INestingRule
        {
            private readonly bool _canHandle;
            private readonly string _parent;

            public StubNestingRule(bool canHandle, string parent)
            {
                _canHandle = canHandle;
                _parent = parent;
            }

            public bool CanHandle(string fileName) => _canHandle;

            public string FindParent(string fileName, HashSet<string> availableFiles) => _parent;
        }
    }
}
