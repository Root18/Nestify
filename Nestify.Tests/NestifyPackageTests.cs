using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Nestify.Tests
{
    [TestClass]
    public class NestifyPackageTests
    {
        [TestMethod]
        public void PackageGuidString_IsValidGuid()
        {
            bool isValid = Guid.TryParse(NestifyPackage.PackageGuidString, out _);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void PackageGuidString_IsNotEmpty()
        {
            var guid = new Guid(NestifyPackage.PackageGuidString);

            Assert.AreNotEqual(Guid.Empty, guid);
        }

        [TestMethod]
        public void Class_InheritsFromAsyncPackage()
        {
            Assert.IsTrue(typeof(AsyncPackage).IsAssignableFrom(typeof(NestifyPackage)));
        }
    }
}
