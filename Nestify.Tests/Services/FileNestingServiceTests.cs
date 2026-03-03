using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Abstractions;
using Nestify.Services;

namespace Nestify.Tests.Services
{
    [TestClass]
    public class FileNestingServiceTests
    {
        [TestMethod]
        public void Constructor_CreatesInstance()
        {
            var service = new FileNestingService();

            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Constructor_ImplementsIFileNestingService()
        {
            var service = new FileNestingService();

            Assert.IsInstanceOfType(service, typeof(IFileNestingService));
        }
    }
}
