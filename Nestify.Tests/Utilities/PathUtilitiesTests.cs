using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Utilities;

namespace Nestify.Tests.Utilities
{
    [TestClass]
    public class PathUtilitiesTests
    {
        [TestMethod]
        public void GetRelativePath_FullPathUnderBase_ReturnsRelative()
        {
            string result = PathUtilities.GetRelativePath(
                @"C:\Projects\MyApp",
                @"C:\Projects\MyApp\Models\User.cs");

            Assert.AreEqual(@"Models\User.cs", result);
        }

        [TestMethod]
        public void GetRelativePath_BaseWithTrailingSlash_ReturnsRelative()
        {
            string result = PathUtilities.GetRelativePath(
                @"C:\Projects\MyApp\",
                @"C:\Projects\MyApp\Models\User.cs");

            Assert.AreEqual(@"Models\User.cs", result);
        }

        [TestMethod]
        public void GetRelativePath_FileDirectlyInBase_ReturnsFileName()
        {
            string result = PathUtilities.GetRelativePath(
                @"C:\Projects\MyApp",
                @"C:\Projects\MyApp\Program.cs");

            Assert.AreEqual("Program.cs", result);
        }

        [TestMethod]
        public void GetRelativePath_DifferentBase_ReturnsFileNameOnly()
        {
            string result = PathUtilities.GetRelativePath(
                @"C:\Other\Path",
                @"C:\Projects\MyApp\Program.cs");

            Assert.AreEqual("Program.cs", result);
        }

        [TestMethod]
        public void GetRelativePath_CaseInsensitiveBase_ReturnsRelative()
        {
            string result = PathUtilities.GetRelativePath(
                @"C:\PROJECTS\MYAPP",
                @"C:\Projects\MyApp\Models\User.cs");

            Assert.AreEqual(@"Models\User.cs", result);
        }
    }
}
